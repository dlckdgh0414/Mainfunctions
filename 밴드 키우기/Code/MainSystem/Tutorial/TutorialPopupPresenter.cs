using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 단계별 팝업 GameObject 매핑 엔트리.
    /// </summary>
    [Serializable]
    public class TutorialPopupEntry
    {
        [SerializeField] private TutorialFlowStep step;
        [SerializeField] private GameObject popupObject;
        [SerializeField] private Button nextButton;
        [SerializeField] private bool closeByNextButton = true;

        public TutorialFlowStep Step => step;
        public GameObject PopupObject => popupObject;
        public Button NextButton => nextButton;
        public bool CloseByNextButton => closeByNextButton;
    }

    /// <summary>
    /// 튜토리얼 단계별 전용 팝업 표시 담당.
    /// </summary>
    public class TutorialPopupPresenter : MonoBehaviour
    {
        [Header("플로우")]
        [SerializeField] private TutorialFlowController tutorialFlowController;

        [Header("단계별 팝업")]
        [SerializeField] private List<TutorialPopupEntry> popupEntries = new List<TutorialPopupEntry>();

        private readonly Dictionary<TutorialFlowStep, TutorialPopupEntry> _entryByStep = new();
        private TutorialFlowStep _currentStep = TutorialFlowStep.None;
        private TutorialPopupEntry _currentEntry;

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
            if (tutorialFlowController != null && tutorialFlowController.IsRunning)
            {
                HandleStepChanged(new TutorialStepChangedEvent(tutorialFlowController.CurrentStep));
            }
        }

        private void OnDisable()
        {
            Bus<TutorialStepChangedEvent>.OnEvent -= HandleStepChanged;
            UnbindCurrentButton();
        }

        /// <summary>
        /// 단계별 팝업 조회 테이블 구성.
        /// </summary>
        private void BuildLookup()
        {
            _entryByStep.Clear();
            int count = popupEntries.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialPopupEntry entry = popupEntries[i];
                if (entry == null || entry.PopupObject == null) continue;
                _entryByStep[entry.Step] = entry;
            }
        }

        /// <summary>
        /// 모든 팝업 비활성화.
        /// </summary>
        private void HideAll()
        {
            int count = popupEntries.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialPopupEntry entry = popupEntries[i];
                if (entry == null || entry.PopupObject == null) continue;
                entry.PopupObject.SetActive(false);
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
                HideCurrent();
                return;
            }

            _currentStep = evt.Step;

            HideCurrent();

            if (!_entryByStep.TryGetValue(evt.Step, out TutorialPopupEntry entry))
            {
                return;
            }

            ShowEntry(entry);
        }

        /// <summary>
        /// 엔트리 팝업 표시 및 다음 버튼 바인딩.
        /// </summary>
        /// <param name="entry">표시 대상 엔트리.</param>
        private void ShowEntry(TutorialPopupEntry entry)
        {
            entry.PopupObject.SetActive(true);
            _currentEntry = entry;

            if (entry.NextButton != null && entry.CloseByNextButton)
            {
                entry.NextButton.onClick.RemoveListener(HandleNextClicked);
                entry.NextButton.onClick.AddListener(HandleNextClicked);
            }
        }

        /// <summary>
        /// 현재 활성 팝업 숨김 및 버튼 해제.
        /// </summary>
        private void HideCurrent()
        {
            UnbindCurrentButton();

            if (_currentEntry != null && _currentEntry.PopupObject != null)
            {
                _currentEntry.PopupObject.SetActive(false);
            }
            _currentEntry = null;
        }

        /// <summary>
        /// 현재 엔트리 버튼 리스너 해제.
        /// </summary>
        private void UnbindCurrentButton()
        {
            if (_currentEntry != null && _currentEntry.NextButton != null)
            {
                _currentEntry.NextButton.onClick.RemoveListener(HandleNextClicked);
            }
        }

        /// <summary>
        /// 다음 버튼 클릭 처리. 팝업 닫기 이벤트 발행 후 숨김.
        /// </summary>
        private void HandleNextClicked()
        {
            TutorialFlowStep closedStep = _currentStep;
            HideCurrent();
            Bus<TutorialPopupClosedEvent>.Raise(new TutorialPopupClosedEvent(closedStep));
        }
    }
}