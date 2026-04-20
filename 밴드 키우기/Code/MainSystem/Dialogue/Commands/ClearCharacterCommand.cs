using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;
using Code.MainSystem.Cutscene.DialogCutscene;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 특정 위치 캐릭터 퇴장 명령어
    /// </summary>
    [Serializable]
    public class ClearCharacterCommand : IDialogueCommand
    {
        public NameTagPositionType position;
        public void Execute() => Bus<ClearCharacterEvent>.Raise(new ClearCharacterEvent(position));
    }
}
