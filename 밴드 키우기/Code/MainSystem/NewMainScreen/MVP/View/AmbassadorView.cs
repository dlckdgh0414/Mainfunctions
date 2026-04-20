using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class AmbassadorView : MonoBehaviour, IAmbassadorView
    {
        [SerializeField] private Button          ambassadorBtn;
        [SerializeField] private GameObject      ambassadorPanel;
        [SerializeField] private Button          ambassadorPanelBtn;
        [SerializeField] private TextMeshProUGUI ambassadorText;
        [SerializeField] private float           autoCloseDelay = 3f;
        [SerializeField] private float           typingInterval = 0.05f;

        public event Action OnButtonClicked;

        public bool IsPanelActive => ambassadorPanel.activeSelf;
        public bool IsTyping      => _isTyping;

        private Coroutine _typingCoroutine;
        private Coroutine _autoCloseCoroutine;
        private string    _currentFullText;
        private bool      _isTyping;

        private void Awake()
        {
            ambassadorPanel.SetActive(false);
            ambassadorBtn.onClick.AddListener(() => OnButtonClicked?.Invoke());

            if (ambassadorPanelBtn != null)
                ambassadorPanelBtn.onClick.AddListener(HandlePanelClicked);
        }

        private void OnDestroy()
        {
            ambassadorBtn.onClick.RemoveAllListeners();
            if (ambassadorPanelBtn != null)
                ambassadorPanelBtn.onClick.RemoveAllListeners();
        }

        private void HandlePanelClicked()
        {
            if (_isTyping)
                FinishTypingImmediately();
        }

        public void SetPanelVisible(bool visible) => ambassadorPanel.SetActive(visible);

        public void SetText(string text) => ambassadorText.SetText(text);

        public void StartTyping(string text, Action onComplete)
        {
            StopTyping();
            _currentFullText = text;
            _typingCoroutine = StartCoroutine(TypingCoroutine(text, onComplete));
        }

        public void StopTyping()
        {
            if (_typingCoroutine != null)    { StopCoroutine(_typingCoroutine);    _typingCoroutine    = null; }
            if (_autoCloseCoroutine != null) { StopCoroutine(_autoCloseCoroutine); _autoCloseCoroutine = null; }
            _isTyping = false;
        }

        private void FinishTypingImmediately()
        {
            StopTyping();
            ambassadorText.SetText(_currentFullText);
            _autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
        }

        private IEnumerator TypingCoroutine(string fullText, Action onComplete)
        {
            _isTyping = true;
            ambassadorText.SetText("");

            foreach (char c in fullText)
            {
                ambassadorText.text += c;
                yield return new WaitForSeconds(typingInterval);
            }

            _isTyping        = false;
            _typingCoroutine = null;
            onComplete?.Invoke();
            _autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
        }

        private IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            ambassadorPanel.SetActive(false);
            ambassadorText.SetText("");
            _autoCloseCoroutine = null;
        }
    }
}