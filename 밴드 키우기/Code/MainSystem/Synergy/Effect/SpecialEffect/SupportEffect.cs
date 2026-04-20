using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class SupportEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category) 
            => category.HasFlag(TraitTag.Support);

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            return trigger is SynergyTrigger.OnFeverStart or SynergyTrigger.OnMemberSwitch ? GetTieredValue() : 0f;
        }
    }
}