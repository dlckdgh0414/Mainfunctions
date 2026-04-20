namespace Code.Core.Bus.GameEvents
{
    public struct ShowTextEvent : IEvent
    {
        public string EventText;
        public int EventAmount;

        public ShowTextEvent(string eventText, int eventAmount = 0)
        {
            EventText = eventText;
            EventAmount = eventAmount;
        }
    }
}