using Code.MainSystem.TraitSystem.Data;

namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 특성 추가를 요청할 때 사용하는 이벤트
    /// </summary>
    public struct TraitAddRequested : IEvent
    {
        /// <summary>
        /// 어떤 멤버에게 특성을 추가할지 구분하기 위한 값
        /// </summary>
        public readonly MemberType MemberType;
        /// <summary>
        /// 추가하려는 특성의 타입
        /// </summary>
        public readonly TraitDataSO TraitData;

        /// <summary>
        /// 특성을 추가 하고 싶은 멤버와 추가 하고 싶은 특성
        /// </summary>
        /// <param name="memberType">특성을 추가 하고 싶은 멤버</param>
        /// <param name="traitData">추가 하고 싶은 특성</param>
        public TraitAddRequested(MemberType memberType, TraitDataSO traitData)
        {
            MemberType = memberType;
            TraitData = traitData;
        }
    }
}