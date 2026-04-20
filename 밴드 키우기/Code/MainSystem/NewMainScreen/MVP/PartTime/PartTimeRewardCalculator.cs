using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 최종 보상 계산 구현.
    /// </summary>
    public class PartTimeRewardCalculator
    {
        /// <summary>
        /// 기본 보상과 배율 기반 최종 보상 계산 수행.
        /// </summary>
        /// <param name="baseReward">카드 적용 전 기본 보상.</param>
        /// <param name="rewardMultiplier">카드 배율 값.</param>
        /// <returns>최종 보상 금액.</returns>
        public int CalculateFinalReward(int baseReward, float rewardMultiplier)
        {
            float reward = baseReward * rewardMultiplier;
            return Mathf.RoundToInt(reward);
        }
    }
}
