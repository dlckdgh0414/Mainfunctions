namespace Code.Core.Bus.GameEvents.TurnEvents
{
    public struct TurnReturnEvent : IEvent
    {
        public int Value;

        public TurnReturnEvent(int value)
        {
            Value = value;
        }
    }
}