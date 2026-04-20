using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 고위험 루틴 효과
    /// </summary>
    public class HighRiskRoutineEffect : MultiStatModifierEffect
    {
        private int _highRiskStack = 0;
        private const int MaxStack = 7;

        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            switch (trigger)
            {
                case TraitTrigger.OnPracticeSuccess:
                    if (_highRiskStack < MaxStack)
                        _highRiskStack++;
                    break;
                case TraitTrigger.OnRestStarted:
                    _highRiskStack = 0;
                    break;
            }
        }

        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger == TraitTrigger.CalcStatMultiplier && context is StatType)
                return _highRiskStack * (GetValue(0) * 0.01f);

            return 0f;
        }
    }
}