using System.Collections.Generic;
using Code.Core;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 잠금 판정 정책 구현.
    /// </summary>
    public class PartTimeCardEligibilityPolicy
    {
        /// <summary>
        /// 전원 조건 기반 카드 잠금 상태 평가 수행.
        /// </summary>
        /// <param name="cardDefinition">평가 대상 카드 정의.</param>
        /// <param name="memberConditions">참여 멤버 컨디션 스냅샷 목록.</param>
        /// <returns>잠금 상태가 반영된 카드 옵션 데이터.</returns>
        public PartTimeCardOption Evaluate(
            PartTimeCardDefinition cardDefinition,
            IReadOnlyList<PartTimeMemberConditionSnapshot> memberConditions)
        {
            PartTimeCardOption option = new PartTimeCardOption
            {
                CardDefinition = cardDefinition,
                IsLocked = false,
                LockReasonCode = PartTimeCardLockReasonCode.None,
                LockReasonMessage = string.Empty,
            };

            if (memberConditions == null || memberConditions.Count == 0)
            {
                return option;
            }

            int memberCount = memberConditions.Count;
            for (int i = 0; i < memberCount; i++)
            {
                PartTimeMemberConditionSnapshot snapshot = memberConditions[i];

                if (cardDefinition.HasMinCondition && snapshot.ConditionMode < cardDefinition.MinCondition)
                {
                    option.IsLocked = true;
                    option.LockReasonCode = PartTimeCardLockReasonCode.MinConditionNotMet;
                    option.LockReasonMessage = BuildMinConditionReason(snapshot.MemberType, cardDefinition.MinCondition);
                    return option;
                }

                if (cardDefinition.HasMaxCondition && snapshot.ConditionMode > cardDefinition.MaxCondition)
                {
                    option.IsLocked = true;
                    option.LockReasonCode = PartTimeCardLockReasonCode.MaxConditionExceeded;
                    option.LockReasonMessage = BuildMaxConditionReason(snapshot.MemberType, cardDefinition.MaxCondition);
                    return option;
                }
            }

            return option;
        }

        /// <summary>
        /// 최소 컨디션 미달 잠금 사유 문자열 생성.
        /// </summary>
        /// <param name="memberType">조건 미달 멤버 타입.</param>
        /// <param name="requiredMinCondition">요구 최소 컨디션.</param>
        /// <returns>잠금 사유 문자열.</returns>
        private static string BuildMinConditionReason(MemberType memberType, MemberConditionMode requiredMinCondition)
        {
            return string.Format(PartTimeTextConstants.LOCK_MIN_REASON_FORMAT, memberType, requiredMinCondition);
        }

        /// <summary>
        /// 최대 컨디션 초과 잠금 사유 문자열 생성.
        /// </summary>
        /// <param name="memberType">조건 초과 멤버 타입.</param>
        /// <param name="requiredMaxCondition">허용 최대 컨디션.</param>
        /// <returns>잠금 사유 문자열.</returns>
        private static string BuildMaxConditionReason(MemberType memberType, MemberConditionMode requiredMaxCondition)
        {
            return string.Format(PartTimeTextConstants.LOCK_MAX_REASON_FORMAT, memberType, requiredMaxCondition);
        }
    }
}
