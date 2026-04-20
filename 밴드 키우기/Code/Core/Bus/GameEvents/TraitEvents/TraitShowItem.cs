using Code.MainSystem.TraitSystem.Runtime;

namespace Code.Core.Bus.GameEvents.TraitEvents
{
    public struct TraitShowItem : IEvent
    {
        public ActiveTrait Trait;

        public TraitShowItem(ActiveTrait trait)
        {
            Trait = trait;
        }
    }
}