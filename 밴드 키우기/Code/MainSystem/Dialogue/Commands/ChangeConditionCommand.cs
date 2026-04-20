using System;
using Code.Core;
using Code.MainSystem.NewMainScreen;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 멤버 컨디션 변경 명령어
    /// </summary>
    [Serializable]
    public class ChangeConditionCommand : IDialogueCommand
    {
        public MemberType targetMember;
        public int variation;

        public void Execute()
        {
            if (MemberConditionManager.Instance == null)
            {
                return;
            }

            int appliedDelta = -variation;
            DialogueSessionState.AddConditionDelta(targetMember, appliedDelta);

            // TODO: Move to unified stat/condition event flow in dialogue domain.
            if (variation < 0)
            {
                MemberConditionManager.Instance.DownCondition(targetMember, -variation);
                return;
            }

            if (variation > 0)
            {
                MemberConditionManager.Instance.UpCondition(targetMember, variation);
            }
        }
    }
}
