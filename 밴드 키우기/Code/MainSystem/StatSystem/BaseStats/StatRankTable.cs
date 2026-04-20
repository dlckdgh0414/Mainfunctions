using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Core.Addressable;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.StatSystem.BaseStats
{
    [Serializable]
    public struct StatRank
    {
        public StatRankType RankName;
        public int Threshold;
        public AssetReferenceSprite RankIconReference;
    }

    [CreateAssetMenu(fileName = "Stat rank", menuName = "SO/Stat/Stat rank", order = 0)]
    public class StatRankTable : ScriptableObject
    {
        public List<StatRank> Ranks = new List<StatRank>();

        /// <summary>
        /// 현재 누적 값이 속한 StatRankType을 반환합니다.
        /// </summary>
        public StatRankType GetCurrentRank(int value)
        {
            if (Ranks == null || Ranks.Count == 0)
                return StatRankType.None;

            for (int i = Ranks.Count - 1; i >= 0; i--)
            {
                if (value >= Ranks[i].Threshold)
                    return Ranks[i].RankName;
            }

            return StatRankType.None;
        }

        /// <summary>
        /// 현재 누적 값 기준 해당 등급의 아이콘 레퍼런스를 반환합니다.
        /// </summary>
        public AssetReferenceSprite GetRankIcon(int value)
        {
            if (Ranks == null || Ranks.Count == 0)
                return null;

            for (int i = Ranks.Count - 1; i >= 0; i--)
                if (value >= Ranks[i].Threshold)
                    return Ranks[i].RankIconReference;

            return Ranks[0].RankIconReference;
        }

        /// <summary>
        /// 특정 RankType의 아이콘 레퍼런스를 직접 반환합니다. (랭크업 UI 등에 활용)
        /// </summary>
        public AssetReferenceSprite GetRankIconByType(StatRankType rankType)
        {
            foreach (var rank in Ranks)
            {
                if (rank.RankName == rankType)
                    return rank.RankIconReference;
            }
            return null;
        }

        /// <summary>
        /// 다음 등급의 Threshold 값을 반환합니다. 최고 등급이면 null을 반환합니다.
        /// </summary>
        public int? GetNextRankThreshold(int currentValue)
        {
            for (int i = 0; i < Ranks.Count; i++)
            {
                if (Ranks[i].Threshold > currentValue)
                    return Ranks[i].Threshold;
            }
            return null;
        }

        /// <summary>
        /// 현재 등급의 시작 Threshold 값을 반환합니다.
        /// </summary>
        public int? GetCurrentRankThreshold(int currentValue)
        {
            for (int i = Ranks.Count - 1; i >= 0; i--)
            {
                if (currentValue >= Ranks[i].Threshold)
                    return Ranks[i].Threshold;
            }
            return null;
        }

        public async Task LoadAllRankIconsAsync()
        {
            var rm = GameResourceManager.Instance;
            var tasks = Ranks.Where(r => r.RankIconReference.RuntimeKeyIsValid())
                .Select(r => rm.LoadAsync<Sprite>(r.RankIconReference.RuntimeKey.ToString()));
            await Task.WhenAll(tasks);
        }
    }
}