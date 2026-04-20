using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager.Upgarde
{
    public class EventStartUI : MonoBehaviour
    {
        [SerializeField] private Button nextBtn;
        [SerializeField] private TextMeshProUGUI speechText;
        [SerializeField] private string speachStr;
        [SerializeField] private Image memberIcon;
        [SerializeField] private float typingSpeed = 0.05f;

        private CancellationTokenSource _cts;

        public event Action OnNextBtnClick;

        private void Awake()
        {
            nextBtn.onClick.AddListener(() => OnNextBtnClick?.Invoke());
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            nextBtn.onClick.RemoveAllListeners();
        }

        public void Setup(Sprite sprite)
        {
            if (memberIcon == null)
            {
                Debug.LogError("[EventStartUI] memberIcon이 Inspector에 연결되지 않았습니다!");
                return;
            }

            memberIcon.sprite = sprite;
            nextBtn.gameObject.SetActive(false);
            speechText.text = "";

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            TypeText(_cts.Token).Forget();
        }

        private async UniTaskVoid TypeText(CancellationToken token)
        {
            foreach (var c in speachStr)
            {
                if (token.IsCancellationRequested) return;
                speechText.text += c;
                await UniTask.Delay((int)(typingSpeed * 1000), cancellationToken: token);
            }
            nextBtn.gameObject.SetActive(true);
        }
    }
}