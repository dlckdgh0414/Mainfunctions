using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Cysharp.Threading.Tasks;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 선택 뷰 계약 정의.
    /// </summary>
    public interface IPartTimeCardChoiceView
    {
        /// <summary>
        /// 카드 선택 UI 표시 후 유저 선택 대기 수행.
        /// </summary>
        /// <param name="members">참여 멤버 목록.</param>
        /// <param name="cardOptions">카드 잠금 상태 목록.</param>
        /// <param name="baseReward">카드 적용 전 기본 보상.</param>
        /// <returns>유저 선택 카드 식별자.</returns>
        UniTask<PartTimeCardId> ShowAndSelectAsync(
            IReadOnlyList<MemberDataSO> members,
            IReadOnlyList<PartTimeCardOption> cardOptions,
            int baseReward);
    }
}
