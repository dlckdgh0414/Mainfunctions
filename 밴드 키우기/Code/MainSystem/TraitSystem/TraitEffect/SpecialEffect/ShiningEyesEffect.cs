using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 반짝이는 눈 효과
    /// </summary>
    public class ShiningEyesEffect : MultiStatModifierEffect
    {
        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            return trigger != TraitTrigger.CheckAdditionalAction
                ? base.QueryValue(trigger, context)
                : GetValue(1); // 계산은 아래 함수에서 했기 때문에 CheckAdditionalAction만 확인하고 값만 반환한다.
        }

        public override bool CheckCondition(TraitTrigger trigger, object context = null)
        {
            if (context is >= 60f)
                return Random.Range(0f, 100f) < GetValue(0);

            return false;
        }
    }
}