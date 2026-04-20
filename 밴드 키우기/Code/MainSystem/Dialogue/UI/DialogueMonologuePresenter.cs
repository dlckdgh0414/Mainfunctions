using System;
using System.Threading;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 다이알로그에서 독백 발생 시 배경 화면을 어둡게 처리하는 연출을 담당하는 Presenter.
    /// </summary>
    public class DialogueMonologuePresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image overlayImage;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;

        private readonly float TARGET_ALPHA = 0.7f;
        private bool _isMonologueActive = false;
        private CancellationTokenSource _fadeCts;

        private void Awake()
        {
            if (overlayImage != null)
            {
                Color color = overlayImage.color;
                color.a = 0f;
                overlayImage.color = color;
                overlayImage.raycastTarget = false;
            }
        }

        private void OnEnable()
        {
            Bus<DialogueProgressEvent>.OnEvent += OnDialogueProgress;
            Bus<DialogueEndEvent>.OnEvent += OnDialogueEnd;
        }

        private void OnDisable()
        {
            Bus<DialogueProgressEvent>.OnEvent -= OnDialogueProgress;
            Bus<DialogueEndEvent>.OnEvent -= OnDialogueEnd;

            CancelFade();
        }

        private void OnDialogueProgress(DialogueProgressEvent evt)
        {
            if (overlayImage == null) return;

            if (evt.IsMonologue)
            {
                if (!_isMonologueActive)
                {
                    _isMonologueActive = true;
                    StartFadeAsync(TARGET_ALPHA).Forget();
                }
            }
            else
            {
                if (_isMonologueActive)
                {
                    _isMonologueActive = false;
                    StartFadeAsync(0f).Forget();
                }
            }
        }

        private void OnDialogueEnd(DialogueEndEvent e)
        {
            if (overlayImage == null) return;

            if (_isMonologueActive || overlayImage.color.a > 0f)
            {
                _isMonologueActive = false;
                StartFadeAsync(0f).Forget();
            }
        }

        private async UniTaskVoid StartFadeAsync(float targetAlpha)
        {
            CancelFade();
            _fadeCts = new CancellationTokenSource();

            try
            {
                await FadeOverlayAsync(targetAlpha, _fadeCts.Token);
            }
            catch (OperationCanceledException)
            {
                // 페이드 취소됨. 무시.
            }
        }

        private async UniTask FadeOverlayAsync(float targetAlpha, CancellationToken token)
        {
            float startAlpha = overlayImage.color.a;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                if (token.IsCancellationRequested) return;

                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

                Color newColor = overlayImage.color;
                newColor.a = currentAlpha;
                overlayImage.color = newColor;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            Color finalColor = overlayImage.color;
            finalColor.a = targetAlpha;
            overlayImage.color = finalColor;
        }

        private void CancelFade()
        {
            if (_fadeCts != null)
            {
                _fadeCts.Cancel();
                _fadeCts.Dispose();
                _fadeCts = null;
            }
        }
    }
}
