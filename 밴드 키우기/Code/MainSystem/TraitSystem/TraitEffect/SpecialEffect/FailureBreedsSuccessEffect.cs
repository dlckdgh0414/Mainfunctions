using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 실패는 성공의 어머니 효과
    /// </summary>
    public class FailureBreedsSuccessEffect : MultiStatModifierEffect
    {
        private float _currentInspiration;

        public override bool IsTargetStat(TraitTarget category) => false;
        public override float GetAmount(TraitTarget category, object context = null) => 0;

        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            switch (trigger)
            {
                case TraitTrigger.OnPracticeFailed:
                    _currentInspiration += GetValue(0);
                    break;
                case TraitTrigger.OnPracticeSuccess:
                    if (_currentInspiration >= GetValue(1))
                        _currentInspiration = 0;
                    break;
            }
        }

        public override bool CheckCondition(TraitTrigger trigger, object context = null)
        {
            if (trigger == TraitTrigger.CheckSuccessGuaranteed)
                return _currentInspiration >= GetValue(1);

            return false;
        }
    }
}