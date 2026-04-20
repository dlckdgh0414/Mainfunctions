using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.NewMainScreen.MVP.PartTime.Data;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 정의 목록 제공 구현.
    /// </summary>
    public class PartTimeCardCatalog
    {
        private const float DEFAULT_REWARD_MULTIPLIER = 1.0f;

        private readonly PartTimeCardCatalogSO _cardCatalogSO;

        public PartTimeCardCatalog(PartTimeCardCatalogSO cardCatalogSO)
        {
            _cardCatalogSO = cardCatalogSO;
        }

        /// <summary>
        /// 카드 정의 목록 조회 수행.
        /// </summary>
        /// <returns>카드 정의 목록.</returns>
        public IReadOnlyList<PartTimeCardDefinition> GetCardDefinitions()
        {
            List<PartTimeCardDefinition> cardDefinitions = new List<PartTimeCardDefinition>();

            if (_cardCatalogSO == null || _cardCatalogSO.Cards == null)
            {
                return BuildFallbackDefinitions();
            }

            HashSet<PartTimeCardId> seenCardIds = new HashSet<PartTimeCardId>();
            int cardCount = _cardCatalogSO.Cards.Count;
            for (int i = 0; i < cardCount; i++)
            {
                PartTimeCardConfigSO cardConfigSO = _cardCatalogSO.Cards[i];
                if (cardConfigSO == null)
                {
                    continue;
                }

                if (seenCardIds.Contains(cardConfigSO.CardId))
                {
                    Debug.LogWarning($"[PartTimeCardCatalog] Duplicate CardId ignored: {cardConfigSO.CardId}");
                    continue;
                }

                seenCardIds.Add(cardConfigSO.CardId);
                cardDefinitions.Add(ConvertToDefinition(cardConfigSO));
            }

            if (cardDefinitions.Count == 0)
            {
                return BuildFallbackDefinitions();
            }

            return cardDefinitions;
        }

        /// <summary>
        /// 카드 설정 SO를 런타임 정의 데이터로 변환.
        /// </summary>
        /// <param name="cardConfigSO">변환 대상 카드 설정 SO.</param>
        /// <returns>런타임 카드 정의 데이터.</returns>
        private static PartTimeCardDefinition ConvertToDefinition(PartTimeCardConfigSO cardConfigSO)
        {
            return new PartTimeCardDefinition
            {
                CardId = cardConfigSO.CardId,
                DisplayName = cardConfigSO.DisplayName,
                RewardMultiplier = cardConfigSO.RewardMultiplier,
                ConditionDelta = cardConfigSO.ConditionDelta,
                HasMinCondition = cardConfigSO.HasMinCondition,
                MinCondition = cardConfigSO.MinCondition,
                HasMaxCondition = cardConfigSO.HasMaxCondition,
                MaxCondition = cardConfigSO.MaxCondition,
                UseRandomCondition = cardConfigSO.UseRandomCondition,
                WeightedOutcomes = cardConfigSO.WeightedOutcomes,
                Description = cardConfigSO.Description,
            };
        }

        /// <summary>
        /// 카탈로그 미설정 대비 폴백 카드 정의 목록 생성.
        /// </summary>
        /// <returns>표준 근무 단일 카드 정의 목록.</returns>
        private static IReadOnlyList<PartTimeCardDefinition> BuildFallbackDefinitions()
        {
            List<PartTimeCardDefinition> fallbackDefinitions = new List<PartTimeCardDefinition>(1)
            {
                new PartTimeCardDefinition
                {
                    CardId = PartTimeCardId.StandardWork,
                    DisplayName = PartTimeTextConstants.DEFAULT_CARD_NAME,
                    RewardMultiplier = DEFAULT_REWARD_MULTIPLIER,
                    ConditionDelta = 0,
                    HasMinCondition = false,
                    MinCondition = MemberConditionMode.VeryBad,
                    HasMaxCondition = false,
                    MaxCondition = MemberConditionMode.VeryBad,
                    UseRandomCondition = false,
                    WeightedOutcomes = null,
                    Description = string.Empty,
                },
            };

            return fallbackDefinitions;
        }
    }
}
