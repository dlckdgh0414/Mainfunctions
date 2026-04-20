namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 카드 선택 후보 데이터.
    /// </summary>
    public struct PartTimeCardOption
    {
        public PartTimeCardDefinition CardDefinition;
        public bool IsLocked;
        public PartTimeCardLockReasonCode LockReasonCode;
        public string LockReasonMessage;
    }
}
