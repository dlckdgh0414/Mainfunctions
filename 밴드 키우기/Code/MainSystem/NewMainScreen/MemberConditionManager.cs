using System;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.Tree.Addon;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class MemberConditionManager : MonoBehaviour
    {
        public static MemberConditionManager Instance;

        private readonly Dictionary<MemberType, MemberConditionMode> _memberConditionDict
            = new Dictionary<MemberType, MemberConditionMode>();
        public event Action<MemberType, MemberConditionMode, MemberConditionMode> OnConditionChanged;

        public IEnumerable<MemberType> RegisteredMembers => _memberConditionDict.Keys;

        private const int ConditionMin = (int)MemberConditionMode.VeryGood;
        private const int ConditionMax = (int)MemberConditionMode.VeryBad;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitManager(MemberType type, MemberConditionMode mode)
        {
            _memberConditionDict[type] = mode;
        }

        /// <summary>
        /// 멤버 타입을 넣어서 현재 멤버 컨디션을 받아온다.
        /// </summary>
        public MemberConditionMode GetCondition(MemberType type)
        {
            return _memberConditionDict.TryGetValue(type, out var mode) ? mode : MemberConditionMode.Commonly;
        }

        /// <summary>
        /// 컨디션을 upCnt만큼 올림 (VeryGood 방향)
        /// ex) Bad(3) + 2 → VeryGood(0) 이면 VeryGood에서 클램프
        /// </summary>
        public void UpCondition(MemberType type, int upCnt)
        {
            ChangeCondition(type, -(upCnt));
        }

        /// <summary>
        /// 컨디션을 downCnt만큼 내림 (VeryBad 방향)
        /// ex) Good(1) - 2 → VeryBad(4) 이면 VeryBad에서 클램프
        /// </summary>
        public void DownCondition(MemberType type, int downCnt)
        {
            ChangeCondition(type, +(downCnt));
        }

        private void ChangeCondition(MemberType type, int delta)
        {
            if (!_memberConditionDict.TryGetValue(type, out var current))
            {
                Debug.LogWarning($"[ConditionManager] {type} 가 등록되지 않았습니다.");
                return;
            }

            int rawNext  = Mathf.Clamp((int)current + delta, ConditionMin, ConditionMax);
            var next     = (MemberConditionMode)rawNext;

            if (current == next) return;

            _memberConditionDict[type] = next;

            Debug.Log($"[ConditionManager] {type} : {current} → {next}");
            OnConditionChanged?.Invoke(type, current, next);
        }

        /// <summary>
        /// 튜토리얼 종료 시 모든 멤버 컨디션 초기화.
        /// </summary>
        /// <param name="defaultMode">초기화 대상 컨디션 모드.</param>
        public void ResetAllConditionsForTutorial(MemberConditionMode defaultMode)
        {
            List<MemberType> memberTypes = new List<MemberType>(_memberConditionDict.Keys);
            int memberCount = memberTypes.Count;
            for (int i = 0; i < memberCount; i++)
            {
                MemberType memberType = memberTypes[i];
                MemberConditionMode oldMode = _memberConditionDict[memberType];

                if (oldMode == defaultMode)
                {
                    continue;
                }

                _memberConditionDict[memberType] = defaultMode;
                OnConditionChanged?.Invoke(memberType, oldMode, defaultMode);
            }
        }
    }
}
