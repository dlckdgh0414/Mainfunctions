namespace Code.Core.Bus.GameEvents.DialogueEvents.Audio
{
    /// <summary>
    /// 효과음 재생을 요청하는 이벤트
    /// </summary>
    public struct PlaySFXEvent : IEvent
    {
        public string SFXID;
        public PlaySFXEvent(string sfxId) => SFXID = sfxId;
    }
}
