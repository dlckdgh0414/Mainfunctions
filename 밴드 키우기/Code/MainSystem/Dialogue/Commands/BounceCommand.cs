using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;
using Code.MainSystem.Cutscene.DialogCutscene;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 캐릭터 바운스 연출 명령어
    /// </summary>
    [Serializable]
    public class BounceCommand : IDialogueCommand
    {
        public NameTagPositionType position;
        public float intensity = 30f;
        public float duration = 0.5f;
        public int count = 2;

        public void Execute()
        {
            Bus<CharacterEffectEvent>.Raise(new CharacterEffectEvent(position, CharacterEffectType.Bounce, intensity, duration, count));
        }
    }
}
