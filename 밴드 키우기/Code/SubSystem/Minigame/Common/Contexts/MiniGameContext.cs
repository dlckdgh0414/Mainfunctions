namespace Code.SubSystem.Minigame.Common.Contexts
{
    /// <summary>
    /// 미니게임 시작 시 전달할 초기화 데이터
    /// (필요에 따라 확장 가능)
    /// </summary>
    public struct MiniGameContext
    {
        /// <summary>
        /// 게임 난이도
        /// </summary>
        public int Difficulty;

        /// <summary>
        /// 특수 효과 활성화 여부
        /// </summary>
        public bool EnableSpecialEffects;

        /// <summary>
        /// 기타 매개변수 저장용
        /// </summary>
        public object[] CustomData;

        // 필요시 데이터 추가
    }
}