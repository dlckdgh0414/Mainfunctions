using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 현재 말하고 있는 캐릭터가 좌우로 왕복하며 폴짝폴짝 뛰는 연출 명령어
    /// </summary>
    [Serializable]
        public class ExcitedCommand : IDialogueCommand
        {
            // DialogueCommandFactory가 필드 순서대로 값을 주입하므로 중요함
            public float intensity = 30f;  // 점프 높이
            public float distance = 20f;   // 좌우 왕복 거리
            public float duration = 1.0f;  // 전체 지속 시간
            public int count = 4;          // 점프 횟수

        public void Execute()
        {
            // DialogueManager에서 현재 발화 중인 캐릭터 위치(Left/Right) 자동 조회
            NameTagPositionType currentPos = DialogueManager.Instance.CurrentPosition;

            Bus<CharacterEffectEvent>.Raise(new CharacterEffectEvent(
                currentPos, 
                CharacterEffectType.Excited, 
                intensity, 
                duration, 
                count, 
                distance));
        }
    }
}
