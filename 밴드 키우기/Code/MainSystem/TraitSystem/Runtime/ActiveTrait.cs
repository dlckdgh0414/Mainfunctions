using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Code.Core;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.TraitEffect;

namespace Code.MainSystem.TraitSystem.Runtime
{
    public class ActiveTrait
    {
        public TraitDataSO Data { get; private set; }
        public AbstractTraitEffect TraitEffect { get; private set; }
        public List<float> CurrentEffects { get; private set; }

        public MemberType Owner { get; private set; }

        public ActiveTrait(TraitDataSO data, MemberType owner)
        {
            Data = data;
            Owner = owner;
            CurrentEffects = new List<float>(data.Effects);
            
            TraitEffect = data.CreateEffectInstance();
            TraitEffect.Initialize(this);
        }

        public MemberTraitComment GetMemberTraitComment()
            => Data.GetComment(Owner);
        
        public string GetFormattedDescription()
        {
            if (Data is null || string.IsNullOrEmpty(Data.DescriptionEffect))
                return string.Empty;

            if (CurrentEffects == null || CurrentEffects.Count == 0)
                return Data.DescriptionEffect;

            object[] args = CurrentEffects
                .Select(FormatValue)
                .Cast<object>()
                .ToArray();

            try
            {
                return string.Format(Data.DescriptionEffect, args);
            }
            catch (FormatException)
            {
                return Data.DescriptionEffect;
            }
        }

        private string FormatValue(float value)
        {
            return Math.Abs(value) < 1f
                ? (value * 100f).ToString("0.#", CultureInfo.InvariantCulture)
                : value.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}