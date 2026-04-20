using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 튜토리얼 시작 상태 초기화를 담당.
    /// </summary>
    public class TutorialBootstrap : MonoBehaviour
    {
        [Header("실행 설정")]
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool resetTutorialProgress;

        [Header("데이터 참조")]
        [SerializeField] private MemberThrowDataSO memberThrowDataSO;
        [SerializeField] private TutorialFlowController tutorialFlowController;

        [Header("튜토리얼 시작값")]
        [SerializeField] private int startMoney = 1500;
        [SerializeField] private int startExp;
        [SerializeField] private int startFans;

        private void Awake()
        {
            if (!applyOnAwake)
            {
                return;
            }

            ApplyInitialState();
        }

        /// <summary>
        /// 튜토리얼 초기 상태 적용.
        /// </summary>
        public void ApplyInitialState()
        {
            if (resetTutorialProgress && tutorialFlowController != null)
            {
                tutorialFlowController.ResetProgress();
            }

            if (memberThrowDataSO != null)
            {
                memberThrowDataSO.ClearAll();
                memberThrowDataSO.CleanupCompletedActivity();
                memberThrowDataSO.ClearScheduleQueue();
            }

            if (GameStatManager.Instance != null)
            {
                GameStatManager.Instance.ResetStats();
            }

            if (BandSupplyManager.Instance != null)
            {
                AlignBandSupply(startMoney, startExp, startFans);
            }
        }

        /// <summary>
        /// 밴드 재화 상태를 지정 값으로 정렬.
        /// </summary>
        /// <param name="targetMoney">목표 자금.</param>
        /// <param name="targetExp">목표 경험치.</param>
        /// <param name="targetFans">목표 팬 수.</param>
        private static void AlignBandSupply(int targetMoney, int targetExp, int targetFans)
        {
            BandSupplyManager manager = BandSupplyManager.Instance;

            int moneyDelta = targetMoney - manager.BandFunds;
            if (moneyDelta > 0)
            {
                manager.AddBandFunds(moneyDelta);
            }
            else if (moneyDelta < 0)
            {
                manager.SpendBandFunds(-moneyDelta);
            }

            int expDelta = targetExp - manager.BandExp;
            if (expDelta > 0)
            {
                manager.AddBandExp(expDelta);
            }
            else if (expDelta < 0)
            {
                manager.SpendBandExp(-expDelta);
            }

            int fansDelta = targetFans - manager.BandFans;
            if (fansDelta > 0)
            {
                manager.AddBandFans(fansDelta);
            }
            else if (fansDelta < 0)
            {
                manager.RemoveBandFans(-fansDelta);
            }
        }
    }
}
