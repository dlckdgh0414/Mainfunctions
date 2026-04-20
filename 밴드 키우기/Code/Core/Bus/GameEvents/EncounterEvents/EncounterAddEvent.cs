using Code.MainSystem.Encounter;

namespace Code.Core.Bus.GameEvents.EncounterEvents
{
    public struct EncounterAddEvent : IEvent
    {
        public EncounterDataSO EncounterData;

        public EncounterAddEvent(EncounterDataSO encounterData)
        {
            EncounterData = encounterData;
        }
    }
}