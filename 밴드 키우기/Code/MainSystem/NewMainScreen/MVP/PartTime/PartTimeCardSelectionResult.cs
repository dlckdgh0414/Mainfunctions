namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 카드 선택 결과 데이터.
    /// </summary>
    public struct PartTimeCardSelectionResult
    {
        public PartTimeCardId SelectedCardId;
        public string SelectedCardName;
        public int BaseReward;
        public float AppliedMultiplier;
        public int FinalReward;
        public int ConditionDelta;
    }
}
