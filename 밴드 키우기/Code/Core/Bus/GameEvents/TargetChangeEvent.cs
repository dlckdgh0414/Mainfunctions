namespace Code.Core.Bus.GameEvents
{
    public struct TargetChangeEvent : IEvent
    {
        public string ChangeTarget; //만약 이제 목표 조건 개수가 바뀔때 사용하는 이벤트 만약 목표 도달했다면 ""보내주셈

        public TargetChangeEvent(string changeTarget)
        {
            this.ChangeTarget = changeTarget;
        }
    }
}