using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 효과음 재생 명령어
    /// </summary>
    [Serializable]
    public class PlaySFXCommand : IDialogueCommand
    {
        public string sfxId;
        public void Execute() => Bus<PlaySFXEvent>.Raise(new PlaySFXEvent(sfxId));
    }
}
