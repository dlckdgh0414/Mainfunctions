using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 록스피릿 효과
    /// </summary>
    public class RockSpiritEffect : MultiStatModifierEffect
    {
        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            return trigger == TraitTrigger.CalcTrainingReward
                ? GetValue(0)
                : base.QueryValue(trigger, context);
        }
    }
}