namespace Code.Core.Bus.GameEvents.DialogueEvents.Effects
{
    /// <summary>
    /// 캐릭터가 흥분하여 좌우로 왕복하며 폴짝폴짝 뛰는 연출 이벤트
    /// </summary>
    public class ExcitedEvent
    {
        public readonly string CharacterID;
        public readonly float Duration;
        public readonly float JumpPower;
        public readonly int JumpCount;
        public readonly float MoveDistance;

        public ExcitedEvent(string characterID, float duration, float jumpPower, int jumpCount, float moveDistance)
        {
            CharacterID = characterID;
            Duration = duration;
            JumpPower = jumpPower;
            JumpCount = jumpCount;
            MoveDistance = moveDistance;
        }
    }
}
