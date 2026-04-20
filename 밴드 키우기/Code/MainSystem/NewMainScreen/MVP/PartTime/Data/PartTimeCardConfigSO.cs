using System.Collections.Generic;
using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime.Data
{
    /// <summary>
    /// 카드별 컨디션 랜덤 결과 단일 항목 데이터.
    /// </summary>
    [System.Serializable]
    public class PartTimeConditionRandomOutcomeEntry
    {
        [SerializeField] private int delta;
        [SerializeField] private int weight = 1;

        public int Delta => delta;
        public int Weight => weight;
    }

    /// <summary>
    /// 아르바이트 카드 단일 설정 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "PartTimeCardConfig", menuName = "SO/MainScreen/PartTime/CardConfig")]
    public class PartTimeCardConfigSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private PartTimeCardId cardId;

        [Header("Display")]
        [SerializeField] private string displayName;
        [SerializeField] [TextArea(2, 4)] private string description;

        [Header("Balance")]
        [SerializeField] private float rewardMultiplier = 1f;
        [SerializeField] private int conditionDelta;
        [SerializeField] private bool hasMinCondition;
        [SerializeField] private MemberConditionMode minCondition = MemberConditionMode.VeryBad;
        [SerializeField] private bool hasMaxCondition;
        [SerializeField] private MemberConditionMode maxCondition = MemberConditionMode.VeryBad;

        [Header("Condition Random")]
        [SerializeField] private bool useRandomCondition = true;
        [SerializeField] private List<PartTimeConditionRandomOutcomeEntry> weightedOutcomes =
            new List<PartTimeConditionRandomOutcomeEntry>();

        [Header("Balance Log")]
        [SerializeField] private string version = "v1.0.0";
        [SerializeField] [TextArea(2, 6)] private string memo;

        public PartTimeCardId CardId => cardId;
        public string DisplayName => displayName;
        public string Description => description;
        public float RewardMultiplier => rewardMultiplier;
        public int ConditionDelta => conditionDelta;
        public bool HasMinCondition => hasMinCondition;
        public MemberConditionMode MinCondition => minCondition;
        public bool HasMaxCondition => hasMaxCondition;
        public MemberConditionMode MaxCondition => maxCondition;
        public bool UseRandomCondition => useRandomCondition;
        public IReadOnlyList<PartTimeConditionRandomOutcomeEntry> WeightedOutcomes => weightedOutcomes;
        public string Version => version;
        public string Memo => memo;
    }

}
