namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 식별자 정의.
    /// </summary>
    public enum PartTimeCardId
    {
        SafeWork,
        StandardWork,
        NightShift,
    }

    /// <summary>
    /// 아르바이트 연출 및 로직 공통 문자열 상수 정의.
    /// </summary>
    public static class PartTimeTextConstants
    {
        public const string PARTTIME_COMPLETE_TITLE = "알바 완료";
        public const string PARTTIME_PROGRESS_TEXT = "알바 진행중...";

        public const string BASE_REWARD_FORMAT = "기본 보상 {0:N0}원";
        public const string BASE_REWARD_SHORT_FORMAT = "{0:N0}원";
        public const string REWARD_FORMAT = "보상 배율 \n x{0:0.0}";

        public const string CONDITION_PLUS_FORMAT = "컨디션 +{0}";
        public const string CONDITION_MINUS_FORMAT = "컨디션 {0}";
        public const string CONDITION_STAY_TEXT = "없음";

        public const string LOCK_MIN_REASON_FORMAT = "{0}의 컨디션이 {1} 이상이어야 선택 가능";
        public const string LOCK_MAX_REASON_FORMAT = "{0}의 컨디션이 {1} 이하여야 선택 가능";

        public const string DEFAULT_CARD_NAME = "표준 근무";
    }
}
