using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class MusicEnlargementCanvas : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform areaPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("연출 설정")]
        [SerializeField] private float openDuration  = 0.35f;
        [SerializeField] private float closeDuration = 0.25f;
        
        private const float SlideOffsetY = -40f;

        private Vector2 _panelOriginPos;
        private Sequence _currentSeq;

        private void Awake()
        {
            closeButton.onClick.AddListener(HandleCloseTap);

            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;

            _panelOriginPos = areaPanel.anchoredPosition;
        }

        public void OpenTap()
        {
            KillCurrent();
            
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
            areaPanel.anchoredPosition = _panelOriginPos + new Vector2(0f, SlideOffsetY);
            areaPanel.localScale       = Vector3.one * 0.92f;

            _currentSeq = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, openDuration).SetEase(Ease.OutCubic))
                .Join(areaPanel.DOAnchorPos(_panelOriginPos, openDuration).SetEase(Ease.OutBack))
                .Join(areaPanel.DOScale(Vector3.one, openDuration).SetEase(Ease.OutBack))
                .OnStart(() =>
                {
                    canvasGroup.blocksRaycasts = true;
                })
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                });
        }

        private void HandleCloseTap()
        {
            KillCurrent();

            canvasGroup.interactable = false;

            _currentSeq = DOTween.Sequence()
                .Append(canvasGroup.DOFade(0f, closeDuration).SetEase(Ease.InCubic))
                .Join(areaPanel.DOAnchorPos(_panelOriginPos + new Vector2(0f, SlideOffsetY), closeDuration).SetEase(Ease.InBack))
                .Join(areaPanel.DOScale(Vector3.one * 0.92f, closeDuration).SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    canvasGroup.blocksRaycasts = false;
                    areaPanel.anchoredPosition = _panelOriginPos;
                    areaPanel.localScale       = Vector3.one;
                });
        }

        private void KillCurrent()
        {
            _currentSeq?.Kill();
            _currentSeq = null;
        }

        private void OnDestroy()
        {
            KillCurrent();
            closeButton.onClick.RemoveAllListeners();
        }
    }
}