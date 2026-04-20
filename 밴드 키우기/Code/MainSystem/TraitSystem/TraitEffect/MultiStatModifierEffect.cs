using System.Linq;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect
{
    /// <summary>
    /// 기본 효과
    /// </summary>
    public class MultiStatModifierEffect : AbstractTraitEffect
    {
        public override bool IsTargetStat(TraitTarget category) 
            => _ownerTrait.Data.Impacts.Any(i => i.Target == category);

        public override float GetAmount(TraitTarget category, object context = null)
        {
            float total = 0;
            var impacts = _ownerTrait.Data.Impacts;

            for (int i = 0; i < impacts.Count; i++)
            {
                if (impacts[i].Target != category) continue;
                if (!CheckCondition(impacts[i].RequiredTag, context))
                    continue;

                total += GetValue(i);
            }
            return total;
        }

        public CalculationType GetCalcType(TraitTarget category)
        {
            return _ownerTrait.Data.Impacts.FirstOrDefault(i => i.Target == category).CalcType;
        }

        protected virtual bool CheckCondition(string tag, object context)
        {
            if (string.IsNullOrEmpty(tag))
                return true;
            // TODO 여기서 GuitarOnly, Solo 등의 태그 조건 검사 로직 구현
            return true; 
        }
    }
}