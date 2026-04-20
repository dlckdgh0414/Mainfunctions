namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 다이알로그 선택지 노출 여부를 결정하는 조건 인터페이스
    /// </summary>
    public interface IDialogueCondition
    {
        /// <summary>
        /// 조건을 평가하여 만족 여부를 반환
        /// </summary>
        bool Evaluate();
    }
}
