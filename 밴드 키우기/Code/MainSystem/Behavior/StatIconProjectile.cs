using System;
using Code.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.behavior
{
    public class StatIconProjectile : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private float riseUpDuration  = 1.0f;
        [SerializeField] private float fallDownDuration = 2.0f;
        [SerializeField] private float riseUpDistance  = 120f;

        private Action<MusicRelatedStatsType, int> _onReachedTarget;
        private Action<StatIconProjectile>          _onReturnToPool;
        private MusicRelatedStatsType               _statType;
        private int                                 _increaseAmount;
        private Sequence                            _seq;

        public void Initialize(Sprite icon, Vector2 startPos, Vector2 landingOffset,
            MusicRelatedStatsType statType, int increaseAmount,
            Action<MusicRelatedStatsType, int> onReached,
            Transform targetTransform,
            Action<StatIconProjectile> onReturnToPool)
        {
            if (iconImage != null && icon != null)
                iconImage.sprite = icon;

            _onReachedTarget = onReached;
            _onReturnToPool  = onReturnToPool;
            _statType        = statType;
            _increaseAmount  = increaseAmount;

            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Vector2 riseUpPos = startPos + Vector2.up * riseUpDistance;

            _seq = DOTween.Sequence();
            _seq.Append(rectTransform.DOLocalMove(riseUpPos, riseUpDuration).SetEase(Ease.OutQuad));
            _seq.Join(iconImage.DOFade(0f, riseUpDuration));
            _seq.AppendInterval(0.1f);
            _seq.AppendCallback(() =>
            {
                rectTransform.SetParent(targetTransform, false);
                rectTransform.localPosition = (Vector2.up * riseUpDistance) + landingOffset;
                iconImage.color = new Color(1, 1, 1, 0);
            });
            _seq.Append(rectTransform.DOLocalMove(landingOffset, fallDownDuration).SetEase(Ease.Linear));
            _seq.Join(iconImage.DOFade(1f, fallDownDuration * 0.3f));
            _seq.OnComplete(() =>
            {
                _onReachedTarget?.Invoke(statType, increaseAmount);
                _onReturnToPool?.Invoke(this);
            });
        }

        public void Skip()
        {
            if (_seq == null || !_seq.IsActive()) return;
            _seq.Kill();
            _seq = null;
            _onReachedTarget?.Invoke(_statType, _increaseAmount);
            _onReachedTarget = null;
            _onReturnToPool?.Invoke(this);
        }

        private void OnDisable()
        {
            _seq?.Kill();
            _seq = null;
            if (iconImage != null) iconImage.color = Color.white;
            _onReachedTarget = null;
            _onReturnToPool  = null;
        }

        public void ResetProjectile()
        {
            _seq?.Kill();
            _seq = null;
            if (iconImage != null) iconImage.color = Color.white;
            _onReachedTarget = null;
            _onReturnToPool  = null;
            gameObject.SetActive(false);
        }
    }
}