using System.Collections.Generic;
using Code.MainSystem.Dialogue;

namespace Code.Core.Bus.GameEvents.DialogueEvents.UI
{
    /// <summary>
    /// 다이얼로그 선택지 표시 요청 이벤트
    /// </summary>
    public struct DialogueShowChoiceEvent : IEvent
    {
        /// <summary>
        /// 표시할 선택지 리스트
        /// </summary>
        public readonly List<DialogueChoiceViewData> Choices;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="choices">선택지 리스트</param>
        public DialogueShowChoiceEvent(List<DialogueChoiceViewData> choices)
        {
            Choices = choices;
        }
    }
}
