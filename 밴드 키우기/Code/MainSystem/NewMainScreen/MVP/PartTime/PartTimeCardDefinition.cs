using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.NewMainScreen.MVP.PartTime.Data;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 정의 데이터.
    /// </summary>
    public struct PartTimeCardDefinition
    {
        public PartTimeCardId CardId;
        public string DisplayName;
        public float RewardMultiplier;
        public int ConditionDelta;
        public bool HasMinCondition;
        public MemberConditionMode MinCondition;
        public bool HasMaxCondition;
        public MemberConditionMode MaxCondition;
        public bool UseRandomCondition;
        public IReadOnlyList<PartTimeConditionRandomOutcomeEntry> WeightedOutcomes;
        public string Description;
    }
}
