using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.StatSystem.BaseStats
{
    [CreateAssetMenu(fileName = "Stat data", menuName = "SO/Stat/Stat data", order = 0)]
    public class StatData : ScriptableObject
    {
        public StatType statType;
        public string statName;
        public int currentValue;

        public AssetReferenceSprite statIcon;

        public AssetReferenceT<StatValueRange> dateRange;
        public AssetReferenceT<StatRankTable> rankTable;
    }
}