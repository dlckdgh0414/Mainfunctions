using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Manager;

namespace Code.MainSystem.TraitSystem.TraitEffect.SpecialEffect
{
    /// <summary>
    /// 이신 전심 특성
    /// </summary>
    public class TelepathyEffect : MultiStatModifierEffect
    {
        public override float QueryValue(TraitTrigger trigger, object context = null)
        {
            if (trigger != TraitTrigger.CalcEnsembleBonus || 
                context is not List<MemberType> { Count: 2 } members)
                return 0f;

            MemberType partner = members.Find(m => m != _ownerTrait.Owner);
            
            bool hasPartnerTrait = TraitManager.Instance.HasTrait(partner, _ownerTrait.Data.IDHash);
            return GetValue(hasPartnerTrait ? 1 : 0);
        }
    }
}