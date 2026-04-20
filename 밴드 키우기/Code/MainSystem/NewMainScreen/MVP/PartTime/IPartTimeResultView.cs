using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Cysharp.Threading.Tasks;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// PartTime 결과 연출 뷰 계약 정의
    /// </summary>
    public interface IPartTimeResultView
    {
        /// <summary>
        /// 카드 선택 결과 정산 연출 후 확인 입력 대기
        /// </summary>
        /// <param name="members">PartTime 참여 멤버 목록</param>
        /// <param name="selectionResult">카드 선택 결과 데이터</param>
        /// <param name="beforeGold">알바 수행 이전 밴드 보유 금액</param>
        UniTask ShowAsync(IReadOnlyList<MemberDataSO> members, PartTimeCardSelectionResult selectionResult, int beforeGold);
    }
}
