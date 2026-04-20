namespace Code.Core.Load
{
    /// <summary>
    /// 게임 시작하는 최초 1회만 실행되는 함수를 만드는 인터페이스
    /// </summary>
    public interface IAwakeLoadComponent
    {
        /// <summary>
        /// 게임 시작 최초 1회만 발동합니다.(씬이 로딩되거나 껐다 켜도 실행되지 않음)
        /// </summary>
        public void FirstTimeAwake();
    }
}