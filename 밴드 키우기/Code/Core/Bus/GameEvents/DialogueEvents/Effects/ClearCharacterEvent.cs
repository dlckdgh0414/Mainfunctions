using Code.MainSystem.Dialogue;

namespace Code.Core.Bus.GameEvents.DialogueEvents.Effects
{
    /// <summary>
    /// 특정 위치의 캐릭터를 화면에서 제거하는 이벤트
    /// </summary>
    public struct ClearCharacterEvent : IEvent
    {
        public NameTagPositionType Position;
        public ClearCharacterEvent(NameTagPositionType position) => Position = position;
    }
}