using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 다이알로그 오토 모드 상태를 토글하는 기능의 UI 버튼 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DialogueAutoToggleButton : MonoBehaviour
    {
        private Button _autoButton;
        private Image _buttonImage;
        
        [SerializeField]
        private Color onColor = new Color(0.7f, 0.7f, 0.7f, 1f); // 오토 켜졌을 때 약간 어두운 색
        
        [SerializeField]
        private Color offColor = Color.white; // 오토 꺼졌을 때 원래 색

        // 현재 오토 모드가 켜져있는지 여부를 저장하는 내부 상태값
        private bool _isAutoModeOn = false;

        private void Awake()
        {
            _autoButton = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            _autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        private void OnDisable()
        {
            _autoButton.onClick.RemoveListener(OnAutoButtonClicked);
        }

        /// <summary>
        /// 버튼 클릭 시 내부 상태를 반전시키고 오토 모드 토글 이벤트를 발행
        /// </summary>
        private void OnAutoButtonClicked()
        {
            _isAutoModeOn = !_isAutoModeOn;
            
            // 시각적 피드백: 이미지 색상 변경
            if (_buttonImage != null)
            {
                _buttonImage.color = _isAutoModeOn ? onColor : offColor;
            }

            ToggleAutoModeEvent toggleEvent = new ToggleAutoModeEvent(_isAutoModeOn);
            Bus<ToggleAutoModeEvent>.Raise(toggleEvent);
        }
    }
}
