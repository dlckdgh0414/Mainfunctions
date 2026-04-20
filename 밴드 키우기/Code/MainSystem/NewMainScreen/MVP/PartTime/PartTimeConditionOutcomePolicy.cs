using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.PartTime.Data;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 카드별 확률 테이블 기반 컨디션 변화량 산출 구현.
    /// </summary>
    public class PartTimeConditionOutcomePolicy
    {
        /// <summary>
        /// 카드 선택 결과 기반 최종 컨디션 변화량 계산 수행.
        /// </summary>
        /// <param name="cardDefinition">선택 카드 정의 데이터.</param>
        /// <param name="fallbackDelta">카드 기본 변화량.</param>
        /// <param name="members">참여 멤버 목록.</param>
        /// <returns>최종 컨디션 변화량.</returns>
        public int ResolveConditionDelta(
            PartTimeCardDefinition cardDefinition,
            int fallbackDelta,
            IReadOnlyList<MemberDataSO> members)
        {
            if (members == null || members.Count == 0)
            {
                return 0;
            }

            if (!cardDefinition.UseRandomCondition)
            {
                return fallbackDelta;
            }

            IReadOnlyList<PartTimeConditionRandomOutcomeEntry> weightedOutcomes = cardDefinition.WeightedOutcomes;
            if (weightedOutcomes == null || weightedOutcomes.Count == 0)
            {
                return fallbackDelta;
            }

            int totalWeight = 0;
            int entryCount = weightedOutcomes.Count;
            for (int i = 0; i < entryCount; i++)
            {
                PartTimeConditionRandomOutcomeEntry entry = weightedOutcomes[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.Weight > 0)
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0)
            {
                return fallbackDelta;
            }

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;
            for (int i = 0; i < entryCount; i++)
            {
                PartTimeConditionRandomOutcomeEntry entry = weightedOutcomes[i];
                if (entry == null || entry.Weight <= 0)
                {
                    continue;
                }

                cumulativeWeight += entry.Weight;
                if (randomValue < cumulativeWeight)
                {
                    return entry.Delta;
                }
            }

            return fallbackDelta;
        }
    }
}
