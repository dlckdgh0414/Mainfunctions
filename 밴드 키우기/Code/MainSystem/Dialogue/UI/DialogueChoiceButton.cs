using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 선택지 버튼 UI 스크립트
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DialogueChoiceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI choiceText;
        [SerializeField] private TextMeshProUGUI subText;
        [SerializeField] private GameObject lockIndicator;
        
        private Button _button;
        private DialogueChoiceViewData _choiceData;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        /// <summary>
        /// 선택지 데이터 설정
        /// </summary>
        /// <param name="choice">표시할 선택지 데이터</param>
        public void Setup(DialogueChoiceViewData choice)
        {
            _choiceData = choice;

            if (_button != null)
            {
                _button.interactable = !choice.IsLocked;
            }

            if (choiceText != null)
            {
                if (subText == null && !string.IsNullOrWhiteSpace(choice.SubText))
                {
                    choiceText.text = $"{choice.ChoiceText}\n<size=65%>{choice.SubText}</size>";
                }
                else
                {
                    choiceText.text = choice.ChoiceText;
                }
            }

            if (subText != null)
            {
                bool hasSubText = !string.IsNullOrWhiteSpace(choice.SubText);
                subText.gameObject.SetActive(hasSubText);
                if (hasSubText)
                {
                    subText.text = choice.SubText;
                }
            }

            if (lockIndicator != null)
            {
                lockIndicator.SetActive(choice.IsLocked);
            }
        }

        /// <summary>
        /// 버튼 클릭 이벤트 처리
        /// </summary>
        private void OnClick()
        {
            if (_choiceData.IsLocked)
            {
                return;
            }

            Bus<DialogueChoiceSelectedEvent>.Raise(new DialogueChoiceSelectedEvent(_choiceData.ChoiceIndex));
        }
    }
}
