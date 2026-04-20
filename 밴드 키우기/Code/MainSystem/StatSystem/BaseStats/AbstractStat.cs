using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.Addressable;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.StatSystem.BaseStats
{
    public abstract class AbstractStat : MonoBehaviour
    {
        [SerializeField] private string statDataLabel;

        protected readonly Dictionary<StatType, BaseStat> _stats = new();
        protected bool _isInitialized;

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            Debug.Assert(!string.IsNullOrEmpty(statDataLabel), $"{gameObject.name}: Label이 비어있습니다!");

            try
            {
                await GameResourceManager.Instance.LoadAllAsync<StatData>(statDataLabel);

                var locations = await Addressables
                    .LoadResourceLocationsAsync(statDataLabel, typeof(StatData)).Task;

                foreach (var location in locations)
                {
                    StatData data = GameResourceManager.Instance.Load<StatData>(location.PrimaryKey);
                    if (data == null)
                        continue;

                    var baseStat = new BaseStat(data);
                    await baseStat.InitializeAssetsAsync(data);
                    _stats[data.statType] = baseStat;
                }

                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{name}] 초기화 실패: {e.Message}");
            }
        }

        public void ApplyStatIncrease(StatType statType, float value)
        {
            if (_stats.TryGetValue(statType, out var stat))
                stat.PlusValue((int)value);
        }

        public BaseStat GetStat(StatType statType)
            => _stats.GetValueOrDefault(statType);

        public IReadOnlyDictionary<StatType, BaseStat> GetAllStats()
            => _stats;
    }
}