using Cysharp.Threading.Tasks;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// PartTime 실행 오케스트레이션 계약 정의
    /// </summary>
    public interface IPartTimeExecutor
    {
        /// <summary>
        /// PartTime 실행 처리 수행
        /// </summary>
        UniTask ExecuteAsync();
    }
}
