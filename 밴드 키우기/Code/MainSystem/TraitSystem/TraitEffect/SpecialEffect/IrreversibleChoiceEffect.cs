using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 되돌릴 수 없는 선택 효과
    /// </summary>
    public class IrreversibleChoiceEffect : MemberStatEffect
    {
        private float _maxStatBonusPercent;

        public override void Initialize(ActiveTrait trait)
        {
            base.Initialize(trait);
            _maxStatBonusPercent = GetValue(0);
        }

        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            switch (trigger)
            {
                case TraitTrigger.OnTraitAdded:
                    ModifyStatMaxValues(true);
                    break;
                case TraitTrigger.OnTraitRemoved:
                    ModifyStatMaxValues(false);
                    break;
            }
        }

        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger == TraitTrigger.CalcStatMultiplier && context is TraitTarget.Condition)
                return -1f;

            return 0f;
        }

        private void ModifyStatMaxValues(bool isAdding)
        {
            // StatManager statManager = StatManager.Instance;
            // if (statManager == null)
            //     return;
            //
            // StatType[] majorStats = GetMajorStatsByMember(_ownerTrait.Owner);
            // if (majorStats == null)
            //     return;
            //
            // foreach (var type in majorStats)
            // {
            //     BaseStat stat = statManager.GetMemberStat(_ownerTrait.Owner, type);
            //     if (stat == null) continue;
            //
            //     int amount = Mathf.RoundToInt(stat.MaxValue * _maxStatBonusPercent);
            //
            //     if (isAdding) stat.AddMaxValue(amount);
            //     else stat.SubtractMaxValue(amount);
            // }
        }
    }
}