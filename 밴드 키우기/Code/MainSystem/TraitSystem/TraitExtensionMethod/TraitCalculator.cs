using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Interface;
using Code.MainSystem.TraitSystem.TraitEffect;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.TraitExtensionMethod
{
    public static class TraitCalculator
    {
        public static float GetCalculatedStat(this ITraitHolder holder, TraitTarget category, float baseValue, object context = null)
        {
            var modifiers = holder.GetModifiers<MultiStatModifierEffect>();
    
            float flatBonus = 0f;
            float percentBonus = 0f;
            float finalMultiplier = 1f;

            foreach (var m in modifiers)
            {
                if (!m.IsTargetStat(category)) continue;

                float amount = m.GetAmount(category, context);
                CalculationType type = m.GetCalcType(category);

                switch (type)
                {
                    case CalculationType.Additive:
                        flatBonus += amount;
                        break;
                    case CalculationType.Subtractive:
                        flatBonus -= amount;
                        break;
                    case CalculationType.PercentAdd:
                        percentBonus += amount * 0.01f;
                        break;
                    case CalculationType.PercentSub:
                        percentBonus -= amount * 0.01f;
                        break;
                    case CalculationType.Multiplicative:
                        finalMultiplier *= amount; 
                        break;
                }
            }
            
            float result = (baseValue + flatBonus) * (1f + percentBonus) * finalMultiplier;
            
            return Mathf.Max(0, result);
        }
    }
}