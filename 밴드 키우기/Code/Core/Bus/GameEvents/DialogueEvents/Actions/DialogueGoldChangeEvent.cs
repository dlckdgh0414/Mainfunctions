namespace Code.Core.Bus.GameEvents.DialogueEvents.Actions
{
    public struct DialogueGoldChangeEvent : IEvent
    {
        public readonly int Variation;

        public DialogueGoldChangeEvent(int variation)
        {
            Variation = variation;
        }
    }
}
