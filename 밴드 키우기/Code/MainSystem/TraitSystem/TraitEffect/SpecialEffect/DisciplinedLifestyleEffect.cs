using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 규칙적인 생활 효과
    /// </summary>
    public class DisciplinedLifestyleEffect : MultiStatModifierEffect
    {
        private string _lastActionId = string.Empty;

        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger != TraitTrigger.CalcStatMultiplier || context is not (string currentActionId, bool isPreview)) 
                return 0f;
        
            bool isConsecutive = !string.IsNullOrEmpty(_lastActionId) && _lastActionId == currentActionId;
          
            if (!isPreview)
                _lastActionId = currentActionId;

            return isConsecutive ? GetValue(0) : 0f;
        }
    }
}