namespace Code.Core.Bus.GameEvents.DialogueEvents.UI
{
    /// <summary>
    /// 오토 모드 활성화 상태를 토글하는 이벤트
    /// </summary>
    public struct ToggleAutoModeEvent : IEvent
    {
        public readonly bool IsAuto;

        public ToggleAutoModeEvent(bool isAuto)
        {
            IsAuto = isAuto;
        }
    }
}
