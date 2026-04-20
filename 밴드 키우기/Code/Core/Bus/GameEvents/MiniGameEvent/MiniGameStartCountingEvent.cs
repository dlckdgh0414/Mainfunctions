namespace Code.Core.Bus.GameEvents.MiniGameEvent
{
    public struct MiniGameStartCountingEvent : IEvent
    {
        public int Counting;

        public MiniGameStartCountingEvent(int counting)
        {
            Counting = counting;
        }
    }
}