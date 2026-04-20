using Code.MainSystem.Encounter;

namespace Code.Core.Bus.GameEvents.EncounterEvents
{
    public struct EncounterCheckEvent : IEvent
    {
        public EncounterConditionType Type;

        public EncounterCheckEvent(EncounterConditionType type)
        {
            Type = type;
        }
    }
    
}