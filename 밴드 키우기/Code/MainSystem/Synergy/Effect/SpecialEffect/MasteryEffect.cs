// using Code.MainSystem.Rhythm.Data;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class MasteryEffect : AbstractSynergyEffect
    {
        private int _prepStack = 0;

        public override void OnReset()
        {
            _prepStack = 0;
        }
        
        public override bool IsTargetStat(TraitTag category)
            => category.HasFlag(TraitTag.Mastery);

        public override void OnTrigger(SynergyTrigger trigger, object context = null)
        {
            if (trigger == SynergyTrigger.OnActionExecuted)
                _prepStack++;
        }

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnNoteHit || _prepStack <= 0 || context is not int index)
                return 0f;

            /*
            JudgementType type = (JudgementType)index;
            if (type is JudgementType.Miss or JudgementType.Bad or JudgementType.Good)
            {
                _prepStack--;
                return 1f;
            }
            */
            
            return 0f;
        }
    }
}