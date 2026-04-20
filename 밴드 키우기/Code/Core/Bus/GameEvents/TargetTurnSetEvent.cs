namespace Code.Core.Bus.GameEvents
{
    public struct TargetTurnSetEvent : IEvent
    {
        public int Value;

        public TargetTurnSetEvent(int value)
        {
            Value = value;
        }
    }
}