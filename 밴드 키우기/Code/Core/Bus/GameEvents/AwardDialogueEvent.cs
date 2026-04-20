using Code.SubSystem.Award;

namespace Code.Core.Bus.GameEvents
{
    public struct AwardDialogueEvent : IEvent
    {
        public AwardType AwardType;

        public AwardDialogueEvent(AwardType awardType)
        {
            this.AwardType = awardType;
        }
    }
}