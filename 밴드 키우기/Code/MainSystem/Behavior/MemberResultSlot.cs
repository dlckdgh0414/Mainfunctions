using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.StatSystem.BaseStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Code.MainSystem.behavior
{
    public class MemberResultSlot : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image           backgroundImage;
        [SerializeField] private Image           memberIconImage;
        [SerializeField] private TextMeshProUGUI memberNameText;
        [SerializeField] private TextMeshProUGUI statValueText;
        [SerializeField] private Image           rankProgressGaugeImage;
        [SerializeField] private Image           rankImage;

        private MemberType            _memberType;
        private MusicRelatedStatsType _statType;
        private int                   _beforeValue;
        private int                   _afterValue;
        private Sequence              _statAnimSequence;

        private void OnDestroy()
        {
            _statAnimSequence?.Kill();
        }

        public void SetPersonalColor(Color color)
        {
            if (backgroundImage != null)
                backgroundImage.color = color;
        }

        public void Setup(MemberDataSO memberData, StatType statType, int earned)
        {
            _memberType = memberData.memberType;
            _statType   = (MusicRelatedStatsType)statType;

            if (memberIconImage != null && memberData.IconSprite != null)
                memberIconImage.sprite = memberData.IconSprite;

            if (memberNameText != null)
                memberNameText.text = memberData.memberName;

            var currentData = GameStatManager.Instance.GetMemberStatData(_memberType, _statType);
            _afterValue  = currentData.currentValue;
            _beforeValue = _afterValue - earned;

            if (statValueText != null)
                statValueText.text = _beforeValue.ToString();

            UpdateRankProgressGaugeWithValue(_beforeValue);

            if (rankImage != null)
            {
                var initialRank = GameStatManager.Instance.CalculateRankPublic(_beforeValue);
                rankImage.sprite = GameStatManager.Instance.GetRankIcon(initialRank);
            }
        }

        public void ApplyStatIncrease(int earned, float duration)
        {
            _statAnimSequence?.Kill();
            _statAnimSequence = DOTween.Sequence();

            _statAnimSequence.Append(DOTween.To(() => _beforeValue, x =>
            {
                if (statValueText != null) statValueText.text = x.ToString();
                UpdateRankProgressGaugeWithValue(x);
            }, _afterValue, duration).SetEase(Ease.OutQuad));

            _statAnimSequence.OnComplete(() =>
            {
                var finalRank = GameStatManager.Instance.CalculateRankPublic(_afterValue);
                if (rankImage != null)
                    rankImage.sprite = GameStatManager.Instance.GetRankIcon(finalRank);
            });
        }
        
        public void SkipToFinal(int earned)
        {
            _statAnimSequence?.Kill();

            if (statValueText != null) statValueText.text = _afterValue.ToString();
            UpdateRankProgressGaugeWithValue(_afterValue);

            var finalRank = GameStatManager.Instance.CalculateRankPublic(_afterValue);
            if (rankImage != null)
                rankImage.sprite = GameStatManager.Instance.GetRankIcon(finalRank);
        }

        private void UpdateRankProgressGaugeWithValue(int value)
        {
            if (rankProgressGaugeImage == null) return;

            int currentMin = GameStatManager.Instance.GetCurrentRankMin(_memberType, _statType);
            int nextMax    = GameStatManager.Instance.GetNextRankMax(_memberType, _statType);

            float range    = nextMax - currentMin;
            float progress = range > 0 ? (float)(value - currentMin) / range : 0f;
            rankProgressGaugeImage.fillAmount = Mathf.Clamp01(progress);
        }
    }
}