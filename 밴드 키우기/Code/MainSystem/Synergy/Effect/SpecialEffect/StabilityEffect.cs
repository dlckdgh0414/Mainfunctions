using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class StabilityEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category)
            => category.HasFlag(TraitTag.Stability);

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            return trigger == SynergyTrigger.OnMissAvoided ? 1f : 0f;
        }
    }
}