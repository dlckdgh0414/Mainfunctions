using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 서로서로 도와요 효과
    /// </summary>
    public class MutualAssistanceEffect : MemberStatEffect
    {
        private int _addValue;

        public override void Initialize(ActiveTrait trait)
        {
            base.Initialize(trait);
            _addValue = (int)GetValue(0);
        }

        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            if (trigger != TraitTrigger.OnEnsembleSuccess) 
                return;
        
            if (context is Dictionary<(MemberType, StatType), int> statDeltaDict)
                ApplyLowestStatBonus(statDeltaDict);
        }

        private void ApplyLowestStatBonus(Dictionary<(MemberType, StatType), int> statDeltaDict)
        {
            // StatManager statManager = StatManager.Instance;
            // if (statManager == null) return;
            //
            // MemberType owner = _ownerTrait.Owner;
            // StatType[] targetStats = GetMajorStatsByMember(owner);
            //
            // if (targetStats == null || targetStats.Length == 0) return;
            //
            // StatType lowestStatType = targetStats[0];
            // float lowestValue = float.MaxValue;
            //
            // foreach (var type in targetStats)
            // {
            //     BaseStat stat = statManager.GetMemberStat(owner, type);
            //     if (stat == null || !(stat.CurrentValue < lowestValue))
            //         continue;
            //
            //     lowestValue = stat.CurrentValue;
            //     lowestStatType = type;
            // }
            //
            // BaseStat targetStat = statManager.GetMemberStat(owner, lowestStatType);
            // if (targetStat == null)
            //     return;
            //
            // targetStat.PlusValue(_addValue);
            // var key = (owner, lowestStatType);
            // statDeltaDict[key] = statDeltaDict.GetValueOrDefault(key) + _addValue;
        }
    }
}