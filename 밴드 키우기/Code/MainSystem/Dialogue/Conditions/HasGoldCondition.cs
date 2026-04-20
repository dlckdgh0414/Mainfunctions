using System;
using Code.SubSystem.BandFunds;

namespace Code.MainSystem.Dialogue.Conditions
{
    [Serializable]
    public class HasGoldCondition : IDialogueCondition
    {
        public int requiredGold;

        public bool Evaluate()
        {
            // TODO: Replace direct singleton access with an injected abstraction when
            // the dialogue condition system is refactored for testability.
            if (BandSupplyManager.Instance == null)
            {
                return false;
            }

            int currentGold = BandSupplyManager.Instance.BandFunds + DialogueSessionState.GoldDelta;
            return currentGold >= requiredGold;
        }
    }
}
