
namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 멤버별 특성 보유 현황 확인 때 사용하는 이벤트
    /// </summary>
    public struct TraitShowRequested : IEvent
    {
        /// <summary>
        /// 조회 대상 멤버
        /// </summary>
        public MemberType MemberType { get; }
        
        /// <summary>
        /// 특성 보유 현황 조회 요청 이벤트를 생성한다
        /// </summary>
        /// <param name="memberType">특성 보유 현황을 조회하고자 하는 멤버 타입 </param>
        public TraitShowRequested(MemberType memberType)
        {
            MemberType = memberType;
        }
    }
}