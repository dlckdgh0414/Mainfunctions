using System.Collections.Generic;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.Core.Bus.GameEvents.SynergyEvents
{
    public struct SynergyUpdateEvent : IEvent
    {
        public IReadOnlyDictionary<MemberType, ITraitHolder> Holders { get; }
        
        public SynergyUpdateEvent(IReadOnlyDictionary<MemberType, ITraitHolder> holders)
        {
            Holders = holders;
        }
    }
}