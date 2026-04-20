using Code.Core;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 흐름 되살리기 효과
    /// </summary>
    public class GrooveRestorationEffect : MultiStatModifierEffect
    {
        private bool _isBuffered = false;

        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            switch (trigger)
            {
                case TraitTrigger.OnRestStarted:
                    _isBuffered = true;
                    break;
                case TraitTrigger.OnPracticeSuccess:
                    if (_isBuffered)
                        _isBuffered = false;
                    break;
            }
        }

        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger != TraitTrigger.CalcStatMultiplier)
                return 0f;

            bool isEnsemble = context is PracticenType.Team;

            if (_isBuffered && isEnsemble)
                return GetValue(0) * 0.01f;
            
            
            return 0f;
        }
    }
}