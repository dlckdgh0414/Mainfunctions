// using Code.MainSystem.Rhythm.Data;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class GeniusEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category)
            => category.HasFlag(TraitTag.Genius);

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnNoteHit || context is not int judge) 
                return 1.0f;

            /*
            JudgementType type = (JudgementType)judge;

            return type switch
            {
                JudgementType.Perfect  => GetTieredValue(),
                JudgementType.Miss  => 1.2f,
                _ => 1.0f
            };
            */
            return 1.0f;
        }
    }
}