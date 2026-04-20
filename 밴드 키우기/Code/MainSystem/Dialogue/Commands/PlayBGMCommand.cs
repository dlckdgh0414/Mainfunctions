using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// BGM 재생 명령어
    /// </summary>
    [Serializable]
    public class PlayBGMCommand : IDialogueCommand
    {
        public string bgmId;
        public void Execute() => Bus<PlayBGMEvent>.Raise(new PlayBGMEvent(bgmId));
    }
}
