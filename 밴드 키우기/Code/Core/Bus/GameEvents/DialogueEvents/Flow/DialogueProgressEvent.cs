using Code.MainSystem.Dialogue;
using Member.LS.Code.Dialogue;
using UnityEngine;

namespace Code.Core.Bus.GameEvents.DialogueEvents.Flow
{
    /// <summary>
    /// 다이알로그 진행 시 로드 완료된 데이터를 UI에 전달하는 이벤트
    /// </summary>
    public struct DialogueProgressEvent : IEvent
    {
        /// <summary>
        /// 화면에 출력될 대사 내용
        /// </summary>
        public readonly string DialogueDetail;

        /// <summary>
        /// 대화 중인 캐릭터 이름
        /// </summary>
        public readonly string CharacterName;

        /// <summary>
        /// 현재 표시할 캐릭터 스프라이트
        /// </summary>
        public readonly Sprite CharacterSprite;

        /// <summary>
        /// 현재 표시할 배경 스프라이트
        /// </summary>
        public readonly Sprite BackgroundImage;

        /// <summary>
        /// 이름표 표시 위치 타입
        /// </summary>
        public readonly NameTagPositionType NameTagPosition;

        /// <summary>
        /// 오토 모드 진행을 위한 대기 시간 (초)
        /// </summary>
        public readonly float AutoWaitTime;

        /// <summary>
        /// 현재 노드에 선택지가 포함되어 있는지 여부
        /// </summary>
        public readonly bool HasChoices;

        /// <summary>
        /// 현재 대사가 독백인지 여부
        /// </summary>
        public readonly bool IsMonologue;

        /// <summary>
        /// 생성자
        /// </summary>
        public DialogueProgressEvent(string dialogueDetail, string characterName, Sprite characterSprite, Sprite backgroundImage, NameTagPositionType nameTagPosition, float autoWaitTime, bool hasChoices, bool isMonologue)
        {
            DialogueDetail = dialogueDetail;
            CharacterName = characterName;
            CharacterSprite = characterSprite;
            BackgroundImage = backgroundImage;
            NameTagPosition = nameTagPosition;
            AutoWaitTime = autoWaitTime;
            HasChoices = hasChoices;
            IsMonologue = isMonologue;
        }
    }
}
