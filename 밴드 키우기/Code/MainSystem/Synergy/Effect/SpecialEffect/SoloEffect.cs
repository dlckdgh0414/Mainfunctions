using Code.Core.Bus;
// using Code.Core.Bus.GameEvents.RhythmEvents;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class SoloEffect : AbstractSynergyEffect
    {
        public override bool IsTargetStat(TraitTag category)
            => category.HasFlag(TraitTag.Solo);

        public override void OnTrigger(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnNoteHit)
                return;

            // float statValue = StatManager.Instance.GetMemberStat(MemberType.Guitar, StatType.GuitarConcentration).CurrentValue;
            // int recovery = Mathf.FloorToInt(statValue / 100f) * 1;
            //Bus<RequestPopularityChangeEvent>.Raise(new RequestPopularityChangeEvent(recovery));
        }

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            return trigger == SynergyTrigger.OnNoteHit ? GetTieredValue() : 1.0f;
        }
    }
}