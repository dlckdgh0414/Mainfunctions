using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class TeamworkEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category) 
            => category == TraitTag.Teamwork;

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnNoteHit || context is not float harmony)
                return 0f;
            
            float bonusRatio = GetTieredValue() / 100f;
            
            return harmony * bonusRatio;
        }
    }
}