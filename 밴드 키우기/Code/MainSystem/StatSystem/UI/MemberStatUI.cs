using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.StatSystem.BaseStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.StatSystem.UI
{
    public class MemberStatUI : MonoBehaviour
    {
        [Header("스탯 설정")]
        [SerializeField] private MusicRelatedStatsType statType = MusicRelatedStatsType.Composition;

        [Header("UI")]
        [SerializeField] private Image rankImage;
        [SerializeField] private Image gaugeFillImage;
        [SerializeField] private TextMeshProUGUI valueText;

        private MemberType _memberType;
        private bool _memberTypeSet = false;
        private bool _subscribed = false;

        private void Start()
        {
            // OnEnable 시점에 GameStatManager가 아직 없었을 경우를 대비
            TrySubscribe();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (GameStatManager.Instance == null) return;

            GameStatManager.Instance.OnStatsChanged += UpdateUI;
            _subscribed = true;

            if (_memberTypeSet)
                UpdateUI();
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            if (GameStatManager.Instance != null)
                GameStatManager.Instance.OnStatsChanged -= UpdateUI;
            _subscribed = false;
        }

        public void SetMemberType(MemberType memberType)
        {
            _memberType = memberType;
            _memberTypeSet = true;

            // SetMemberType이 호출되는 시점에 아직 구독 안 됐을 수 있음
            TrySubscribe();
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (GameStatManager.Instance == null || !_memberTypeSet) return;

            if (statType == MusicRelatedStatsType.Composition ||
                statType == MusicRelatedStatsType.InstrumentProficiency)
            {
                UpdateMemberStat();
            }
            else if (statType == MusicRelatedStatsType.Lyrics ||
                     statType == MusicRelatedStatsType.Teamwork ||
                     statType == MusicRelatedStatsType.Proficiency ||
                     statType == MusicRelatedStatsType.Melody)
            {
                UpdateActivityStat();
            }
        }

        private void UpdateMemberStat()
        {
            var statData = GameStatManager.Instance.GetMemberStatData(_memberType, statType);

            if (valueText != null)
                valueText.text = statData.currentValue.ToString();

            if (rankImage != null)
            {
                rankImage.enabled = true;
                Sprite rankIcon = GameStatManager.Instance.GetRankIcon(statData.currentRank);
                if (rankIcon != null)
                    rankImage.sprite = rankIcon;
            }

            if (gaugeFillImage != null)
            {
                gaugeFillImage.enabled = true;
                int min = GameStatManager.Instance.GetCurrentRankMin(_memberType, statType);
                int max = GameStatManager.Instance.GetNextRankMax(_memberType, statType);
                float range = max - min;
                float progress = range > 0 ? (float)(statData.currentValue - min) / range : 0f;
                gaugeFillImage.fillAmount = Mathf.Clamp01(progress);
            }
        }

        private void UpdateActivityStat()
        {
            int score = GameStatManager.Instance.GetScore(statType);

            if (valueText != null)
                valueText.text = score.ToString();

            if (rankImage != null)
                rankImage.enabled = false;

            if (gaugeFillImage != null)
                gaugeFillImage.enabled = false;
        }
    }
}