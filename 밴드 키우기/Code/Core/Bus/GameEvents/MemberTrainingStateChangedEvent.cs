
namespace Code.Core.Bus.GameEvents
{
    public struct MemberTrainingStateChangedEvent : IEvent
    {
        public readonly MemberType MemberType;

        public MemberTrainingStateChangedEvent(MemberType memberType)
        {
            MemberType = memberType;
        }
    }
}