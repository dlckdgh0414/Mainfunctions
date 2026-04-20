using System;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.MusicProduction;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.NewMainScreen.Data;
using Code.SubSystem.BandFunds;
using Code.SubSystem.Save;
using Code.Tool.Fade;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.MainSystem.Tutorial
{
    /// <summary>
    /// 튜토리얼 단계 순차 진행을 이벤트로 관리하는 컨트롤러.
    /// </summary>
    public class TutorialFlowController : MonoBehaviour
    {
        [Serializable]
        private class TutorialStepOrderEntry
        {
            [SerializeField] private TutorialFlowStep step;

            public TutorialFlowStep Step => step;
        }

        [Header("실행 설정")]
        [SerializeField] private bool autoStartTutorial = false;
        [SerializeField] private bool resetProgressOnStart;
        [SerializeField] private bool useSequentialGate = true;
        [SerializeField] private TutorialFlowStep initialStep = TutorialFlowStep.None;

        [Header("데이터 참조")]
        [SerializeField] private MemberThrowDataSO memberThrowDataSO;

        [Header("튜토리얼 보상 값")]
        [SerializeField] private int fundsGrantAmount = 5000;

        [Header("튜토리얼 종료 전환")]
        [SerializeField] private string nextSceneName = "Main";
        [SerializeField] private float sceneFadeDuration = 0.5f;
        [SerializeField] private FadeImageType sceneFadeType = FadeImageType.Random;

        [Header("튜토리얼 종료 초기화 값")]
        [SerializeField] private int resetBandFunds = 1000;
        [SerializeField] private int resetBandExp;
        [SerializeField] private int resetBandFans;
        [SerializeField] private MemberConditionMode resetConditionMode = MemberConditionMode.Commonly;

        [Header("단계 순서")]
        [SerializeField] private List<TutorialStepOrderEntry> orderedSteps = new List<TutorialStepOrderEntry>();

        [Header("배치 완료 조건")]
        [SerializeField] private int requiredActivityMemberCount = 5;
        [SerializeField] private int requiredPartTimeMemberCount = 5;

        [Header("튜토리얼 업로드 보정")]
        [SerializeField] private int tutorialMusicStatTarget = 50;
        [SerializeField] private int tutorialMemberStatBonus = 120;

        private const string TUTORIAL_PROGRESS_KEY = "tutorial_flow_step";
        private const string TUTORIAL_COMPLETED_KEY = "tutorial_flow_completed";

        private TutorialFlowStep _currentStep = TutorialFlowStep.None;
        private bool _isRunning;
        private Dictionary<TutorialFlowStep, int> _stepIndexByStep = new Dictionary<TutorialFlowStep, int>();
        private bool _isTransitioning;
        private bool _isUploadReadyReceived;
        private bool _isStatGuideAcknowledged;
        private bool _isPartTimeCompletedAcknowledged;

        public TutorialFlowStep CurrentStep => _currentStep;
        public bool IsRunning => _isRunning;

        private void Awake()
        {
            BuildStepIndex();
            if (resetProgressOnStart) ResetProgress();
        }

        private void OnEnable()
        {
            SubscribeSignals();
            Bus<TutorialStartEvent>.OnEvent += OnStartTutorial;
        }

        // private void Start()
        // {
        //     if (autoStartTutorial) StartTutorial();
        // }

        private void OnDisable()
        {
            UnsubscribeSignals();
            Bus<TutorialStartEvent>.OnEvent -= OnStartTutorial;
        }

        private void OnStartTutorial(TutorialStartEvent evt)
        {
            if (autoStartTutorial) StartTutorial();
        }

        public void StartTutorial()
        {
            if (PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1)
            {
                _isRunning = false;
                return;
            }

            _isRunning = true;
            _isUploadReadyReceived = false;
            _isStatGuideAcknowledged = false;
            _isPartTimeCompletedAcknowledged = false;

            TutorialFlowStep savedStep = LoadProgress();
            if (savedStep == TutorialFlowStep.None) savedStep = initialStep;

            SetStep(savedStep);
        }

        public void CompleteTutorial()
        {
            if (!_isRunning) return;

            _isRunning = false;
            SetStep(TutorialFlowStep.Completed);
            PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(TUTORIAL_PROGRESS_KEY);
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();

            _currentStep = TutorialFlowStep.None;
            _isRunning = false;
        }

        private void BuildStepIndex()
        {
            _stepIndexByStep.Clear();

            if (orderedSteps == null || orderedSteps.Count == 0)
            {
                BuildDefaultStepIndex();
                return;
            }

            int count = orderedSteps.Count;
            for (int i = 0; i < count; i++)
            {
                TutorialStepOrderEntry entry = orderedSteps[i];
                if (entry == null) continue;
                _stepIndexByStep[entry.Step] = i;
            }
        }

        private void BuildDefaultStepIndex()
        {
            int index = 0;
            _stepIndexByStep[TutorialFlowStep.None] = index++;
            _stepIndexByStep[TutorialFlowStep.Welcome] = index++;
            _stepIndexByStep[TutorialFlowStep.UIOverview_Funds] = index++;
            _stepIndexByStep[TutorialFlowStep.UIOverview_Fans] = index++;
            _stepIndexByStep[TutorialFlowStep.UIOverview_Calendar] = index++;
            _stepIndexByStep[TutorialFlowStep.UIOverview_Members] = index++;
            _stepIndexByStep[TutorialFlowStep.PhoneGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.BandManageGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.MusicConfigured] = index++;
            _stepIndexByStep[TutorialFlowStep.FundsGranted] = index++;
            _stepIndexByStep[TutorialFlowStep.ShopGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.ShopHomeGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.PromotionGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.TreeUnlock] = index++;
            _stepIndexByStep[TutorialFlowStep.ReturnToSelectApp] = index++;
            _stepIndexByStep[TutorialFlowStep.ScheduleManageGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.ActivityGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.ActivitySelected] = index++;
            _stepIndexByStep[TutorialFlowStep.MemberAssigned] = index++;
            _stepIndexByStep[TutorialFlowStep.ScheduleRegistered] = index++;
            _stepIndexByStep[TutorialFlowStep.StartAllPressed] = index++;
            _stepIndexByStep[TutorialFlowStep.ActivityCompleted] = index++;
            _stepIndexByStep[TutorialFlowStep.StatGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.PartTimeGuide] = index++;
            _stepIndexByStep[TutorialFlowStep.PartTimeRegistrationPending] = index++;
            _stepIndexByStep[TutorialFlowStep.StartAllPressed_Part2] = index++;
            _stepIndexByStep[TutorialFlowStep.PartTimeCompleted] = index++;
            _stepIndexByStep[TutorialFlowStep.UploadReady] = index++;
            _stepIndexByStep[TutorialFlowStep.UploadCompleted] = index++;
            _stepIndexByStep[TutorialFlowStep.Completed] = index;
        }

        private void SubscribeSignals()
        {
            Bus<TutorialPhoneOpenedEvent>.OnEvent += HandlePhoneOpened;
            Bus<TutorialManageTabOpenedEvent>.OnEvent += HandleManageTabOpened;
            Bus<TutorialMusicConfiguredEvent>.OnEvent += HandleMusicConfigured;
            Bus<TutorialShopPurchasedEvent>.OnEvent += HandleShopPurchased;
            Bus<TutorialPromotionClosedEvent>.OnEvent += HandlePromotionClosed;
            Bus<TutorialTreeClosedEvent>.OnEvent += HandleTreeClosed;
            Bus<TutorialActivitySelectedEvent>.OnEvent += HandleActivitySelected;
            Bus<TutorialMemberAssignedEvent>.OnEvent += HandleMemberAssigned;
            Bus<TutorialReturnButtonClickedEvent>.OnEvent += HandleReturnButtonClicked;
            Bus<TutorialScheduleRegisteredEvent>.OnEvent += HandleScheduleRegistered;
            Bus<TutorialStartAllPressedEvent>.OnEvent += HandleStartAllPressed;
            Bus<TutorialActivityCompletedEvent>.OnEvent += HandleActivityCompleted;
            Bus<TutorialPartTimeCompletedEvent>.OnEvent += HandlePartTimeCompleted;
            Bus<TutorialWeekAdvancedEvent>.OnEvent += HandleWeekAdvanced;
            Bus<TutorialUploadReadyEvent>.OnEvent += HandleUploadReady;
            Bus<TutorialUploadCompletedEvent>.OnEvent += HandleUploadCompleted;
            Bus<TutorialPopupClosedEvent>.OnEvent += HandlePopupClosed;
        }

        private void UnsubscribeSignals()
        {
            Bus<TutorialPhoneOpenedEvent>.OnEvent -= HandlePhoneOpened;
            Bus<TutorialManageTabOpenedEvent>.OnEvent -= HandleManageTabOpened;
            Bus<TutorialMusicConfiguredEvent>.OnEvent -= HandleMusicConfigured;
            Bus<TutorialShopPurchasedEvent>.OnEvent -= HandleShopPurchased;
            Bus<TutorialPromotionClosedEvent>.OnEvent -= HandlePromotionClosed;
            Bus<TutorialTreeClosedEvent>.OnEvent -= HandleTreeClosed;
            Bus<TutorialActivitySelectedEvent>.OnEvent -= HandleActivitySelected;
            Bus<TutorialMemberAssignedEvent>.OnEvent -= HandleMemberAssigned;
            Bus<TutorialReturnButtonClickedEvent>.OnEvent -= HandleReturnButtonClicked;
            Bus<TutorialScheduleRegisteredEvent>.OnEvent -= HandleScheduleRegistered;
            Bus<TutorialStartAllPressedEvent>.OnEvent -= HandleStartAllPressed;
            Bus<TutorialActivityCompletedEvent>.OnEvent -= HandleActivityCompleted;
            Bus<TutorialPartTimeCompletedEvent>.OnEvent -= HandlePartTimeCompleted;
            Bus<TutorialWeekAdvancedEvent>.OnEvent -= HandleWeekAdvanced;
            Bus<TutorialUploadReadyEvent>.OnEvent -= HandleUploadReady;
            Bus<TutorialUploadCompletedEvent>.OnEvent -= HandleUploadCompleted;
            Bus<TutorialPopupClosedEvent>.OnEvent -= HandlePopupClosed;
        }

        /// <summary>
        /// 팝업 닫기 신호 처리.
        /// </summary>
        private void HandlePopupClosed(TutorialPopupClosedEvent evt)
        {
            switch (_currentStep)
            {
                case TutorialFlowStep.None:
                    TryAdvance(TutorialFlowStep.Welcome);
                    break;
                case TutorialFlowStep.Welcome:
                    TryAdvance(TutorialFlowStep.UIOverview_Funds);
                    break;
                case TutorialFlowStep.UIOverview_Funds:
                    TryAdvance(TutorialFlowStep.UIOverview_Fans);
                    break;
                case TutorialFlowStep.UIOverview_Fans:
                    TryAdvance(TutorialFlowStep.UIOverview_Calendar);
                    break;
                case TutorialFlowStep.UIOverview_Calendar:
                    TryAdvance(TutorialFlowStep.UIOverview_Members);
                    break;
                case TutorialFlowStep.UIOverview_Members:
                    TryAdvance(TutorialFlowStep.PhoneGuide);
                    break;
                case TutorialFlowStep.FundsGranted:
                    TryAdvance(TutorialFlowStep.ShopGuide);
                    break;
                case TutorialFlowStep.ActivityCompleted:
                    TryAdvance(TutorialFlowStep.StatGuide);
                    break;
                case TutorialFlowStep.StatGuide:
                    _isStatGuideAcknowledged = true;
                    TryAdvance(TutorialFlowStep.PartTimeGuide);
                    break;
                case TutorialFlowStep.PartTimeGuide:
                    TryAdvance(TutorialFlowStep.PartTimeRegistrationPending);
                    break;
                case TutorialFlowStep.PartTimeCompleted:
                    _isPartTimeCompletedAcknowledged = true;
                    PrepareTutorialUploadState();
                    _isUploadReadyReceived = true;
                    TryAdvance(TutorialFlowStep.UploadReady);
                    break;
                case TutorialFlowStep.UploadCompleted:
                    FinishTutorialAndMoveToMainAsync().Forget();
                    break;
            }
        }

        private void HandlePhoneOpened(TutorialPhoneOpenedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.PhoneGuide)
            {
                TryAdvance(TutorialFlowStep.BandManageGuide);
                return;
            }

            TryAdvance(TutorialFlowStep.PhoneGuide);
        }

        private void HandleManageTabOpened(TutorialManageTabOpenedEvent evt)
        {
            if (evt.TabType == TutorialManageTabType.BandManage)
            {
                if (_currentStep == TutorialFlowStep.BandManageGuide)
                {
                    TryAdvance(TutorialFlowStep.MusicConfigured);
                    return;
                }

                TryAdvance(TutorialFlowStep.BandManageGuide);
            }
            else if (evt.TabType == TutorialManageTabType.ScheduleManage)
            {
                TryAdvance(TutorialFlowStep.ScheduleManageGuide);
                // 스케줄 진입하면 바로 활동 설명 단계로
                if (_currentStep == TutorialFlowStep.ScheduleManageGuide)
                {
                    TryAdvance(TutorialFlowStep.ActivityGuide);
                }
            }
        }

        private void HandleMusicConfigured(TutorialMusicConfiguredEvent evt)
        {
            TryAdvance(TutorialFlowStep.MusicConfigured);
            if (_currentStep == TutorialFlowStep.MusicConfigured)
            {
                TryAdvance(TutorialFlowStep.FundsGranted);
            }
        }

        /// <summary>
        /// 상점 구매 완료 신호 처리.
        /// </summary>
        /// <param name="evt">상점 구매 이벤트.</param>
        private void HandleShopPurchased(TutorialShopPurchasedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.ShopGuide)
            {
                TryAdvance(TutorialFlowStep.ShopHomeGuide);
            }
        }

        /// <summary>
        /// 홍보 닫힘 신호 처리.
        /// </summary>
        /// <param name="evt">홍보 닫힘 이벤트.</param>
        private void HandlePromotionClosed(TutorialPromotionClosedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.PromotionGuide)
            {
                TryAdvance(TutorialFlowStep.TreeUnlock);
            }
        }

        /// <summary>
        /// 트리 닫힘 신호 처리.
        /// </summary>
        /// <param name="evt">트리 닫힘 이벤트.</param>
        private void HandleTreeClosed(TutorialTreeClosedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.TreeUnlock)
            {
                TryAdvance(TutorialFlowStep.ReturnToSelectApp);
            }
        }

        private void HandleActivitySelected(TutorialActivitySelectedEvent evt)
        {
            if (_currentStep != TutorialFlowStep.ActivityGuide) return;

            if (evt.ActivityType == ManagementBtnType.Song || evt.ActivityType == ManagementBtnType.Concert)
            {
                TryAdvance(TutorialFlowStep.ActivitySelected);
            }
        }

        /// <summary>
        /// 멤버 배치 신호 처리. 5명 전원 배치 시 다음 단계.
        /// </summary>
        private void HandleMemberAssigned(TutorialMemberAssignedEvent evt)
        {
            if (_currentStep != TutorialFlowStep.ActivitySelected) return;

            int assignedCount = GetAssignedCount(ManagementBtnType.Song)
                              + GetAssignedCount(ManagementBtnType.Concert);

            if (assignedCount >= requiredActivityMemberCount)
            {
                TryAdvance(TutorialFlowStep.MemberAssigned);
            }
        }

        private void HandleReturnButtonClicked(TutorialReturnButtonClickedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.ShopHomeGuide)
            {
                TryAdvance(TutorialFlowStep.PromotionGuide);
                return;
            }

            if (_currentStep == TutorialFlowStep.ReturnToSelectApp)
            {
                TryAdvance(TutorialFlowStep.ScheduleManageGuide);
                return;
            }

            TryAdvance(TutorialFlowStep.ReturnToSelectApp);
        }

        /// <summary>
        /// 일정 등록 신호 처리. 1차(곡/합주)와 2차(알바) 분기.
        /// </summary>
        private void HandleScheduleRegistered(TutorialScheduleRegisteredEvent evt)
        {
            // 1차: 곡/합주 등록 완료
            if (_currentStep == TutorialFlowStep.MemberAssigned)
            {
                if (evt.ActivityType == ManagementBtnType.Song || evt.ActivityType == ManagementBtnType.Concert)
                {
                    TryAdvance(TutorialFlowStep.ScheduleRegistered);
                }
                return;
            }

            // 2차: 알바 등록 완료
            if (_currentStep == TutorialFlowStep.PartTimeGuide
                || _currentStep == TutorialFlowStep.PartTimeRegistrationPending)
            {
                if (evt.ActivityType == ManagementBtnType.PartTime)
                {
                    int partTimeCount = GetAssignedCount(ManagementBtnType.PartTime);
                    if (partTimeCount >= requiredPartTimeMemberCount)
                    {
                        // PartTimeGuide → PartTimeRegistrationPending 은 생략 가능
                        if (_currentStep == TutorialFlowStep.PartTimeGuide)
                        {
                            TryAdvance(TutorialFlowStep.PartTimeRegistrationPending);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 전체 일정 시작 신호 처리. 1차/2차 분기.
        /// </summary>
        private void HandleStartAllPressed(TutorialStartAllPressedEvent evt)
        {
            if (_currentStep == TutorialFlowStep.ScheduleRegistered)
            {
                TryAdvance(TutorialFlowStep.StartAllPressed);
                return;
            }

            if (_currentStep == TutorialFlowStep.PartTimeRegistrationPending)
            {
                TryAdvance(TutorialFlowStep.StartAllPressed_Part2);
            }
        }

        /// <summary>
        /// 활동 완료 신호 처리.
        /// </summary>
        private void HandleActivityCompleted(TutorialActivityCompletedEvent evt)
        {
            if (_currentStep != TutorialFlowStep.StartAllPressed) return;

            if (evt.ActivityType == ManagementBtnType.Song || evt.ActivityType == ManagementBtnType.Concert)
            {
                TryAdvance(TutorialFlowStep.ActivityCompleted);
            }
        }

        private void HandlePartTimeCompleted(TutorialPartTimeCompletedEvent evt)
        {
            if (_currentStep != TutorialFlowStep.StartAllPressed_Part2) return;

            TryAdvance(TutorialFlowStep.PartTimeCompleted);
        }

        private void HandleWeekAdvanced(TutorialWeekAdvancedEvent evt)
        {
            // 현재 시나리오에서 미사용
        }

        private void HandleUploadReady(TutorialUploadReadyEvent evt)
        {
            _isUploadReadyReceived = true;

            if (_currentStep == TutorialFlowStep.PartTimeCompleted && !_isPartTimeCompletedAcknowledged)
            {
                return;
            }

            if (_currentStep == TutorialFlowStep.StatGuide && !_isStatGuideAcknowledged)
            {
                return;
            }

            TryAdvance(TutorialFlowStep.UploadReady);
        }

        /// <summary>
        /// 업로드 직전 튜토리얼 보정 상태 반영.
        /// </summary>
        private void PrepareTutorialUploadState()
        {
            GameStatManager gameStatManager = GameStatManager.Instance;
            if (gameStatManager != null)
            {
                BoostMusicStatToTarget(gameStatManager, MusicRelatedStatsType.Lyrics, tutorialMusicStatTarget);
                BoostMusicStatToTarget(gameStatManager, MusicRelatedStatsType.Teamwork, tutorialMusicStatTarget);
                BoostMusicStatToTarget(gameStatManager, MusicRelatedStatsType.Proficiency, tutorialMusicStatTarget);
                BoostMusicStatToTarget(gameStatManager, MusicRelatedStatsType.Melody, tutorialMusicStatTarget);

                MemberType[] memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));
                int memberCount = memberTypes.Length;
                for (int i = 0; i < memberCount; i++)
                {
                    MemberType memberType = memberTypes[i];
                    gameStatManager.AddMemberStatDirect(memberType, MusicRelatedStatsType.Composition, tutorialMemberStatBonus);
                    gameStatManager.AddMemberStatDirect(memberType, MusicRelatedStatsType.InstrumentProficiency, tutorialMemberStatBonus);
                }

                if (gameStatManager.GetMusicPerfectionPercent() < 100)
                {
                    gameStatManager.SetMusicPercent(100);
                }
            }
        }

        /// <summary>
        /// 목표치까지 음악 세부 스탯 보정.
        /// </summary>
        private static void BoostMusicStatToTarget(GameStatManager gameStatManager, MusicRelatedStatsType statType, int targetValue)
        {
            int currentValue = gameStatManager.GetScore(statType);
            int delta = targetValue - currentValue;
            if (delta > 0)
            {
                gameStatManager.AddScore(delta, statType);
            }
        }

        private void HandleUploadCompleted(TutorialUploadCompletedEvent evt)
        {
            TryAdvance(TutorialFlowStep.UploadCompleted);
        }

        private int GetAssignedCount(ManagementBtnType type)
        {
            if (memberThrowDataSO == null) return 0;
            return memberThrowDataSO.GetAssignedMemberCount(type);
        }

        private void TryAdvance(TutorialFlowStep nextStep)
        {
            if (!_isRunning) return;
            if (nextStep == TutorialFlowStep.None) return;

            if (!_stepIndexByStep.TryGetValue(nextStep, out int nextIndex)) return;

            if (!_stepIndexByStep.TryGetValue(_currentStep, out int currentIndex))
            {
                currentIndex = -1;
            }

            if (useSequentialGate)
            {
                if (nextIndex != currentIndex + 1) return;
            }
            else
            {
                if (nextIndex <= currentIndex) return;
            }

            SetStep(nextStep);
            SaveProgress(nextStep);
        }

        private void SetStep(TutorialFlowStep step)
        {
            _currentStep = step;
            if (step == TutorialFlowStep.StatGuide)
            {
                _isStatGuideAcknowledged = false;
            }

            if (step == TutorialFlowStep.PartTimeCompleted)
            {
                _isPartTimeCompletedAcknowledged = false;
            }
            Bus<TutorialStepChangedEvent>.Raise(new TutorialStepChangedEvent(step));
            ExecuteStepSideEffect(step);
        }

        private void ExecuteStepSideEffect(TutorialFlowStep step)
        {
            switch (step)
            {
                case TutorialFlowStep.FundsGranted:
                    if (BandSupplyManager.Instance != null && fundsGrantAmount > 0)
                    {
                        BandSupplyManager.Instance.AddBandFunds(fundsGrantAmount);
                    }
                    break;
            }
        }

        private static TutorialFlowStep LoadProgress()
        {
            int savedValue = PlayerPrefs.GetInt(TUTORIAL_PROGRESS_KEY, (int)TutorialFlowStep.None);
            return (TutorialFlowStep)savedValue;
        }

        private static void SaveProgress(TutorialFlowStep step)
        {
            PlayerPrefs.SetInt(TUTORIAL_PROGRESS_KEY, (int)step);
            PlayerPrefs.Save();
        }

        private async UniTaskVoid FinishTutorialAndMoveToMainAsync()
        {
            if (_isTransitioning) return;

            _isTransitioning = true;
            await UniTask.NextFrame();

            CompleteTutorial();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CompleteTutorial(
                    resetBandFunds,
                    resetBandExp,
                    resetBandFans,
                    resetConditionMode);
            }
            Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(nextSceneName, sceneFadeDuration, sceneFadeType));
        }
    }
}
