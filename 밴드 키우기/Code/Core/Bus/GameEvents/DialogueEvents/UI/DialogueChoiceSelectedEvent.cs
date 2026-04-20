namespace Code.Core.Bus.GameEvents.DialogueEvents.UI
{
    /// <summary>
    /// 플레이어가 선택지를 선택했을 때 발생하는 이벤트
    /// </summary>
    public struct DialogueChoiceSelectedEvent : IEvent
    {
        /// <summary>
        /// 현재 노드 기준 선택지 인덱스
        /// </summary>
        public readonly int ChoiceIndex;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="choiceIndex">선택지 인덱스</param>
        public DialogueChoiceSelectedEvent(int choiceIndex)
        {
            ChoiceIndex = choiceIndex;
        }
    }
}
