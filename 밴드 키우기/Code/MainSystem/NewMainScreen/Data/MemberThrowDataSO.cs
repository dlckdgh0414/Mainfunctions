using System.Collections.Generic;
using System.Linq;
using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "SelectedMemberData", menuName = "SO/Communication/SelectedMember")]
    public class MemberThrowDataSO : ScriptableObject
    {
        private readonly Dictionary<ManagementBtnType, List<MemberDataSO>> _scheduledMembers = new();
        public IReadOnlyDictionary<ManagementBtnType, List<MemberDataSO>> ScheduledMembers => _scheduledMembers;

        public ManagementBtnType CurrentActivity { get; private set; }
        private readonly List<MemberDataSO> _currentMembers = new();
        public IReadOnlyList<MemberDataSO> CurrentMembers => _currentMembers;

        [SerializeField] private List<ManagementBtnType> _scheduleList = new();
        [SerializeField] private int _scheduleIndex;
        [SerializeField] private bool _hasRunningActivity;
        [SerializeField] private ManagementBtnType _runningActivityType;

        public bool HasPendingSchedule => _scheduleIndex < _scheduleList.Count;
        public ManagementBtnType? RunningActivity => _hasRunningActivity ? _runningActivityType : null;

        public void EnqueueSchedules(List<ManagementBtnType> activities)
        {
            _scheduleList.Clear();
            _scheduleList.AddRange(activities);
            _scheduleIndex = 0;
            _hasRunningActivity = false;
        }

        public ManagementBtnType? DequeueNextSchedule()
        {
            if (_scheduleIndex >= _scheduleList.Count)
            {
                _hasRunningActivity = false;
                return null;
            }
            _runningActivityType = _scheduleList[_scheduleIndex];
            _hasRunningActivity = true;
            _scheduleIndex++;
            return _runningActivityType;
        }

        public void PrepareCurrentMembersForExecution(ManagementBtnType type)
        {
            _currentMembers.Clear();
            CurrentActivity = type;

            if (_scheduledMembers.TryGetValue(type, out var members))
            {
                _currentMembers.AddRange(members);
            }
        }

        public void CleanupCompletedActivity()
        {
            if (!_hasRunningActivity) return;

            var type = _runningActivityType;
            _scheduledMembers.Remove(type);

            _hasRunningActivity = false;
        }

        public void ClearScheduleQueue()
        {
            _scheduleList.Clear();
            _scheduleIndex = 0;
            _hasRunningActivity = false;
        }

        public void SetCurrentActivity(ManagementBtnType type, bool restoreRegistered = false)
        {
            CurrentActivity = type;
            _currentMembers.Clear();

            if (restoreRegistered && _scheduledMembers.TryGetValue(type, out var existing))
                _currentMembers.AddRange(existing);
        }

        public void AddCurrentMember(MemberDataSO data)
        {
            if (_currentMembers.Contains(data)) return;
            _currentMembers.Add(data);
        }

        public void RemoveCurrentMember(MemberDataSO data)
        {
            _currentMembers.Remove(data);
        }

        public bool RegisterCurrent()
        {
            if (_currentMembers.Count == 0) return false;

            _scheduledMembers[CurrentActivity] = new List<MemberDataSO>(_currentMembers);
            _currentMembers.Clear();
            return true;
        }
        
        public void UnregisterActivity(ManagementBtnType type)
        {
            _scheduledMembers.Remove(type);
        }

        public bool IsActivityRegistered(ManagementBtnType type)
            => _scheduledMembers.ContainsKey(type);

        public List<MemberDataSO> GetMembers(ManagementBtnType type)
            => _scheduledMembers.TryGetValue(type, out var list) ? list : null;

        public void ClearCurrent() => _currentMembers.Clear();

        public void ClearAll()
        {
            _scheduledMembers.Clear();
            _currentMembers.Clear();
            _scheduleList.Clear();
            _scheduleIndex = 0;
            _hasRunningActivity = false;
        }

        public int GetAssignedMemberCount(ManagementBtnType type)
        {
            if (CurrentActivity == type && _currentMembers.Count > 0)
            {
                return _currentMembers.Count;
            }
            
            if (_scheduledMembers.TryGetValue(type, out List<MemberDataSO> members))
            {
                return members.Count;
            }

            return 0;
        }
    }
}