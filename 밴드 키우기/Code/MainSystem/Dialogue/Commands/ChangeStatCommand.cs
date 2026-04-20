using System;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Actions;
using Code.MainSystem.Cutscene.DialogCutscene;
using Code.MainSystem.StatSystem.BaseStats;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 스탯 변경 명령어
    /// </summary>
    [Serializable]
    public class ChangeStatCommand : IDialogueCommand
    {
        public MemberType targetMember;
        public StatType targetStat;
        public int variation;

        public void Execute()
        {
            DialogueSessionState.AddStatDelta(targetMember, targetStat, variation);
            Bus<DialogueStatUpgradeEvent>.Raise(new DialogueStatUpgradeEvent(
                new StatVariation { targetMember = targetMember, targetStat = targetStat, variation = variation }
            ));
        }
    }
}
