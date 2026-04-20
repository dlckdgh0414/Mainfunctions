using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.SubSystem.Save;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem
{
    public class InitialSetupController : MonoBehaviour
    {
        private enum Step { BandName, UserName }

        [Header("Shared UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI charCountText;
        [SerializeField] private TextMeshProUGUI placeholderText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        [Header("Step Titles")]
        [SerializeField] private string bandNameTitle = "밴드 이름을 정해주세요";
        [SerializeField] private string userNameTitle = "당신의 이름을 알려주세요";

        [Header("Step Placeholders")]
        [SerializeField] private string bandNamePlaceholder = "이름을 입력하세요";
        [SerializeField] private string userNamePlaceholder = "프로듀서 이름을 입력하세요";

        [Header("Step Button Labels")]
        [SerializeField] private string nextButtonLabel = "다음";
        [SerializeField] private string finishButtonLabel = "결정";

        [Header("Settings")]
        [SerializeField] private int minLength = 2;
        [SerializeField] private int maxLength = 20;
        [SerializeField] private string nextSceneName = "Tutor";

        [Header("Colors")]
        [SerializeField] private Color confirmActiveColor = new Color(0.37f, 0.76f, 0.93f);
        [SerializeField] private Color confirmInactiveColor = new Color(0.81f, 0.90f, 0.95f);

        private Image _confirmButtonImage;
        private Step _currentStep = Step.BandName;
        private string _bandNameBuffer;
        private bool _isSubmitting;

        private void Awake()
        {
            _confirmButtonImage = confirmButton.GetComponent<Image>();

            inputField.characterLimit = maxLength;
            inputField.onValueChanged.AddListener(OnInputChanged);
            inputField.onSelect.AddListener(OnInputFieldSelected);
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void Start()
        {
            EnterStep(Step.BandName);
        }

        private void EnterStep(Step step)
        {
            _currentStep = step;

            switch (step)
            {
                case Step.BandName:
                    titleText.text = bandNameTitle;
                    if (placeholderText != null) placeholderText.text = bandNamePlaceholder;
                    if (confirmButtonText != null) confirmButtonText.text = nextButtonLabel;

                    var savedBand = SaveManager.Instance?.Data?.bandName;
                    inputField.text = string.IsNullOrEmpty(savedBand) ? "" : savedBand;
                    break;

                case Step.UserName:
                    titleText.text = userNameTitle;
                    if (placeholderText != null) placeholderText.text = userNamePlaceholder;
                    if (confirmButtonText != null) confirmButtonText.text = finishButtonLabel;

                    var savedUser = SaveManager.Instance?.Data?.userName;
                    inputField.text = string.IsNullOrEmpty(savedUser) ? "" : savedUser;
                    break;
            }

            UpdateCharCount(inputField.text);
            SetConfirmButtonActive(inputField.text.Trim().Length >= minLength);

            titleText.transform.localScale = Vector3.one * 0.9f;
            titleText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            
            FocusInputFieldAsync().Forget();
        }
        
        private async UniTaskVoid FocusInputFieldAsync()
        {
            await UniTask.NextFrame();

            if (inputField == null) return;

            inputField.Select();
            inputField.ActivateInputField();

#if UNITY_ANDROID || UNITY_IOS
            TouchScreenKeyboard.hideInput = false;
#endif
        }
        
        private void OnInputFieldSelected(string value)
        {
            inputField.ActivateInputField();
        }

        private void OnInputChanged(string value)
        {
            UpdateCharCount(value);
            bool isValid = value.Trim().Length >= minLength;
            SetConfirmButtonActive(isValid);
        }

        private void UpdateCharCount(string value)
        {
            if (charCountText != null)
                charCountText.text = $"{value.Length}/{maxLength}";
        }

        private void SetConfirmButtonActive(bool active)
        {
            confirmButton.interactable = active;

            if (_confirmButtonImage != null)
            {
                _confirmButtonImage.DOKill();
                _confirmButtonImage.DOColor(
                    active ? confirmActiveColor : confirmInactiveColor,
                    0.2f
                );
            }
        }

        private void OnConfirmClicked()
        {
            if (_isSubmitting) return;

            string value = inputField.text.Trim();

            if (!IsValidName(value))
            {
                ShakeInput();
                return;
            }

            confirmButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.25f);

            if (_currentStep == Step.BandName)
            {
                _bandNameBuffer = value;
                SaveBandName(value);
                EnterStep(Step.UserName);
            }
            else
            {
                SubmitFinalAsync(value).Forget();
            }
        }

        private async UniTaskVoid SubmitFinalAsync(string userName)
        {
            _isSubmitting = true;
            confirmButton.interactable = false;

            SaveUserName(userName);

            await UniTask.Delay(250);

            Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(nextSceneName));
        }

        private void SaveBandName(string bandName)
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[InitialSetup] SaveManager 없음!");
                return;
            }

            SaveManager.Instance.Data.bandName = bandName;
            SaveManager.Instance.ForceSaveNow();
            Debug.Log($"[InitialSetup] 밴드 이름 저장됨: {bandName}");
        }

        private void SaveUserName(string userName)
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[InitialSetup] SaveManager 없음!");
                return;
            }

            SaveManager.Instance.Data.userName = userName;
            SaveManager.Instance.ForceSaveNow();
            Debug.Log($"[InitialSetup] 유저 이름 저장됨: {userName}");
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.Length < minLength) return false;

            string[] forbidden = { /* 욕설 목록 */ };
            foreach (var word in forbidden)
            {
                if (name.Contains(word)) return false;
            }

            return true;
        }

        private void ShakeInput()
        {
            inputField.transform.DOShakePosition(0.3f, 10f, 20);
        }

        private void OnDestroy()
        {
            inputField.onValueChanged.RemoveListener(OnInputChanged);
            inputField.onSelect.RemoveListener(OnInputFieldSelected);
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }
}