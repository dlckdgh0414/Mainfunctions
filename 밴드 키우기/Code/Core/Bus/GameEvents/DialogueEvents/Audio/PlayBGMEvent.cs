namespace Code.Core.Bus.GameEvents.DialogueEvents.Audio
{
    /// <summary>
    /// BGM 재생을 요청하는 이벤트
    /// </summary>
    public struct PlayBGMEvent : IEvent
    {
        public string BGMID;
        public PlayBGMEvent(string bgmId) => BGMID = bgmId;
    }
}
