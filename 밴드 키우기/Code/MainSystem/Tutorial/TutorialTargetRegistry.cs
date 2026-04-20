using System;
using System.Collections.Generic;
using Code.Core.Bus.GameEvents.TutorialEvents;
using UnityEngine;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 튜토리얼 타깃 단일 매핑 데이터.
    /// </summary>
    [Serializable]
    public class TutorialTargetEntry
    {
        [SerializeField] private TutorialTargetId targetId;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private RectTransform anchorOverride;

        /// <summary>
        /// 타깃 식별자 반환.
        /// </summary>
        public TutorialTargetId TargetId => targetId;

        /// <summary>
        /// 타깃 오브젝트 반환.
        /// </summary>
        public GameObject TargetObject => targetObject;

        /// <summary>
        /// 앵커 오버라이드 반환.
        /// </summary>
        public RectTransform AnchorOverride => anchorOverride;
    }

    /// <summary>
    /// 튜토리얼 타깃 조회 레지스트리.
    /// </summary>
    public class TutorialTargetRegistry : MonoBehaviour
    {
        [SerializeField] private List<TutorialTargetEntry> targets = new List<TutorialTargetEntry>();

        private Dictionary<TutorialTargetId, TutorialTargetEntry> _entryById =
            new Dictionary<TutorialTargetId, TutorialTargetEntry>();

        private void Awake()
        {
            BuildLookup();
        }

        /// <summary>
        /// 타깃 오브젝트 조회.
        /// </summary>
        /// <param name="targetId">조회 대상 식별자.</param>
        /// <param name="targetObject">조회된 오브젝트 출력.</param>
        /// <returns>조회 성공 여부.</returns>
        public bool TryGetTargetObject(TutorialTargetId targetId, out GameObject targetObject)
        {
            targetObject = null;
            if (!_entryById.TryGetValue(targetId, out TutorialTargetEntry entry))
            {
                return false;
            }

            targetObject = entry.TargetObject;
            return targetObject != null;
        }

        /// <summary>
        /// 타깃 앵커 RectTransform 조회.
        /// </summary>
        /// <param name="targetId">조회 대상 식별자.</param>
        /// <param name="anchorRect">조회된 앵커 출력.</param>
        /// <returns>조회 성공 여부.</returns>
        public bool TryGetTargetAnchor(TutorialTargetId targetId, out RectTransform anchorRect)
        {
            anchorRect = null;
            if (!_entryById.TryGetValue(targetId, out TutorialTargetEntry entry))
            {
                return false;
            }

            if (entry.AnchorOverride != null)
            {
                anchorRect = entry.AnchorOverride;
                return true;
            }

            if (entry.TargetObject == null)
            {
                return false;
            }

            anchorRect = entry.TargetObject.GetComponent<RectTransform>();
            return anchorRect != null;
        }

        /// <summary>
        /// 레지스트리 조회 테이블 구성.
        /// </summary>
        private void BuildLookup()
        {
            _entryById.Clear();

            int count = targets.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialTargetEntry entry = targets[i];
                if (entry == null)
                {
                    continue;
                }

                _entryById[entry.TargetId] = entry;
            }
        }
    }
}
