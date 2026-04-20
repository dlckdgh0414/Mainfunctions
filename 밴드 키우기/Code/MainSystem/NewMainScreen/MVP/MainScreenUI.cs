using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.behavior;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.PartTime;
using Code.MainSystem.NewMainScreen.MVP.PartTime.Data;
using Code.MainSystem.NewMainScreen.MVP.Presenter;
using Code.MainSystem.NewMainScreen.MVP.View;
using Code.SubSystem.BandFunds;
using Code.SubSystem.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP
{
    public class MainScreenUI : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MemberInfoView memberInfoView;
        [SerializeField] private ParticipationView participationView;
        [SerializeField] private ScheduleView scheduleView;
        [SerializeField] private AmbassadorView ambassadorView;
        [SerializeField] private ShopView shopView;
        [SerializeField] private PromotionView promotionView;

        [Header("핸드폰")]
        [SerializeField] private CellPhoneSwiping cellPhoneSwiping;

        [Header("멤버 데이터 레퍼런스")]
        [SerializeField] private List<AssetReference> memberDataReferenceList;

        [Header("스탯 아이콘")]
        [SerializeField] private Sprite compositionSprite;
        [SerializeField] private Sprite proficiencySprite;

        [Header("대사 데이터")]
        [SerializeField] private Image leaderImage;
        [SerializeField] private List<MemberAmbassadorDataSO> ambassadorDataList;
        [SerializeField] private MemberType leader = MemberType.Bass;

        [Header("Behavior")]
        [SerializeField] private BehaviorController behaviorController;

        [Header("경험치 설정")]
        [SerializeField] private int addExp = 10;

        [Header("MemberThrow SO")]
        [SerializeField] private MemberThrowDataSO memberThrowDataSO;

        [Header("일정 등록 UI")]
        [SerializeField] private ScheduleRegistrationUI concertRegistrationUI;
        [SerializeField] private ScheduleRegistrationUI songRegistrationUI;
        [SerializeField] private ScheduleRegistrationUI partTimeRegistrationUI;

        [Header("알바 결과 UI")]
        [SerializeField] private PartTimeResultPopupView partTimeResultPopupView;
        [SerializeField] private PartTimeCardChoicePopupView partTimeCardChoicePopupView;
        [SerializeField] private PartTimeProgressOverlayView partTimeProgressOverlayView;

        [Header("알바 카드 데이터")]
        [SerializeField] private PartTimeCardCatalogSO partTimeCardCatalogSO;

        [Header("상점 데이터")]
        [SerializeField] private ShopItemListSO shopItemListData;
        [SerializeField] private MemberSelectPopup memberSelectPopup;

        [Header("홍보 데이터")]
        [SerializeField] private List<PromotionListSO> promotionPages;

        [Header("로딩 UI")]
        [SerializeField] private LoadingUI loadingUI;

        private MemberInfoPresenter _memberInfoPresenter;
        private ParticipationPresenter _participationPresenter;
        private SchedulePresenter _schedulePresenter;
        private AmbassadorPresenter _ambassadorPresenter;
        private ShopPresenter _shopPresenter;
        private PromotionPresenter _promotionPresenter;
        private IPartTimeExecutor _partTimeExecutor;

        private readonly HashSet<MemberType> _partTimeParticipants = new HashSet<MemberType>();

        private TutorialFlowStep _currentTutorialStep = TutorialFlowStep.None;
        private bool _hasTutorialStepSignal;

        private async UniTaskVoid Start()
        {
            loadingUI.Show();
            BuildPresentersBeforeLoad();

            bool hasCompletedActivity = memberThrowDataSO.RunningActivity != null;
            bool hasPending           = memberThrowDataSO.HasPendingSchedule;

            var memberDataList = await _memberInfoPresenter.LoadAsync();
            _memberInfoPresenter.Refresh();

            foreach (var data in memberDataList)
            {
                MemberConditionManager.Instance.InitManager(data.memberType, data.currentmod);
                if (data.memberType == leader)
                    leaderImage.sprite = data.IconSprite;
            }

            _participationPresenter.Initialize(memberDataList);
            SetLeaderAmbassadorData();

            _shopPresenter = new ShopPresenter(
                shopView,
                shopItemListData,
                memberDataList,
                memberSelectPopup
            );

            _schedulePresenter = new SchedulePresenter(
                scheduleView, _participationPresenter, _shopPresenter,
                memberThrowDataSO, TurnManager.Instance, _promotionPresenter
            );
            _schedulePresenter.OnScheduleStarted       += HandleScheduleStarted;
            _schedulePresenter.OnScheduleCancelled     += HandleScheduleCancelled;
            _schedulePresenter.OnAllSchedulesCompleted += HandleAllSchedulesCompleted;

            // [수정] _schedulePresenter.SetMusicUploadReady(false); 를 제거했습니다.
            // 이제 Presenter 생성 시 세이브 데이터를 자동으로 체크하여 100%라면 버튼을 활성화합니다.

            BuildPartTimeExecutor();

            if (hasCompletedActivity)
            {
                memberThrowDataSO.CleanupCompletedActivity();
                if (TurnManager.Instance != null && !TurnManager.Instance.IsMaxReached)
                    TurnManager.Instance.AdvanceOneWeek();
            }
            else if (hasPending)
            {
                memberThrowDataSO.CleanupCompletedActivity();
            }
            else
            {
                memberThrowDataSO.CleanupCompletedActivity();
                memberThrowDataSO.ClearAll();
            }

            loadingUI.Hide();

            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));

            if (hasPending)
                _schedulePresenter.ExecuteNextSchedule();
        }

        private void BuildPresentersBeforeLoad()
        {
            _memberInfoPresenter = new MemberInfoPresenter(
                memberDataReferenceList, memberInfoView, compositionSprite, proficiencySprite
            );
            _memberInfoPresenter.OnLeaderChanged += HandleLeaderChanged;

            _participationPresenter = new ParticipationPresenter(
                participationView, memberThrowDataSO,
                concertRegistrationUI, songRegistrationUI, partTimeRegistrationUI
            );

            _promotionPresenter = new PromotionPresenter(promotionView, promotionPages);
            _ambassadorPresenter = new AmbassadorPresenter(ambassadorView);

            scheduleView.OnActivityClicked += HandleActivitySelected;
            scheduleView.OnCancelFromSelectApp += HandleCancelFromSelectApp;

            if (cellPhoneSwiping != null)
                cellPhoneSwiping.onPhonePutAway.AddListener(OnPhonePutAway);

            if (behaviorController != null)
            {
                behaviorController.OnBehaviorCompleted    += HandleBehaviorCompleted;
                behaviorController.OnMusicReadyToUpload   += HandleMusicReadyToUpload;
            }

            Bus<TutorialStepChangedEvent>.OnEvent += HandleTutorialStepChanged;
        }

        private void BuildPartTimeExecutor()
        {
            PartTimeRewardPolicy rewardPolicy = new PartTimeRewardPolicy();
            PartTimeCardCatalog cardCatalog = new PartTimeCardCatalog(partTimeCardCatalogSO);
            PartTimeCardEligibilityPolicy cardEligibilityPolicy = new PartTimeCardEligibilityPolicy();
            PartTimeRewardCalculator rewardCalculator = new PartTimeRewardCalculator();
            PartTimeConditionOutcomePolicy conditionOutcomePolicy = new PartTimeConditionOutcomePolicy();

            _partTimeExecutor = new PartTimeExecutor(
                rewardPolicy,
                cardCatalog,
                cardEligibilityPolicy,
                rewardCalculator,
                partTimeCardChoicePopupView,
                conditionOutcomePolicy,
                partTimeProgressOverlayView,
                partTimeResultPopupView,
                memberThrowDataSO,
                _schedulePresenter
            );
        }

        private void OnPhonePutAway() => _ambassadorPresenter.OnPhonePutAway();

        private void HandleCancelFromSelectApp()
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.ForceHidePhone();
        }

        private void SetLeaderAmbassadorData()
        {
            foreach (var data in ambassadorDataList)
            {
                if (data.memberType != leader) continue;
                _ambassadorPresenter.SetData(data);
                return;
            }
        }

        private void HandleLeaderChanged(MemberType newLeader)
        {
            leader = newLeader;

            var memberData = _memberInfoPresenter.MemberDataList
                .FirstOrDefault(d => d.memberType == newLeader);

            if (memberData != null && memberData.IconSprite != null)
                leaderImage.sprite = memberData.IconSprite;

            SetLeaderAmbassadorData();
        }

        private void HandleActivitySelected(ManagementBtnType type)
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.SetSwipeEnabled(false);
        }

        private void HandleScheduleCancelled()
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.SetSwipeEnabled(true);
        }

        private void HandleScheduleStarted(ManagementBtnType type)
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.SetSwipeEnabled(false);

            if (type == ManagementBtnType.PartTime)
            {
                var participants = memberThrowDataSO.GetMembers(ManagementBtnType.PartTime);
                if (participants != null)
                {
                    foreach (var member in participants)
                    {
                        _partTimeParticipants.Add(member.memberType);
                    }
                }

                ExecutePartTimeAsync().Forget();
                return;
            }

            if (type == ManagementBtnType.Concert || type == ManagementBtnType.Song)
            {
                if (behaviorController != null)
                    behaviorController.StartBehavior(type);
                return;
            }
        }

        private async UniTask ExecutePartTimeAsync()
        {
            if (_partTimeExecutor == null) return;

            await _partTimeExecutor.ExecuteAsync();

            if (!memberThrowDataSO.HasPendingSchedule && memberThrowDataSO.RunningActivity == null)
            {
                if (cellPhoneSwiping != null)
                    cellPhoneSwiping.SetSwipeEnabled(true);
            }
        }

        private void HandleBehaviorCompleted()
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.SetSwipeEnabled(true);

            BandSupplyManager.Instance.AddBandExp(addExp);
            _schedulePresenter.ExecuteNextSchedule();
        }

        private void HandleMusicReadyToUpload()
        {
            if (_hasTutorialStepSignal && !IsUploadStepOrLater(_currentTutorialStep))
            {
                Bus<TutorialUploadReadyEvent>.Raise(new TutorialUploadReadyEvent());
                return;
            }

            // [수정] 업로드 버튼 활성화와 동시에 다른 활동 비활성화 로직이 Presenter 내부에서 연동됩니다.
            _schedulePresenter?.SetMusicUploadReady(true);
            Bus<TutorialUploadReadyEvent>.Raise(new TutorialUploadReadyEvent());
        }

        private void HandleAllSchedulesCompleted()
        {
            ApplyRestConditionBonus();

            _participationPresenter.ClearAll();
            scheduleView.ReturnToSelectAppForTutorial();

            if (TurnManager.Instance != null && !TurnManager.Instance.IsMaxReached)
            {
                TurnManager.Instance.AdvanceOneWeek();
                Bus<TutorialWeekAdvancedEvent>.Raise(new TutorialWeekAdvancedEvent(
                    TurnManager.Instance.CurrentYear,
                    TurnManager.Instance.CurrentMonth,
                    TurnManager.Instance.CurrentWeek));
            }

            if (GameStatManager.Instance != null &&
                GameStatManager.Instance.GetMusicPerfectionPercent() >= 100)
            {
                HandleMusicReadyToUpload();
            }

            RefreshAllUI();
        }

        private void HandleTutorialStepChanged(TutorialStepChangedEvent evt)
        {
            _hasTutorialStepSignal = true;
            _currentTutorialStep = evt.Step;

            if (_schedulePresenter != null)
            {
                // 튜토리얼 단계에 따라 강제 제어
                _schedulePresenter.SetMusicUploadReady(IsUploadStepOrLater(evt.Step), false);
            }
        }

        private static bool IsUploadStepOrLater(TutorialFlowStep step)
        {
            return step == TutorialFlowStep.UploadReady
                || step == TutorialFlowStep.UploadCompleted
                || step == TutorialFlowStep.Completed;
        }

        private void ApplyRestConditionBonus()
        {
            if (_memberInfoPresenter?.MemberDataList == null) return;

            foreach (var memberData in _memberInfoPresenter.MemberDataList)
            {
                if (!_partTimeParticipants.Contains(memberData.memberType))
                {
                    MemberConditionManager.Instance.UpCondition(memberData.memberType, 1);
                }
            }

            _partTimeParticipants.Clear();
        }

        private void RefreshAllUI()
        {
            _participationPresenter.Refresh();
            _memberInfoPresenter.Refresh();
        }

        private void OnDestroy()
        {
            if (cellPhoneSwiping != null)
                cellPhoneSwiping.onPhonePutAway.RemoveListener(OnPhonePutAway);

            scheduleView.OnActivityClicked -= HandleActivitySelected;
            scheduleView.OnCancelFromSelectApp -= HandleCancelFromSelectApp;

            if (_schedulePresenter != null)
            {
                _schedulePresenter.OnScheduleStarted       -= HandleScheduleStarted;
                _schedulePresenter.OnScheduleCancelled     -= HandleScheduleCancelled;
                _schedulePresenter.OnAllSchedulesCompleted -= HandleAllSchedulesCompleted;
                _schedulePresenter.Dispose();
            }

            if (behaviorController != null)
            {
                behaviorController.OnBehaviorCompleted  -= HandleBehaviorCompleted;
                behaviorController.OnMusicReadyToUpload -= HandleMusicReadyToUpload;
            }

            Bus<TutorialStepChangedEvent>.OnEvent -= HandleTutorialStepChanged;

            _memberInfoPresenter?.Dispose();
            _participationPresenter?.Dispose();
            _ambassadorPresenter?.Dispose();
            _shopPresenter?.Dispose();
            _promotionPresenter?.Dispose();
        }
    }
}
