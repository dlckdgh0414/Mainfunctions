using Code.MainSystem.TraitSystem.Interface;

namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 특성 보유 현황 조회 요청에 대한 응답으로 사용되는 이벤트 
    /// </summary>
    public struct TraitShowResponded : IEvent
    {
        public ITraitHolder Holder { get; }

        public TraitShowResponded(ITraitHolder holder)
        {
            Holder = holder;
        }
    }
}