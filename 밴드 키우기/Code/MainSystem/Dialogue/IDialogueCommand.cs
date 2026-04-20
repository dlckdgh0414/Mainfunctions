namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 다이알로그 연출 명령어를 실행하기 위한 인터페이스
    /// </summary>
    public interface IDialogueCommand
    {
        /// <summary>
        /// 명령어를 실행
        /// </summary>
        void Execute();
    }
}
