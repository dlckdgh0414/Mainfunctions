using Code.MainSystem.Dialogue;

namespace Code.Core.Bus.GameEvents.DialogueEvents.Effects
{
    /// <summary>
    /// 캐릭터 연출 효과 종류
    /// </summary>
    public enum CharacterEffectType
    {
        Bounce,
        Shake,
        Jump,
        Excited
    }

    /// <summary>
    /// 캐릭터에게 특정 연출 효과를 적용하기 위한 이벤트 데이터
    /// </summary>
    public struct CharacterEffectEvent : IEvent
    {
        public NameTagPositionType Position;
        public CharacterEffectType EffectType;
        public float Intensity;
        public float Duration;
        public int Count;
        public float Distance; // 새로 추가된 거리 파라미터

        public CharacterEffectEvent(NameTagPositionType position, CharacterEffectType effectType, float intensity, float duration, int count, float distance = 0)
        {
            Position = position;
            EffectType = effectType;
            Intensity = intensity;
            Duration = duration;
            Count = count;
            Distance = distance;
        }
    }
}
