using Code.MainSystem.TraitSystem.Data;


namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 연습 루틴 효과
    /// </summary>
    public class PracticeRoutineEffect : MultiStatModifierEffect
    {
        private readonly string _lastActionId = string.Empty;

        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger != TraitTrigger.CalcStatMultiplier || context is not string currentActionId)
                return base.QueryValue(trigger, context);

            float bonus = 0f;
            
            if (!string.IsNullOrEmpty(_lastActionId) && _lastActionId == currentActionId)
                bonus = GetValue(0);
            
            return bonus;
        }
    }
}