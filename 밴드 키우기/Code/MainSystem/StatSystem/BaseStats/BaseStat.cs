using System;
using System.Threading.Tasks;
using Code.Core.Addressable;
using UnityEngine;

namespace Code.MainSystem.StatSystem.BaseStats
{
    public class BaseStat
    {
        public StatType StatType { get; private set; }
        public string StatName { get; private set; }
        public int CurrentValue { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public Sprite StatIcon { get; private set; }
        public StatRankTable RankTable { get; private set; }

        /// <summary>
        /// 현재 누적 포인트가 속한 등급
        /// </summary>
        public StatRankType CurrentRank { get; private set; } = StatRankType.None;

        /// <summary>
        /// 스탯 수치가 변경되었을 때 발행 (UI 갱신용)
        /// </summary>
        public event Action<BaseStat> OnStatChanged;

        /// <summary>
        /// 랭크가 변경되었을 때 발행 (oldRank, newRank)
        /// </summary>
        public event Action<BaseStat, StatRankType, StatRankType> OnRankUp;

        public Sprite CurrentRankIcon =>
            RankTable?.GetRankIcon(CurrentValue) is { } iconRef && iconRef.RuntimeKeyIsValid()
                ? GameResourceManager.Instance.Load<Sprite>(iconRef.RuntimeKey.ToString())
                : null;

        /// <summary>
        /// 현재 등급의 시작 임계값 (게이지 Min)
        /// </summary>
        public int CurrentRankMin
            => RankTable?.GetCurrentRankThreshold(CurrentValue) ?? MinValue;

        /// <summary>
        /// 다음 등급의 시작 임계값 (게이지 Max). 최고 등급이면 MaxValue 반환.
        /// </summary>
        public int NextRankMax
            => RankTable?.GetNextRankThreshold(CurrentValue) ?? MaxValue;

        public BaseStat(StatData data)
        {
            StatType = data.statType;
            CurrentValue = data.currentValue;
            StatName = data.statName;

            MinValue = 0;
            MaxValue = int.MaxValue;
        }

        public async Task InitializeAssetsAsync(StatData data)
        {
            var rm = GameResourceManager.Instance;

            if (data.statIcon.RuntimeKeyIsValid())
                StatIcon = await rm.LoadAsync<Sprite>(data.statIcon.RuntimeKey.ToString());

            if (data.dateRange != null && data.dateRange.RuntimeKeyIsValid())
            {
                var rangeSo = await rm.LoadAsync<StatValueRange>(data.dateRange.RuntimeKey.ToString());
                if (rangeSo != null)
                {
                    MinValue = rangeSo.Min;
                    MaxValue = rangeSo.Max;
                    ApplyValue(CurrentValue);
                }
            }

            if (data.rankTable.RuntimeKeyIsValid())
            {
                RankTable = await rm.LoadAsync<StatRankTable>(data.rankTable.RuntimeKey.ToString());
                if (RankTable != null)
                {
                    await RankTable.LoadAllRankIconsAsync();
                    // 초기 랭크 설정 (이벤트 발행 없이)
                    CurrentRank = RankTable.GetCurrentRank(CurrentValue);
                }
            }
        }

        // ── 외부에서 호출하는 값 변경 메서드 ──────────────────────────────

        public void PlusValue(int value)
            => ApplyValue(CurrentValue + value);

        public void MultiplyValue(int value)
            => ApplyValue(CurrentValue * value);

        public void SubtractValue(int value)
            => ApplyValue(CurrentValue - value);

        public void PlusPercentValue(int percent)
            => ApplyValue(CurrentValue + RoundPercent(percent));

        public void MinusPercentValue(int percent)
            => ApplyValue(CurrentValue - RoundPercent(percent));

        public void AddMaxValue(int amount)
        {
            MaxValue += amount;
            ApplyValue(CurrentValue);
        }

        public void SubtractMaxValue(int amount)
        {
            MaxValue -= amount;
            ApplyValue(CurrentValue);
        }

        // ── 내부 처리 ──────────────────────────────────────────────────────

        private void ApplyValue(int newValue)
        {
            int clamped = Mathf.Clamp(newValue, MinValue, MaxValue);
            if (CurrentValue == clamped)
                return;

            CurrentValue = clamped;

            CheckRankChange();
            OnStatChanged?.Invoke(this);
        }

        /// <summary>
        /// 값 변경 후 랭크가 바뀌었는지 체크하고, 바뀌었으면 OnRankUp 발행
        /// </summary>
        private void CheckRankChange()
        {
            if (RankTable == null)
                return;

            StatRankType newRank = RankTable.GetCurrentRank(CurrentValue);
            if (newRank == CurrentRank)
                return;

            StatRankType oldRank = CurrentRank;
            CurrentRank = newRank;
            OnRankUp?.Invoke(this, oldRank, newRank);
        }

        private int RoundPercent(float percent)
            => Mathf.RoundToInt(CurrentValue * (percent / 100f));
    }
}