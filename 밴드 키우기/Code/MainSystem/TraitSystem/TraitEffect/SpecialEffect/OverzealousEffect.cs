using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 지나친 열정 효과
    /// </summary>
    public class OverzealousEffect : MultiStatModifierEffect
    {
        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            return trigger switch
            {
                TraitTrigger.CalcConditionCost 
                    => GetValue(0),
                
                TraitTrigger.CheckAdditionalAction
                    => RollAdditionalAction(),
                
                _ => base.QueryValue(trigger, context)
            };
        }

        private float RollAdditionalAction()
        {
            return Random.Range(0f, 100f) < GetValue(2) ? 1f : 0f;
        }
    }
}