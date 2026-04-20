using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.MusicProduction;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using Code.SubSystem.Save;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public class SchedulePresenter : ISchedulePresenter
    {
        private readonly IScheduleView           _view;
        private readonly IParticipationPresenter _participationPresenter;
        private readonly IShopPresenter          _shopPresenter;
        private readonly MemberThrowDataSO       _memberThrowDataSO;
        private readonly TurnManager             _turnManager;
        private readonly PromotionPresenter      _promotionPresenter;

        private ManagementBtnType? _selectedType;
        private bool               _cancelEnabled;

        public event Action<ManagementBtnType> OnScheduleStarted;
        public event Action                    OnScheduleCancelled;
        public event Action                    OnAllSchedulesCompleted;

        private static class ActivityLabel
        {
            public const string Concert  = "현재 선택된 활동 : 멜로디 제작";
            public const string Song     = "현재 선택된 활동 : 가사 제작";
            public const string PartTime = "현재 선택된 활동 : 알바";
        }

        private static readonly ManagementBtnType[] ActivityOrder =
        {
            ManagementBtnType.Concert,
            ManagementBtnType.Song,
            ManagementBtnType.PartTime,
        };

        public SchedulePresenter(
            IScheduleView view,
            IParticipationPresenter participationPresenter,
            IShopPresenter shopPresenter,
            MemberThrowDataSO memberThrowDataSO,
            TurnManager turnManager,
            PromotionPresenter promotionPresenter = null)
        {
            _view                   = view;
            _participationPresenter = participationPresenter;
            _shopPresenter          = shopPresenter;
            _memberThrowDataSO      = memberThrowDataSO;
            _turnManager            = turnManager;
            _promotionPresenter     = promotionPresenter;

            _view.OnActivityClicked += HandleActivityClicked;
            _view.OnRegisterClicked += HandleRegisterClicked;
            _view.OnStartAllClicked += HandleStartAllClicked;
            _view.OnCancelClicked   += HandleCancelClicked;
            _view.OnScheduleGroupEntered += RefreshStartAllButton;

            _participationPresenter.OnSlotChanged += HandleSlotChanged;

            if (_turnManager != null)
                _turnManager.OnDateChanged += HandleDateChanged;

            RefreshCellPhoneText();
            RefreshStartAllButton();
            
            UpdateActivityButtonsState();
            
            RestoreMusicUploadButtonIfNeeded();
        }
        
        private void HandleMusicPercentChanged(int percent)
        {
            UpdateActivityButtonsState();
        }
        
        public void UpdateActivityButtonsState()
        {
            var save = SaveManager.Instance;
            bool isComplete = false;

            if (save != null && save.Data != null)
            {
                isComplete = (save.Data.musicPerfectionPercent >= 100);
            }
            
            _view.SetMusicUploadButtonVisible(isComplete);
            
            _view.SetActivityButtonsInteractable(!isComplete);
        }

        /// <summary>
        /// 세이브 데이터에 musicPerfectionPercent == 100 이 기록되어 있으면
        /// 앱 재시작 시에도 업로드 버튼을 보이게 복원한다.
        /// </summary>
        private void RestoreMusicUploadButtonIfNeeded()
        {
            var save = Code.SubSystem.Save.SaveManager.Instance;
            if (save == null) return;

            bool uploadReady = save.Data.musicPerfectionPercent >= 100
                               && save.Data.hasMusicProduction;
            if (uploadReady)
            {
                Debug.Log("[SchedulePresenter] 저장된 완성도 100% 감지 → 업로드 버튼 복원");
                SetMusicUploadReady(true);
            }
        }
        
        

        public void SetMusicUploadReady(bool ready, bool syncActivityButtons = true)
        {
            _view.SetMusicUploadButtonVisible(ready);

            if (syncActivityButtons)
            {
                _view.SetActivityButtonsInteractable(!ready);
            }
        }

        /// <summary>
        /// 음악 완성도가 100%인지 확인하는 헬퍼.
        /// </summary>
        private static bool IsMusicComplete()
        {
            return GameStatManager.Instance != null
                   && GameStatManager.Instance.GetMusicPerfectionPercent() >= 100;
        }

        private void HandleActivityClicked(ManagementBtnType type)
        {
            if (IsMusicComplete())
            {
                bool isBlocked = type == ManagementBtnType.Concert
                              || type == ManagementBtnType.Song;
                if (isBlocked)
                {
                    Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                        SystemMessageIconType.Warning,
                        "음악이 완성되었습니다!\n업로드하거나 알바만 진행할 수 있습니다."));
                    return;
                }
            }

            Bus<TutorialActivitySelectedEvent>.Raise(new TutorialActivitySelectedEvent(type));
            _view.SetStartAllButtonVisible(false);
            _view.SetRegisteredScheduleBarVisible(false);
            _selectedType  = type;
            _cancelEnabled = true;

            _view.SetMemberListBarVisible(true);
            _view.SetCellPhoneTimeVisible(false);
            _view.SetAppBarVisible(false);

            bool showCondition = type == ManagementBtnType.PartTime;
            _participationPresenter.SetConditionIconVisible(showCondition);

            if (type == ManagementBtnType.Shop)
            {
                _view.SetMemberListBarVisible(false);
                _view.SetStartBarVisible(false);
                _view.SetParticipationInfoVisible(false);
                _shopPresenter?.OpenShop();
            }
            else if (type == ManagementBtnType.Tree)
            {
                _view.SetTreeBarVisible(true);
                _view.SetMemberListBarVisible(false);
            }
            else if (type == ManagementBtnType.Promotion)
            {
                _view.SetMemberListBarVisible(false);
                _view.SetStartBarVisible(false);
                _view.SetParticipationInfoVisible(false);
                _promotionPresenter?.OpenPromotion();
            }
            else if (type == ManagementBtnType.MusicUpload)
            {
                HandleMusicUpload();
            }
            else
            {
                _memberThrowDataSO.SetCurrentActivity(type, true);

                if (_memberThrowDataSO.IsActivityRegistered(type))
                {
                    var members = _memberThrowDataSO.GetMembers(type);
                    _participationPresenter.RestoreSlots(type, members);
                }
                else
                {
                    _participationPresenter.ClearCurrent();
                }
                ShowActivityInfo(type);
            }
            _participationPresenter.Refresh();
        }

        private void HandleMusicUpload()
        {
            if (GameStatManager.Instance != null && GameStatManager.Instance.GetMusicPerfectionPercent() < 100)
            {
                Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                    SystemMessageIconType.Warning, "음악 완성을 해야 업로드할 수 있습니다."));
                ReturnToAppBar();
                return;
            }

            Debug.Log("[SchedulePresenter] 음악 업로드! 1주 경과");
            GameStatManager.Instance?.SetMusicPercent(0);
            Bus<MusicUploadEvent>.Raise(new MusicUploadEvent());
            SetMusicUploadReady(false);
            
            _turnManager?.AdvanceOneWeek();

            ReturnToAppBar();
        }

        private void ShowActivityInfo(ManagementBtnType type)
        {
            _view.SetParticipationInfoVisible(true);
            _view.SetStartBarVisible(true);
            _view.SetParticipationInfoText(type switch
            {
                ManagementBtnType.Concert  => ActivityLabel.Concert,
                ManagementBtnType.Song     => ActivityLabel.Song,
                ManagementBtnType.PartTime => ActivityLabel.PartTime,
                _                          => string.Empty,
            });
        }

        private void HandleRegisterClicked()
        {
            if (_selectedType == null) return;
            var type = _selectedType.Value;

            if ((type == ManagementBtnType.Concert || type == ManagementBtnType.Song)
                && MusicProductionManager.Instance != null
                && !MusicProductionManager.Instance.HasMusicData)
            {
                Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                    SystemMessageIconType.Warning, "음악 데이터가 없습니다! 먼저 곡을 설정해주세요."));
                return;
            }

            if (_participationPresenter.CurrentSlotCount == 0)
            {
                _memberThrowDataSO.UnregisterActivity(type);
                _participationPresenter.OnActivityUnregistered(type);
                ReturnToAppBar();
                return;
            }

            if (_memberThrowDataSO.RegisterCurrent())
            {
                var members = _memberThrowDataSO.GetMembers(type);
                _participationPresenter.OnActivityRegistered(type, members);

                int memberCount = members != null ? members.Count : 0;
                Bus<TutorialScheduleRegisteredEvent>.Raise(new TutorialScheduleRegisteredEvent(type, memberCount));
            }

            ReturnToAppBar();
        }

        private void HandleStartAllClicked()
        {
            var registered = _memberThrowDataSO.ScheduledMembers;
            if (registered.Count == 0) return;

            var activitiesToRun = new List<ManagementBtnType>();
            foreach (var activityType in ActivityOrder)
            {
                if (registered.ContainsKey(activityType))
                    activitiesToRun.Add(activityType);
            }

            if (activitiesToRun.Count == 0) return;

            _memberThrowDataSO.EnqueueSchedules(activitiesToRun);
            Bus<TutorialStartAllPressedEvent>.Raise(new TutorialStartAllPressedEvent(activitiesToRun.Count));
            _participationPresenter.Refresh();
            RefreshStartAllButton();
            _view.SetStartAllButtonVisible(false);
            ExecuteNextSchedule();
        }

        public void ExecuteNextSchedule()
        {
            var next = _memberThrowDataSO.DequeueNextSchedule();
            if (next == null)
            {
                Debug.Log("[SchedulePresenter] 더 이상 실행할 일정이 없습니다. → OnAllSchedulesCompleted 발생");
                OnAllSchedulesCompleted?.Invoke();
                return;
            }

            var type = next.Value;
            _memberThrowDataSO.PrepareCurrentMembersForExecution(type);
            int memberCount = _memberThrowDataSO.CurrentMembers.Count;
            Bus<TutorialScheduleExecutionStartedEvent>.Raise(new TutorialScheduleExecutionStartedEvent(type, memberCount));
            OnScheduleStarted?.Invoke(type);
        }

        private void HandleCancelClicked()
        {
            Bus<TutorialReturnButtonClickedEvent>.Raise(new TutorialReturnButtonClickedEvent());
            Cancel();
        }

        public void RegisterCurrentSchedule() => HandleRegisterClicked();

        public void StartAllSchedules() => HandleStartAllClicked();

        public void Cancel()
        {
            if (!_cancelEnabled) return;

            ManagementBtnType? selectedType = _selectedType;
            _memberThrowDataSO.ClearCurrent();

            if (selectedType == ManagementBtnType.Shop)
                _shopPresenter?.CloseShop();
            else if (selectedType == ManagementBtnType.Promotion)
                _promotionPresenter?.ClosePromotion();
            else if (selectedType == ManagementBtnType.Tree)
                Bus<TutorialTreeClosedEvent>.Raise(new TutorialTreeClosedEvent());

            ReturnToAppBar();
        }

        private void ReturnToAppBar()
        {
            _selectedType  = null;
            _cancelEnabled = false;
            _view.SetAppBarVisible(true);
            _view.ReturnToLastGroup();
            _view.SetCellPhoneTimeVisible(true);
            _view.SetParticipationInfoVisible(false);
            _view.SetStartBarVisible(false);
            _view.SetMemberListBarVisible(false);
            _view.SetTreeBarVisible(false);
            _participationPresenter.SetConditionIconVisible(false);
            RefreshCellPhoneText();
            RefreshStartAllButton();
            _participationPresenter.ClearCurrent();
            _participationPresenter.Refresh();
            OnScheduleCancelled?.Invoke();
        }

        private void RefreshStartAllButton()
        {
            bool hasScheduled = _memberThrowDataSO.ScheduledMembers.Count > 0;
            _view.SetStartAllButtonVisible(hasScheduled && _view.IsInScheduleGroup);
        }

        private void RefreshCellPhoneText()
        {
            _view.SetCellPhoneText(_turnManager != null
                ? _turnManager.GetCellPhoneDisplayText()
                : GetSystemTimeString());
        }

        private static string GetSystemTimeString()
        {
            DateTime now    = DateTime.Now;
            string period   = now.Hour >= 12 ? "PM" : "AM";
            int h12         = now.Hour % 12 == 0 ? 12 : now.Hour % 12;
            return $"{period} : {h12:D2}:{now.Minute:D2}";
        }

        private void HandleSlotChanged() => RefreshRegisterButton();

        private void RefreshRegisterButton()
        {
            if (_selectedType == null)
            {
                _view.SetRegisterButtonInteractable(false);
                return;
            }
            _view.SetRegisterButtonInteractable(true);
        }

        private void HandleDateChanged(int y, int m, int d) => RefreshCellPhoneText();

        public void Dispose()
        {
            _view.OnActivityClicked      -= HandleActivityClicked;
            _view.OnRegisterClicked      -= HandleRegisterClicked;
            _view.OnStartAllClicked      -= HandleStartAllClicked;
            _view.OnCancelClicked        -= HandleCancelClicked;
            _view.OnScheduleGroupEntered -= RefreshStartAllButton;
            _participationPresenter.OnSlotChanged -= HandleSlotChanged;
            if (_turnManager != null) _turnManager.OnDateChanged -= HandleDateChanged;
        }
    }
}
