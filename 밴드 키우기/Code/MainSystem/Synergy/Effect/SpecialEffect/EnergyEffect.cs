using Code.Core.Bus;
// using Code.Core.Bus.GameEvents.RhythmEvents;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class EnergyEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category) 
            => category.HasFlag(TraitTag.Energy);

        public override void OnTrigger(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnComboChanged || context is not int combo)
                return;
            
            int interval = (int)GetTieredValue(); 
            if (combo > 0 && combo % interval == 0)
            {
                //float totalMetalAndCondition = 0;
                // for (int i = 0; i < (int)MemberType.Team; i++)
                // {
                //     // totalMetalAndCondition += StatManager.Instance.GetMemberStat((MemberType)i, StatType.Mental).CurrentValue;
                //     // totalMetalAndCondition += StatManager.Instance.GetMemberStat((MemberType)i, StatType.Condition).CurrentValue;
                // }
                
                //Bus<RequestPopularityChangeEvent>.Raise(new RequestPopularityChangeEvent((int)totalMetalAndCondition / 10));
            }
        }
    }
}