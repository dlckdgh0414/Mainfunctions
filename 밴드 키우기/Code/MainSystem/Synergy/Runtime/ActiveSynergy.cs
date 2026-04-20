using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Code.MainSystem.Synergy.Data;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Runtime
{
    public class ActiveSynergy
    {
        public TraitSynergyDataSO SynergyData { get; }

        public int CurrentCount => _collectedTraits.Count;
        public int MaxCount => _validTraitsForThisSynergy.Count;
        
        private readonly List<TraitDataSO> _validTraitsForThisSynergy;
        private List<TraitDataSO> _collectedTraits = new();

        public ActiveSynergy(TraitSynergyDataSO synergyData, List<TraitDataSO> allTraitsWithTag)
        {
            SynergyData = synergyData;
            _validTraitsForThisSynergy = allTraitsWithTag ?? new List<TraitDataSO>();
        }
        
        public string GetFormattedDescription()
        {
            if (SynergyData == null || string.IsNullOrEmpty(SynergyData.Description))
                return string.Empty;

            if (SynergyData.EffectValues == null || SynergyData.EffectValues.Count == 0)
                return SynergyData.Description;
            
            object[] args = SynergyData.EffectValues
                .Select(FormatValue)
                .Cast<object>()
                .ToArray();

            try
            {
                return string.Format(SynergyData.Description, args);
            }
            catch (FormatException)
            {
                return SynergyData.Description;
            }
        }

        private string FormatValue(float value)
        {
            return Math.Abs(value) < 1f
                ? (value * 100f).ToString("0.#", CultureInfo.InvariantCulture)
                : value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        public void UpdateState(IEnumerable<TraitDataSO> currentActiveTraits)
        {
            _collectedTraits = currentActiveTraits
                .Where(t => t.TraitTag.HasFlag(SynergyData.TargetTag))
                .Distinct()
                .ToList();
        }
        
        public bool IsTraitOwned(TraitDataSO trait)
        {
            return _collectedTraits.Contains(trait);
        }
    }
}