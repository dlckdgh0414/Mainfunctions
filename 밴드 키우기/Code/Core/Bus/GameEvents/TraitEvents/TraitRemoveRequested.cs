using Code.MainSystem.TraitSystem.Data;

namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 특성 제거를 요청할 때 사용하는 이벤트
    /// </summary>
    public struct TraitRemoveRequested : IEvent
    {
        /// <summary>
        /// 어떤 멤버에게 특성을 제거할지 구분하기 위한 값
        /// </summary>
        public readonly MemberType MemberType;
        /// <summary>
        /// 제거하려는 특성의 타입
        /// </summary>
        public readonly TraitDataSO TraitData;

        /// <summary>
        /// 특성을 제거 하고 싶은 멤버와 제거 하고 싶은 특성
        /// </summary>
        /// <param name="memberType">특성을 제거 하고 싶은 멤버</param>
        /// <param name="traitData">제거 하고 싶은 특성</param>
        public TraitRemoveRequested(MemberType memberType, TraitDataSO traitData)
        {
            MemberType = memberType;
            TraitData = traitData;
        }
    }
}