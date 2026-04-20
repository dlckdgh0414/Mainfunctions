using System.Collections.Generic;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    public class MoodMaker : MemberStatEffect
    {
        public override void OnTrigger(TraitTrigger trigger, object context = null)
        {
            if (trigger == TraitTrigger.EnsembleMental && context is EnsembleContext ctx)
            {
                foreach (var member in ctx.Participants)
                {
                    // int bonus = (int)GetValue(0);
                    // ctx.DeltaDict[(member, StatType.Mental)] =
                    //     ctx.DeltaDict.GetValueOrDefault((member, StatType.Mental)) + bonus;
                    //
                    // StatManager.Instance.GetMemberStat(member, StatType.Mental)?.PlusValue(bonus);
                }
            }
        }
    }
}