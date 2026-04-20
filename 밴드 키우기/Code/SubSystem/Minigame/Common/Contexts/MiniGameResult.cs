namespace Code.SubSystem.Minigame.Common.Contexts
{
    /// <summary>
    /// 미니게임 종료 시 메인으로 돌려줄 결과
    /// </summary>
    public struct MiniGameResult
    {
        /// <summary>
        /// 게임 성공 여부
        /// </summary>
        public bool IsSuccess;

        /// <summary>
        /// 얻은 점수
        /// </summary>
        public StatResult StatResult;

        /// <summary>
        /// 게임에 소비한 시간 (초 단위)
        /// </summary>
        public float PlayTime;
        // 필요시 데이터 추가
    }

    public struct StatResult
    {
        // 상승 전 값 
        public int BeforeA;
        public int BeforeB;
        
        // 상승량
        public float StatA;
        public float StatB;
    }
}