using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;

namespace Code.MainSystem.TraitSystem.Interface
{
    public interface ITraitHolder : IModifierProvider, ITraitTrigger
    {
        public MemberType MemberType { get; }
        
        int TotalPoint { get; }
        int MaxPoints { get; }
        IReadOnlyList<ActiveTrait> ActiveTraits { get; }
        void RestoreTraits(IEnumerable<ActiveTrait> traits);
        
        void BeginAdjustment(TraitDataSO pendingTrait);
        void EndAdjustment();
        
        bool IsAdjusting { get; }

        void AddTrait(TraitDataSO newTrait);
        void RemoveActiveTrait(ActiveTrait trait);
    }
}