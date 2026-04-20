namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 새로운 특성 추가 시 특성 포인트 한도를 초과하여
    /// 조정(기존 특성 제거)이 필요함을 알리는 이벤트
    /// </summary>
    public struct TraitOverflow : IEvent
    {
        /// <summary>
        /// 새로운 특성을 추가하려 했을 때의 특성 포인트 합계
        /// (현재 보유 포인트 + 신규 특성 포인트)
        /// </summary>
        public int CurrentPoint { get; }
        /// <summary>
        /// 해당 캐릭터가 가질 수 있는 최대 특성 포인트
        /// </summary>
        public int MaxPoint { get; }

        /// <summary>
        /// 특성 추가로 인해 포인트가 초과된 상황을 전달한다
        /// </summary>
        /// <param name="currentPoint">새로운 특성을 추가하려 했을 때의 특성 포인트 합계</param>
        /// <param name="maxPoint">캐릭터의 최대 특성 포인트</param>
        public TraitOverflow(int currentPoint, int maxPoint)
        {
            CurrentPoint = currentPoint;
            MaxPoint = maxPoint;
        }
    }
}