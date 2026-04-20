using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// PartTime 기본 골드 계산 정책 구현
    /// </summary>
    public class PartTimeRewardPolicy
    {
        private const int BASE_REWARD = 200;
        private const int MEMBER_REWARD = 100;

        /// <summary>
        /// 참여 멤버 수 기반 골드 계산 수행
        /// </summary>
        /// <param name="members">PartTime 참여 멤버 목록</param>
        /// <returns>지급 골드</returns>
        public int CalculateReward(IReadOnlyList<MemberDataSO> members)
        {
            if (members == null || members.Count == 0)
            {
                return 0;
            }

            return BASE_REWARD + members.Count * MEMBER_REWARD;
        }
    }
}
