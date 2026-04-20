using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using UnityEngine;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 단계별 오버레이 포커스 규칙 데이터.
    /// </summary>
    [Serializable]
    public class TutorialOverlayRule
    {
        [SerializeField] private TutorialFlowStep step;
        [SerializeField] private bool showDim = true;
        [SerializeField] private float dimAlpha = 0.7f;
        [SerializeField] private bool showFocus;
        [SerializeField] private TutorialTargetId focusTargetId;

        /// <summary>
        /// 적용 대상 단계 반환.
        /// </summary>
        public TutorialFlowStep Step => step;

        /// <summary>
        /// 딤 표시 여부 반환.
        /// </summary>
        public bool ShowDim => showDim;

        /// <summary>
        /// 딤 알파 반환.
        /// </summary>
        public float DimAlpha => dimAlpha;

        /// <summary>
        /// 포커스 표시 여부 반환.
        /// </summary>
        public bool ShowFocus => showFocus;

        /// <summary>
        /// 포커스 타깃 식별자 반환.
        /// </summary>
        public TutorialTargetId FocusTargetId => focusTargetId;
    }

    /// <summary>
    /// 튜토리얼 오버레이와 포커스 표시 제어 담당.
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        [Header("플로우")]
        [SerializeField] private TutorialFlowController tutorialFlowController;

        [Header("타깃 레지스트리")]
        [SerializeField] private TutorialTargetRegistry targetRegistry;

        [Header("오버레이")]
        [SerializeField] private CanvasGroup dimCanvasGroup;
        [SerializeField] private RectTransform focusFrame;
        [SerializeField] private RectTransform focusCanvasRect;

        [Header("규칙")]
        [SerializeField] private List<TutorialOverlayRule> overlayRules = new List<TutorialOverlayRule>();

        private Dictionary<TutorialFlowStep, TutorialOverlayRule> _ruleByStep =
            new Dictionary<TutorialFlowStep, TutorialOverlayRule>();

        private void Awake()
        {
            BuildLookup();
            HideAll();
        }

        private void OnEnable()
        {
            Bus<TutorialStepChangedEvent>.OnEvent += HandleStepChanged;
        }

        private void Start()
        {
            if (tutorialFlowController == null)
            {
                return;
            }

            if (tutorialFlowController.IsRunning)
            {
                HandleStepChanged(new TutorialStepChangedEvent(tutorialFlowController.CurrentStep));
            }
        }

        private void OnDisable()
        {
            Bus<TutorialStepChangedEvent>.OnEvent -= HandleStepChanged;
        }

        /// <summary>
        /// 규칙 조회 테이블 구성.
        /// </summary>
        private void BuildLookup()
        {
            _ruleByStep.Clear();

            int count = overlayRules.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialOverlayRule rule = overlayRules[i];
                if (rule == null)
                {
                    continue;
                }

                _ruleByStep[rule.Step] = rule;
            }
        }

        /// <summary>
        /// 단계 변경 이벤트 처리.
        /// </summary>
        /// <param name="evt">튜토리얼 단계 변경 이벤트.</param>
        private void HandleStepChanged(TutorialStepChangedEvent evt)
        {
            if (tutorialFlowController != null && !tutorialFlowController.IsRunning)
            {
                HideAll();
                return;
            }

            if (!_ruleByStep.TryGetValue(evt.Step, out TutorialOverlayRule rule))
            {
                HideAll();
                return;
            }

            ApplyRule(rule);
        }

        /// <summary>
        /// 오버레이 규칙 적용.
        /// </summary>
        /// <param name="rule">적용 대상 규칙.</param>
        private void ApplyRule(TutorialOverlayRule rule)
        {
            ApplyDim(rule.ShowDim, rule.DimAlpha);

            if (!rule.ShowFocus)
            {
                if (focusFrame != null)
                {
                    focusFrame.gameObject.SetActive(false);
                }
                return;
            }

            if (focusFrame == null || focusCanvasRect == null || targetRegistry == null)
            {
                return;
            }

            if (!targetRegistry.TryGetTargetAnchor(rule.FocusTargetId, out RectTransform targetRect))
            {
                focusFrame.gameObject.SetActive(false);
                return;
            }

            focusFrame.gameObject.SetActive(true);
            FitFocusFrame(targetRect);
        }

        /// <summary>
        /// 딤 상태 적용.
        /// </summary>
        /// <param name="visible">표시 여부.</param>
        /// <param name="alpha">알파 값.</param>
        private void ApplyDim(bool visible, float alpha)
        {
            if (dimCanvasGroup == null)
            {
                return;
            }

            dimCanvasGroup.gameObject.SetActive(visible);
            dimCanvasGroup.alpha = Mathf.Clamp01(alpha);
            dimCanvasGroup.blocksRaycasts = visible;
            dimCanvasGroup.interactable = visible;
        }

        /// <summary>
        /// 포커스 프레임 크기와 위치를 타깃에 맞춤.
        /// </summary>
        /// <param name="targetRect">포커스 대상 RectTransform.</param>
        private void FitFocusFrame(RectTransform targetRect)
        {
            Vector3[] worldCorners = new Vector3[4];
            targetRect.GetWorldCorners(worldCorners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                focusCanvasRect,
                min,
                null,
                out Vector2 localMin);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                focusCanvasRect,
                max,
                null,
                out Vector2 localMax);

            Vector2 center = (localMin + localMax) * 0.5f;
            Vector2 size = localMax - localMin;

            focusFrame.anchoredPosition = center;
            focusFrame.sizeDelta = size;
        }

        /// <summary>
        /// 오버레이 전체 숨김.
        /// </summary>
        private void HideAll()
        {
            ApplyDim(false, 0f);

            if (focusFrame != null)
            {
                focusFrame.gameObject.SetActive(false);
            }
        }
    }
}
