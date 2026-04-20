using System.Collections.Generic;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 선택지 UI 렌더링에 사용하는 표시 전용 데이터
    /// </summary>
    public struct DialogueChoiceViewData
    {
        /// <summary>
        /// 현재 노드 기준 선택지 인덱스
        /// </summary>
        public int ChoiceIndex;

        /// <summary>
        /// 선택지 메인 텍스트
        /// </summary>
        public string ChoiceText;

        /// <summary>
        /// 선택지 보조 설명 텍스트
        /// </summary>
        public string SubText;

        /// <summary>
        /// 선택지 잠금 여부
        /// </summary>
        public bool IsLocked;

        /// <summary>
        /// 선택지 선택 시 이동할 노드 ID
        /// </summary>
        public string NextNodeID;

        /// <summary>
        /// 선택지 선택 시 실행할 커맨드 목록
        /// </summary>
        public List<IDialogueCommand> Commands;
    }
}
