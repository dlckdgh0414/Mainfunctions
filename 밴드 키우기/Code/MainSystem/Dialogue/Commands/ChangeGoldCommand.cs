using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Actions;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 플레이어의 재화(Gold)를 변경하는 명령어
    /// </summary>
    [Serializable]
    public class ChangeGoldCommand : IDialogueCommand
    {
        public int variation;

        public void Execute()
        {
            DialogueSessionState.AddGoldDelta(variation);

            // 재화 변경 이벤트 호출
            // 시스템에서 해당 이벤트를 수신해 실제 재화 변경 수행
            Bus<DialogueGoldChangeEvent>.Raise(new DialogueGoldChangeEvent(variation));
        }
    }
}
