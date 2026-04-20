namespace Code.Core.Bus.GameEvents.DialogueEvents.Audio
{
    /// <summary>
    /// 캐릭터 보이스 재생 이벤트
    /// </summary>
    public struct PlayVoiceEvent : IEvent
    {
        public string VoiceID;
        public PlayVoiceEvent(string voiceID) => VoiceID = voiceID;
    }
}
