using Code.Core.Bus;

namespace Code.Core.Bus.GameEvents
{
    /// <summary>
    /// BandSupplyManager.AddBandFunds 호출 시 발행.
    /// EarnMoneyConditionSO가 구독합니다.
    /// </summary>
    public struct MoneyChangedEvent : IEvent
    {
        public int TotalEarned; // 현재 누적 밴드 자금
        
        public MoneyChangedEvent(int totalEarned)
        {
            this.TotalEarned = totalEarned;
        }
    }
}