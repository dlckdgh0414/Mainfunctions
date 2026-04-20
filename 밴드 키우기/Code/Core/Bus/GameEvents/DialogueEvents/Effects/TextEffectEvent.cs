using System.Collections.Generic;
using Code.MainSystem.Dialogue.UI;

namespace Code.Core.Bus.GameEvents.DialogueEvents.Effects
{
    /// <summary>
    /// 텍스트 연출(Shrink 등) 정보 전달 이벤트
    /// </summary>
    public struct TextEffectEvent : IEvent
    {
        public List<TextEffectData> Effects;

        public TextEffectEvent(List<TextEffectData> effects)
        {
            Effects = effects;
        }
    }
}
