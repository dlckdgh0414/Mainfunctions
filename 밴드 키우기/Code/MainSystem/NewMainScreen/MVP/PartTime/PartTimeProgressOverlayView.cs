using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 진행 중 입력 차단 오버레이 뷰.
    /// </summary>
    public class PartTimeProgressOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private RectTransform progressBarFill;

        private Tween _progressTween;

        private void Awake()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        /// <summary>
        /// 오버레이 표시 수행.
        /// </summary>
        /// <param name="message">표시할 메시지.</param>
        public void Show(string message)
        {
            if (messageText != null)
            {
                messageText.SetText(message);
            }

            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }
        }

        /// <summary>
        /// 진행 게이지 연출 수행.
        /// </summary>
        /// <param name="duration">진행 완료까지 소요 시간.</param>
        public async UniTask PlayProgressAsync(float duration)
        {
            if (duration <= 0f)
            {
                if (progressBarFill != null)
                {
                    progressBarFill.localScale = Vector3.one;
                }

                return;
            }

            if (progressBarFill == null)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
                return;
            }

            KillProgressTween();
            progressBarFill.localScale = new Vector3(0f, 1f, 1f);

            _progressTween = progressBarFill
                .DOScaleX(1f, duration)
                .SetEase(Ease.Linear);

            await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
            _progressTween = null;
        }

        /// <summary>
        /// 오버레이 숨김 수행.
        /// </summary>
        public void Hide()
        {
            KillProgressTween();

            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        /// <summary>
        /// 진행 게이지 트윈 정리 수행.
        /// </summary>
        private void KillProgressTween()
        {
            if (_progressTween == null)
            {
                return;
            }

            if (_progressTween.IsActive())
            {
                _progressTween.Kill();
            }

            _progressTween = null;
        }

        private void OnDestroy()
        {
            KillProgressTween();
        }
    }
}
