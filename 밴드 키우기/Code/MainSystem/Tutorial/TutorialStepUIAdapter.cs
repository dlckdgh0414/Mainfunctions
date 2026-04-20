using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 단일 UI 타깃 상태 정의 데이터.
    /// </summary>
    [Serializable]
    public class TutorialUiTargetState
    {
        [SerializeField] private TutorialTargetId targetId;
        [SerializeField] private bool visible = true;
        [SerializeField] private bool interactable = true;

        /// <summary>
        /// 타깃 식별자 반환.
        /// </summary>
        public TutorialTargetId TargetId => targetId;

        /// <summary>
        /// 표시 여부 반환.
        /// </summary>
        public bool Visible => visible;

        /// <summary>
        /// 상호작용 가능 여부 반환.
        /// </summary>
        public bool Interactable => interactable;
    }

    /// <summary>
    /// 단계별 UI 상태 정의 데이터.
    /// </summary>
    [Serializable]
    public class TutorialStepUiRule
    {
        [SerializeField] private TutorialFlowStep step;
        [SerializeField] private List<TutorialUiTargetState> targetStates = new List<TutorialUiTargetState>();

        /// <summary>
        /// 적용 대상 단계 반환.
        /// </summary>
        public TutorialFlowStep Step => step;

        /// <summary>
        /// 대상 UI 상태 목록 반환.
        /// </summary>
        public IReadOnlyList<TutorialUiTargetState> TargetStates => targetStates;
    }

    /// <summary>
    /// 튜토리얼 단계 변경 이벤트를 UI 상태에 반영하는 어댑터.
    /// </summary>
    public class TutorialStepUIAdapter : MonoBehaviour
    {
        [Header("플로우")]
        [SerializeField] private TutorialFlowController tutorialFlowController;

        [Header("타깃 레지스트리")]
        [SerializeField] private TutorialTargetRegistry targetRegistry;

        [Header("초기 숨김 타깃")]
        [SerializeField] private List<TutorialTargetId> hiddenOnStartTargets = new List<TutorialTargetId>();

        [Header("단계별 UI 규칙")]
        [SerializeField] private List<TutorialStepUiRule> stepRules = new List<TutorialStepUiRule>();

        private Dictionary<TutorialFlowStep, TutorialStepUiRule> _ruleByStep =
            new Dictionary<TutorialFlowStep, TutorialStepUiRule>();

        private void Awake()
        {
            BuildRuleLookup();
            ApplyHiddenOnStart();
        }

        private void OnEnable()
        {
            Bus<TutorialStepChangedEvent>.OnEvent += HandleTutorialStepChanged;
        }

        private void Start()
        {
            if (tutorialFlowController == null)
            {
                return;
            }

            if (tutorialFlowController.IsRunning)
            {
                HandleTutorialStepChanged(new TutorialStepChangedEvent(tutorialFlowController.CurrentStep));
            }
        }

        private void OnDisable()
        {
            Bus<TutorialStepChangedEvent>.OnEvent -= HandleTutorialStepChanged;
        }

        /// <summary>
        /// 단계 규칙 조회 테이블 구성.
        /// </summary>
        private void BuildRuleLookup()
        {
            _ruleByStep.Clear();

            int count = stepRules.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialStepUiRule rule = stepRules[i];
                if (rule == null)
                {
                    continue;
                }

                _ruleByStep[rule.Step] = rule;
            }
        }

        /// <summary>
        /// 시작 시 숨김 타깃 적용.
        /// </summary>
        private void ApplyHiddenOnStart()
        {
            if (targetRegistry == null)
            {
                return;
            }

            int count = hiddenOnStartTargets.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialTargetId targetId = hiddenOnStartTargets[i];
                if (!targetRegistry.TryGetTargetObject(targetId, out GameObject targetObject))
                {
                    continue;
                }

                ApplyVisible(targetObject, false);
                ApplyInteractable(targetObject, false);
            }
        }

        /// <summary>
        /// 단계 변경 이벤트 수신 처리.
        /// </summary>
        /// <param name="evt">튜토리얼 단계 변경 이벤트.</param>
        private void HandleTutorialStepChanged(TutorialStepChangedEvent evt)
        {
            if (tutorialFlowController != null && !tutorialFlowController.IsRunning)
            {
                return;
            }

            if (targetRegistry == null)
            {
                return;
            }

            if (!_ruleByStep.TryGetValue(evt.Step, out TutorialStepUiRule rule))
            {
                return;
            }

            ApplyRule(rule);
        }

        /// <summary>
        /// 단계 규칙 적용.
        /// </summary>
        /// <param name="rule">적용 대상 단계 규칙.</param>
        private void ApplyRule(TutorialStepUiRule rule)
        {
            IReadOnlyList<TutorialUiTargetState> targetStates = rule.TargetStates;
            int count = targetStates.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialUiTargetState targetState = targetStates[i];
                if (targetState == null)
                {
                    continue;
                }

                if (!targetRegistry.TryGetTargetObject(targetState.TargetId, out GameObject targetObject))
                {
                    continue;
                }

                ApplyVisible(targetObject, targetState.Visible);
                ApplyInteractable(targetObject, targetState.Interactable);
            }
        }

        /// <summary>
        /// 표시 상태 적용.
        /// </summary>
        /// <param name="targetObject">반영 대상 오브젝트.</param>
        /// <param name="visible">표시 여부.</param>
        private static void ApplyVisible(GameObject targetObject, bool visible)
        {
            targetObject.SetActive(visible);
        }

        /// <summary>
        /// 상호작용 상태 적용.
        /// </summary>
        /// <param name="targetObject">반영 대상 오브젝트.</param>
        /// <param name="interactable">상호작용 가능 여부.</param>
        private static void ApplyInteractable(GameObject targetObject, bool interactable)
        {
            Selectable selectable = targetObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectable.interactable = interactable;
            }

            CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }
    }
}
