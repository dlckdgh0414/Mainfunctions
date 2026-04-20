using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using UnityEngine;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 선택지 UI를 관리하는 매니저
    /// </summary>
    public class DialogueChoiceUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Transform choiceButtonContainer;

        private List<GameObject> _activeButtons = new List<GameObject>();

        private void OnEnable()
        {
            Bus<DialogueShowChoiceEvent>.OnEvent += OnShowChoice;
            Bus<DialogueChoiceSelectedEvent>.OnEvent += OnChoiceSelected;
        }

        private void OnDisable()
        {
            Bus<DialogueShowChoiceEvent>.OnEvent -= OnShowChoice;
            Bus<DialogueChoiceSelectedEvent>.OnEvent -= OnChoiceSelected;
        }

        /// <summary>
        /// 선택지 표시 이벤트 핸들러
        /// </summary>
        /// <param name="evt">이벤트 데이터</param>
        private void OnShowChoice(DialogueShowChoiceEvent evt)
        {
            ClearButtons();

            if (evt.Choices == null || evt.Choices.Count == 0) return;

            if (choiceButtonContainer != null)
                choiceButtonContainer.gameObject.SetActive(true);

            foreach (DialogueChoiceViewData choice in evt.Choices)
            {
                if (choiceButtonPrefab == null || choiceButtonContainer == null)
                {
                    Debug.LogError("DialogueChoiceUIManager: Prefab or Container is null!");
                    continue;
                }

                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                if (btnObj.TryGetComponent<DialogueChoiceButton>(out DialogueChoiceButton btn))
                {
                    btn.Setup(choice);
                }
                _activeButtons.Add(btnObj);
            }
        }

        /// <summary>
        /// 선택 완료 이벤트 핸들러
        /// </summary>
        /// <param name="evt">이벤트 데이터</param>
        private void OnChoiceSelected(DialogueChoiceSelectedEvent evt)
        {
            ClearButtons();
            if (choiceButtonContainer != null)
                choiceButtonContainer.gameObject.SetActive(false);
        }

        private void ClearButtons()
        {
            foreach (GameObject btn in _activeButtons)
            {
                Destroy(btn);
            }
            _activeButtons.Clear();
        }
    }
}
