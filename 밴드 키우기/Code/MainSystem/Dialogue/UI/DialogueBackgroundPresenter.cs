using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 배경 이미지 교체 및 페이드 연출을 담당하는 Presenter (UniTask 기반)
    /// </summary>
    public class DialogueBackgroundPresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image mainBackground;
        [SerializeField] private Image fadeBackground; // 페이드 연출용 보조 이미지

        [Header("Settings")]
        [Range(0.1f, 1.0f)]
        [SerializeField] private float fadeDuration = 0.3f;

        private Sprite _lastSprite;
        private CancellationTokenSource _fadeCts;

        private void OnEnable()
        {
            Bus<DialogueProgressEvent>.OnEvent += OnDialogueProgress;
        }

        private void OnDisable()
        {
            Bus<DialogueProgressEvent>.OnEvent -= OnDialogueProgress;
            CancelFade();
        }

        private void OnDialogueProgress(DialogueProgressEvent evt)
        {
            if (evt.BackgroundImage == null || evt.BackgroundImage == _lastSprite) return;

            // 배경이 바뀌었을 때만 페이드 실행
            CancelFade();
            _fadeCts = new CancellationTokenSource();
            FadeBackgroundAsync(evt.BackgroundImage, _fadeCts.Token).Forget();
            
            _lastSprite = evt.BackgroundImage;
        }

        private async UniTaskVoid FadeBackgroundAsync(Sprite newSprite, CancellationToken token)
        {
            if (mainBackground == null || fadeBackground == null) return;

            try
            {
                // 1. 준비: 기존 배경이 있는 경우에만 페이드용 복제 수행
                if (mainBackground.sprite != null)
                {
                    fadeBackground.sprite = mainBackground.sprite;
                    fadeBackground.color = Color.white;
                    fadeBackground.gameObject.SetActive(true);
                }
                else
                {
                    fadeBackground.gameObject.SetActive(false);
                }

                // 2. 새 배경 준비 (투명한 상태로 시작)
                mainBackground.sprite = newSprite;
                mainBackground.color = new Color(1, 1, 1, 0);

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    // 다음 프레임까지 대기 (대사 진행과 충돌하지 않도록 PlayerLoopTiming.Update 사용)
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    
                    elapsed += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(elapsed / fadeDuration);

                    // 기존 배경 Fade Out (활성화된 경우에만), 새 배경 Fade In
                    if (fadeBackground.gameObject.activeSelf)
                    {
                        fadeBackground.color = new Color(1, 1, 1, 1 - normalizedTime);
                    }
                    mainBackground.color = new Color(1, 1, 1, normalizedTime);
                }

                // 3. 마무리
                mainBackground.color = Color.white;
                fadeBackground.gameObject.SetActive(false);
            }
            catch (System.OperationCanceledException)
            {
                // 취소 시 상태 정리
            }
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
