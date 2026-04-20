using System;
using System.Collections.Generic;
using Code.Core.Attributes;
using Member.LS.Code.Dialogue;
using Member.LS.Code.Dialogue.Character;
using UnityEngine;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 다이알로그의 한 지점을 나타내는 정보 구조체
    /// </summary>
    [Serializable]
    public struct DialogueNode
    {
        /// <summary>
        /// 다이알로그 노드의 고유 식별자
        /// </summary>
        public string NodeID;

        /// <summary>
        /// 대화 중인 캐릭터의 식별자
        /// </summary>
        public string CharacterID;

        /// <summary>
        /// 화면에 출력될 대사 내용
        /// </summary>
        [TextArea] public string DialogueDetail;

        /// <summary>
        /// 캐릭터 이름표가 표시될 위치 타입
        /// </summary>
        public NameTagPositionType NameTagPosition;

        /// <summary>
        /// 출력될 배경 리소스의 식별자
        /// </summary>
        public string BackgroundID;

        /// <summary>
        /// 캐릭터의 현재 감정 상태 타입
        /// </summary>
        public CharacterEmotionType CharacterEmotion;

        /// <summary>
        /// 재생될 보이스 리소스의 식별자
        /// </summary>
        public string VoiceID;

        /// <summary>
        /// 에디터 그래프 뷰에서의 노드 위치
        /// </summary>
        public Vector2 NodePosition;

        /// <summary>
        /// 해당 노드 시작 시 실행될 연출 명령어 목록
        /// </summary>
        [SerializeReference, SubclassSelector] public List<IDialogueCommand> Commands;

        /// <summary>
        /// 다음으로 진행할 다이알로그 노드의 고유 식별자 (비어있으면 대화 종료)
        /// </summary>
        public string NextNodeID;

        /// <summary>
        /// 유저가 선택할 수 있는 선택지 목록
        /// </summary>
        public List<DialogueChoice> Choices;
    }
}
