using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public class ParticipationPresenter : IParticipationPresenter
    {
        private readonly IParticipationView     _view;
        private readonly MemberThrowDataSO      _memberThrowDataSO;
        private readonly ScheduleRegistrationUI _concertRegistrationUI;
        private readonly ScheduleRegistrationUI _songRegistrationUI;
        private readonly ScheduleRegistrationUI _partTimeRegistrationUI;

        private const int TopSlotMax    = 3;
        private const int BottomSlotMax = 2;

        private enum SlotArea { Top, Bottom }

        private readonly Dictionary<MemberType, SlotArea> _currentSlots     = new();
        private readonly HashSet<MemberType>              _processingClicks  = new();
        private List<MemberDataSO> _memberDataList;

        public int CurrentSlotCount => _currentSlots.Count;
        public event Action OnSlotChanged;

        public ParticipationPresenter(
            IParticipationView view,
            MemberThrowDataSO memberThrowDataSO,
            ScheduleRegistrationUI concertRegistrationUI,
            ScheduleRegistrationUI songRegistrationUI,
            ScheduleRegistrationUI partTimeRegistrationUI)
        {
            _view                   = view;
            _memberThrowDataSO      = memberThrowDataSO;
            _concertRegistrationUI  = concertRegistrationUI;
            _songRegistrationUI     = songRegistrationUI;
            _partTimeRegistrationUI = partTimeRegistrationUI;

            _view.OnMemberClicked       += HandleMemberClicked;
            _view.OnSlotCancelClicked   += HandleSlotCancelClicked;
            _view.OnMemberDroppedToSlot += HandleMemberDroppedToSlot;

            if (MemberConditionManager.Instance != null)
                MemberConditionManager.Instance.OnConditionChanged += HandleConditionChanged;
        }

        public void Initialize(List<MemberDataSO> memberDataList)
        {
            _memberDataList = memberDataList;
            Refresh();
        }

        public void Refresh()
        {
            if (_memberDataList == null) return;

            var busyMembers = new HashSet<MemberType>();

            foreach (var kvp in _memberThrowDataSO.ScheduledMembers)
            {
                if (kvp.Key == _memberThrowDataSO.CurrentActivity) continue;
                foreach (var m in kvp.Value)
                    busyMembers.Add(m.memberType);
            }

            foreach (var currentSlotMember in _currentSlots.Keys)
                busyMembers.Add(currentSlotMember);

            foreach (var data in _memberDataList)
            {
                if (data.IconSprite != null)
                    _view.SetMemberIcon(data.memberType, data.IconSprite);

                MemberConditionMode currentConditionMode = MemberConditionManager.Instance != null
                    ? MemberConditionManager.Instance.GetCondition(data.memberType)
                    : data.currentmod;

                var matched = data.memberConditionModes.FirstOrDefault(m => m.mode == currentConditionMode);
                if (matched != null)
                    _view.SetConditionIcon(data.memberType, matched.icon);

                bool isBusy = busyMembers.Contains(data.memberType);
                _view.SetMemberVisible(data.memberType, !isBusy);
            }
        }

        private void HandleConditionChanged(MemberType memberType, MemberConditionMode oldMode, MemberConditionMode newMode)
        {
            if (_memberDataList == null) return;

            var data = _memberDataList.Find(d => d.memberType == memberType);
            if (data == null) return;

            data.currentmod = newMode;

            var matched = data.memberConditionModes.FirstOrDefault(m => m.mode == newMode);
            if (matched != null)
                _view.SetConditionIcon(memberType, matched.icon);
        }

        private void HandleMemberClicked(MemberType memberType)
        {
            if (_processingClicks.Contains(memberType)) return;
            if (_currentSlots.ContainsKey(memberType)) return;

            var data = _memberDataList?.Find(d => d.memberType == memberType);
            if (data == null) return;

            if (!TryGetAvailableSlot(out var area)) return;

            _processingClicks.Add(memberType);
            _currentSlots.Add(memberType, area);
            _memberThrowDataSO.AddCurrentMember(data);
            _view.AddMemberToSlot(memberType, data.IconSprite, data.memberName, area == SlotArea.Top);
            Bus<TutorialMemberAssignedEvent>.Raise(new TutorialMemberAssignedEvent(
                _memberThrowDataSO.CurrentActivity,
                memberType,
                area == SlotArea.Top));

            Refresh();

            _processingClicks.Remove(memberType);
            OnSlotChanged?.Invoke();
        }

        private void HandleMemberDroppedToSlot(MemberType memberType, bool isTop)
        {
            if (_processingClicks.Contains(memberType)) return;
            if (_currentSlots.ContainsKey(memberType)) return;

            var data = _memberDataList?.Find(d => d.memberType == memberType);
            if (data == null) return;

            var targetArea = isTop ? SlotArea.Top : SlotArea.Bottom;
            int count = _currentSlots.Values.Count(e => e == targetArea);
            int max   = isTop ? TopSlotMax : BottomSlotMax;

            if (count >= max)
            {
                if (!TryGetAvailableSlot(out targetArea)) return;
                isTop = targetArea == SlotArea.Top;
            }

            _processingClicks.Add(memberType);
            _currentSlots.Add(memberType, targetArea);
            _memberThrowDataSO.AddCurrentMember(data);
            _view.AddMemberToSlot(memberType, data.IconSprite, data.memberName, isTop);
            Bus<TutorialMemberAssignedEvent>.Raise(new TutorialMemberAssignedEvent(
                _memberThrowDataSO.CurrentActivity,
                memberType,
                isTop));

            Refresh();

            _processingClicks.Remove(memberType);
            OnSlotChanged?.Invoke();
        }

        private void HandleSlotCancelClicked(MemberType memberType)
        {
            if (!_currentSlots.ContainsKey(memberType)) return;

            var data = _memberDataList?.Find(d => d.memberType == memberType);
            if (data == null) return;

            bool wasTop = _currentSlots[memberType] == SlotArea.Top;

            _currentSlots.Remove(memberType);
            _memberThrowDataSO.RemoveCurrentMember(data);
            _view.RemoveMemberFromSlot(memberType);
            
            Bus<TutorialMemberAssignedEvent>.Raise(new TutorialMemberAssignedEvent(
                _memberThrowDataSO.CurrentActivity,
                memberType,
                wasTop));

            Refresh();
            OnSlotChanged?.Invoke();
        }

        public void ClearCurrent()
        {
            var currentActivity = _memberThrowDataSO.CurrentActivity;
            bool hadAny = _currentSlots.Count > 0;

            foreach (var type in new List<MemberType>(_currentSlots.Keys))
                _view.RemoveMemberFromSlot(type);

            _currentSlots.Clear();
            _memberThrowDataSO.ClearCurrent();

            if (hadAny)
            {
                Bus<TutorialMemberAssignedEvent>.Raise(new TutorialMemberAssignedEvent(
                    currentActivity,
                    default,
                    false));
            }

            Refresh();
            OnSlotChanged?.Invoke();
        }

        public void ClearAll()
        {
            ClearCurrent();
            _memberThrowDataSO.ClearAll();
            _concertRegistrationUI?.Hide();
            _songRegistrationUI?.Hide();
            _partTimeRegistrationUI?.Hide();
            Refresh();
        }

        public void RestoreSlots(ManagementBtnType type, List<MemberDataSO> members)
        {
            foreach (var existingType in new List<MemberType>(_currentSlots.Keys))
                _view.RemoveMemberFromSlot(existingType);

            _currentSlots.Clear();
            _memberThrowDataSO.ClearCurrent();

            if (members != null)
            {
                foreach (var data in members)
                {
                    if (!TryGetAvailableSlot(out var area)) break;
                    _currentSlots.Add(data.memberType, area);
                    _view.AddMemberToSlot(data.memberType, data.IconSprite, data.memberName, area == SlotArea.Top);
                    _memberThrowDataSO.AddCurrentMember(data);
                }
            }

            Refresh();
            OnSlotChanged?.Invoke();
        }

        private bool TryGetAvailableSlot(out SlotArea area)
        {
            int topCount    = _currentSlots.Values.Count(e => e == SlotArea.Top);
            int bottomCount = _currentSlots.Values.Count(e => e == SlotArea.Bottom);

            if (topCount    < TopSlotMax)    { area = SlotArea.Top;    return true; }
            if (bottomCount < BottomSlotMax) { area = SlotArea.Bottom; return true; }

            area = default;
            return false;
        }

        public void OnActivityRegistered(ManagementBtnType type, List<MemberDataSO> members)
        {
            GetRegistrationUI(type)?.SetupRegistrationUI(members);
            Refresh();
        }

        public void OnActivityUnregistered(ManagementBtnType type)
        {
            GetRegistrationUI(type)?.Hide();
            Refresh();
        }

        private ScheduleRegistrationUI GetRegistrationUI(ManagementBtnType type) => type switch
        {
            ManagementBtnType.Concert  => _concertRegistrationUI,
            ManagementBtnType.Song     => _songRegistrationUI,
            ManagementBtnType.PartTime => _partTimeRegistrationUI,
            _                          => null,
        };

        public void SetMembersInteractable(List<MemberType> memberTypes, bool interactable)
        {
            Refresh();
        }

        public void SetConditionIconVisible(bool visible)
        {
            _view.SetConditionIconVisible(visible);
        }

        public void Dispose()
        {
            _view.OnMemberClicked       -= HandleMemberClicked;
            _view.OnSlotCancelClicked   -= HandleSlotCancelClicked;
            _view.OnMemberDroppedToSlot -= HandleMemberDroppedToSlot;

            if (MemberConditionManager.Instance != null)
                MemberConditionManager.Instance.OnConditionChanged -= HandleConditionChanged;
        }
    }
}