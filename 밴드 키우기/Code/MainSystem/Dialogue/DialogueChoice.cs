using System;
using System.Collections.Generic;
using Code.Core.Attributes;
using UnityEngine;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 다이알로그에서 플레이어의 선택을 위한 정보 구조체
    /// </summary>
    [Serializable]
    public struct DialogueChoice
    {
        /// <summary>
        /// 선택지에 표시될 텍스트
        /// </summary>
        public string ChoiceText;

        /// <summary>
        /// 선택 가능 상태에서 선택지 하단에 표시할 보조 설명 텍스트
        /// </summary>
        public string SubText;

        /// <summary>
        /// 선택 불가(잠금) 상태에서 선택지 하단에 표시할 보조 설명 텍스트
        /// </summary>
        public string LockedSubText;

        /// <summary>
        /// 선택 시 이동할 다이알로그 노드의 고유 식별자
        /// </summary>
        public string NextNodeID;

        /// <summary>
        /// 선택 시 실행될 추가 연출 명령어 목록
        /// </summary>
        [SerializeReference, SubclassSelector] public List<IDialogueCommand> Commands;

        /// <summary>
        /// 선택지 노출 조건 목록
        /// </summary>
        [SerializeReference, SubclassSelector] public List<IDialogueCondition> Conditions;
    }
}
