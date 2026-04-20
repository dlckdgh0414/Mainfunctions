using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.Presenter;
using Code.MainSystem.Tree.Addon;
using Code.MainSystem.Tree.Upgrade;
using Code.SubSystem.BandFunds;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// PartTime 실행 흐름 전담 클래스
    /// </summary>
    public class PartTimeExecutor : IPartTimeExecutor, IAddon
    {
        private readonly PartTimeRewardPolicy _rewardPolicy;
        private readonly PartTimeCardCatalog _cardCatalog;
        private readonly PartTimeCardEligibilityPolicy _cardEligibilityPolicy;
        private readonly PartTimeRewardCalculator _rewardCalculator;
        private readonly IPartTimeCardChoiceView _cardChoiceView;
        private readonly PartTimeConditionOutcomePolicy _conditionOutcomePolicy;
        private readonly PartTimeProgressOverlayView _progressOverlayView;
        private readonly IPartTimeResultView _resultView;
        private readonly MemberThrowDataSO _memberThrowDataSO;
        private readonly SchedulePresenter _schedulePresenter;

        private const float PROGRESS_MIN_DURATION = 1.0f;
        private const float PROGRESS_MAX_DURATION = 2.0f;

        public PartTimeExecutor(
            PartTimeRewardPolicy rewardPolicy,
            PartTimeCardCatalog cardCatalog,
            PartTimeCardEligibilityPolicy cardEligibilityPolicy,
            PartTimeRewardCalculator rewardCalculator,
            IPartTimeCardChoiceView cardChoiceView,
            PartTimeConditionOutcomePolicy conditionOutcomePolicy,
            PartTimeProgressOverlayView progressOverlayView,
            IPartTimeResultView resultView,
            MemberThrowDataSO memberThrowDataSO,
            SchedulePresenter schedulePresenter)
        {
            _rewardPolicy = rewardPolicy;
            _cardCatalog = cardCatalog;
            _cardEligibilityPolicy = cardEligibilityPolicy;
            _rewardCalculator = rewardCalculator;
            _cardChoiceView = cardChoiceView;
            _conditionOutcomePolicy = conditionOutcomePolicy;
            _progressOverlayView = progressOverlayView;
            _resultView = resultView;
            _memberThrowDataSO = memberThrowDataSO;
            _schedulePresenter = schedulePresenter;
            EventHandle();
        }

        /// <summary>
        /// PartTime 카드 선택 포함 보상 연출 및 지급 수행
        /// </summary>
        public async UniTask ExecuteAsync()
        {
            IReadOnlyList<MemberDataSO> members = _memberThrowDataSO.CurrentMembers;
            int baseReward = _rewardPolicy.CalculateReward(members);

            List<PartTimeMemberConditionSnapshot> memberConditions = BuildMemberConditions(members);
            List<PartTimeCardOption> cardOptions = BuildCardOptions(memberConditions);

            PartTimeCardId selectedCardId = GetDefaultCardId(cardOptions);
            if (_cardChoiceView != null && cardOptions.Count > 0)
            {
                selectedCardId = await _cardChoiceView.ShowAndSelectAsync(members, cardOptions, baseReward);
            }

            bool isProgressOverlayVisible = false;
            if (_progressOverlayView != null)
            {
                _progressOverlayView.Show(PartTimeTextConstants.PARTTIME_PROGRESS_TEXT);
                isProgressOverlayVisible = true;
                float randomProgressDuration = UnityEngine.Random.Range(PROGRESS_MIN_DURATION, PROGRESS_MAX_DURATION);
                await _progressOverlayView.PlayProgressAsync(randomProgressDuration);
            }

            try
            {
                ApplyAllUpgrades();
                PartTimeCardSelectionResult selectionResult = BuildSelectionResult(selectedCardId, cardOptions, baseReward);
                PartTimeCardDefinition selectedCardDefinition = GetSelectedCardDefinition(selectedCardId, cardOptions);
                int resolvedConditionDelta = ResolveConditionDelta(selectedCardDefinition, selectionResult.ConditionDelta, members);
                selectionResult.ConditionDelta = resolvedConditionDelta;
                selectionResult.FinalReward += (int)(selectionResult.FinalReward * PartTimeGetGoldPlusValue / 100f);
                int beforeGold = BandSupplyManager.Instance != null ? BandSupplyManager.Instance.BandFunds : 0;
                ApplyConditionToAllMembers(members, selectionResult.ConditionDelta);

                UniTask resultPopupTask = UniTask.CompletedTask;
                if (_resultView != null)
                {
                    resultPopupTask = _resultView.ShowAsync(members, selectionResult, beforeGold);
                }

                if (_progressOverlayView != null)
                {
                    _progressOverlayView.Hide();
                    isProgressOverlayVisible = false;
                }

                await resultPopupTask;

                ApplyRewardAndContinueQueue(
                    selectionResult.FinalReward,
                    selectionResult.ConditionDelta);
                ResetUpgradeValue();
            }
            finally
            {
                if (isProgressOverlayVisible && _progressOverlayView != null)
                {
                    _progressOverlayView.Hide();
                }
            }
        }

        /// <summary>
        /// 선택 카드 정의 데이터 조회
        /// </summary>
        /// <param name="selectedCardId">선택 카드 식별자</param>
        /// <param name="cardOptions">카드 옵션 목록</param>
        /// <returns>선택 카드 정의 데이터</returns>
        private static PartTimeCardDefinition GetSelectedCardDefinition(
            PartTimeCardId selectedCardId,
            IReadOnlyList<PartTimeCardOption> cardOptions)
        {
            int optionCount = cardOptions != null ? cardOptions.Count : 0;
            for (int i = 0; i < optionCount; i++)
            {
                PartTimeCardOption option = cardOptions[i];
                if (option.CardDefinition.CardId == selectedCardId && !option.IsLocked)
                {
                    return option.CardDefinition;
                }
            }

            PartTimeCardOption fallbackOption = GetFallbackOption(cardOptions);
            return fallbackOption.CardDefinition;
        }

        /// <summary>
        /// 참여 멤버 컨디션 스냅샷 목록 생성
        /// </summary>
        /// <param name="members">참여 멤버 목록</param>
        /// <returns>컨디션 스냅샷 목록</returns>
        private List<PartTimeMemberConditionSnapshot> BuildMemberConditions(IReadOnlyList<MemberDataSO> members)
        {
            List<PartTimeMemberConditionSnapshot> snapshots = new List<PartTimeMemberConditionSnapshot>();
            if (members == null)
            {
                return snapshots;
            }

            int memberCount = members.Count;
            for (int i = 0; i < memberCount; i++)
            {
                MemberDataSO member = members[i];
                if (member == null)
                {
                    continue;
                }

                PartTimeMemberConditionSnapshot snapshot = new PartTimeMemberConditionSnapshot
                {
                    MemberType = member.memberType,
                    ConditionMode = MemberConditionManager.Instance != null
                        ? MemberConditionManager.Instance.GetCondition(member.memberType)
                        : Code.Core.MemberConditionMode.Commonly,
                };

                snapshots.Add(snapshot);
            }

            return snapshots;
        }

        /// <summary>
        /// 카드 잠금 상태 목록 생성
        /// </summary>
        /// <param name="memberConditions">참여 멤버 컨디션 스냅샷 목록</param>
        /// <returns>카드 옵션 목록</returns>
        private List<PartTimeCardOption> BuildCardOptions(IReadOnlyList<PartTimeMemberConditionSnapshot> memberConditions)
        {
            List<PartTimeCardOption> options = new List<PartTimeCardOption>();

            if (_cardCatalog == null)
            {
                return options;
            }

            IReadOnlyList<PartTimeCardDefinition> definitions = _cardCatalog.GetCardDefinitions();
            if (definitions == null)
            {
                return options;
            }

            int definitionCount = definitions.Count;
            for (int i = 0; i < definitionCount; i++)
            {
                PartTimeCardDefinition definition = definitions[i];
                PartTimeCardOption option = _cardEligibilityPolicy != null
                    ? _cardEligibilityPolicy.Evaluate(definition, memberConditions)
                    : new PartTimeCardOption
                    {
                        CardDefinition = definition,
                        IsLocked = false,
                        LockReasonCode = PartTimeCardLockReasonCode.None,
                        LockReasonMessage = string.Empty,
                    };
                options.Add(option);
            }

            return options;
        }

        /// <summary>
        /// 기본 선택 카드 식별자 반환
        /// </summary>
        /// <param name="cardOptions">카드 옵션 목록</param>
        /// <returns>기본 선택 카드 식별자</returns>
        private static PartTimeCardId GetDefaultCardId(IReadOnlyList<PartTimeCardOption> cardOptions)
        {
            if (cardOptions == null || cardOptions.Count == 0)
            {
                return PartTimeCardId.StandardWork;
            }

            int optionCount = cardOptions.Count;
            for (int i = 0; i < optionCount; i++)
            {
                if (!cardOptions[i].IsLocked)
                {
                    return cardOptions[i].CardDefinition.CardId;
                }
            }

            return PartTimeCardId.StandardWork;
        }

        /// <summary>
        /// 카드 선택 결과 생성
        /// </summary>
        /// <param name="selectedCardId">선택 카드 식별자</param>
        /// <param name="cardOptions">카드 옵션 목록</param>
        /// <param name="baseReward">카드 적용 전 기본 보상</param>
        /// <returns>카드 선택 결과 데이터</returns>
        private PartTimeCardSelectionResult BuildSelectionResult(
            PartTimeCardId selectedCardId,
            IReadOnlyList<PartTimeCardOption> cardOptions,
            int baseReward)
        {
            PartTimeCardOption selectedOption = GetFallbackOption(cardOptions);

            int optionCount = cardOptions != null ? cardOptions.Count : 0;
            for (int i = 0; i < optionCount; i++)
            {
                PartTimeCardOption option = cardOptions[i];
                if (option.CardDefinition.CardId == selectedCardId && !option.IsLocked)
                {
                    selectedOption = option;
                    break;
                }
            }

            float multiplier = selectedOption.CardDefinition.RewardMultiplier;
            int finalReward = _rewardCalculator != null
                ? _rewardCalculator.CalculateFinalReward(baseReward, multiplier)
                : baseReward;

            PartTimeCardSelectionResult result = new PartTimeCardSelectionResult
            {
                SelectedCardId = selectedOption.CardDefinition.CardId,
                SelectedCardName = selectedOption.CardDefinition.DisplayName,
                BaseReward = baseReward,
                AppliedMultiplier = multiplier,
                FinalReward = finalReward,
                ConditionDelta = selectedOption.CardDefinition.ConditionDelta,
            };

            return result;
        }

        /// <summary>
        /// 카드 선택 실패 대비 폴백 카드 반환
        /// </summary>
        /// <param name="cardOptions">카드 옵션 목록</param>
        /// <returns>폴백 카드 옵션</returns>
        private static PartTimeCardOption GetFallbackOption(IReadOnlyList<PartTimeCardOption> cardOptions)
        {
            if (cardOptions != null)
            {
                int optionCount = cardOptions.Count;
                for (int i = 0; i < optionCount; i++)
                {
                    if (!cardOptions[i].IsLocked)
                    {
                        return cardOptions[i];
                    }
                }
            }

            PartTimeCardDefinition defaultDefinition = new PartTimeCardDefinition
            {
                CardId = PartTimeCardId.StandardWork,
                DisplayName = PartTimeTextConstants.DEFAULT_CARD_NAME,
                RewardMultiplier = 1.0f,
                ConditionDelta = 0,
                HasMinCondition = false,
                MinCondition = Code.Core.MemberConditionMode.VeryBad,
                HasMaxCondition = false,
                MaxCondition = Code.Core.MemberConditionMode.VeryBad,
                Description = string.Empty,
            };

            return new PartTimeCardOption
            {
                CardDefinition = defaultDefinition,
                IsLocked = false,
                LockReasonCode = PartTimeCardLockReasonCode.None,
                LockReasonMessage = string.Empty,
            };
        }

        /// <summary>
        /// 카드 선택 결과 기반 최종 컨디션 변화량 계산
        /// </summary>
        /// <param name="cardDefinition">선택 카드 정의 데이터</param>
        /// <param name="fallbackDelta">카드 기본 변화량</param>
        /// <param name="members">참여 멤버 목록</param>
        /// <returns>최종 컨디션 변화량</returns>
        private int ResolveConditionDelta(PartTimeCardDefinition cardDefinition, int fallbackDelta, IReadOnlyList<MemberDataSO> members)
        {
            if (_conditionOutcomePolicy == null)
            {
                return fallbackDelta;
            }

            return _conditionOutcomePolicy.ResolveConditionDelta(cardDefinition, fallbackDelta, members);
        }

        /// <summary>
        /// 컨디션 변화량을 참여 멤버 전원에게 동일 적용
        /// </summary>
        /// <param name="members">참여 멤버 목록</param>
        /// <param name="conditionDelta">적용할 컨디션 변화량</param>
        private void ApplyConditionToAllMembers(IReadOnlyList<MemberDataSO> members, int conditionDelta)
        {
            if (MemberConditionManager.Instance == null)
            {
                return;
            }

            if (members == null || conditionDelta == 0)
            {
                return;
            }

            int amount = conditionDelta > 0 ? conditionDelta : -conditionDelta;
            int memberCount = members.Count;
            for (int i = 0; i < memberCount; i++)
            {
                MemberDataSO member = members[i];
                if (member == null)
                {
                    continue;
                }

                if (conditionDelta > 0)
                {
                    MemberConditionManager.Instance.UpCondition(member.memberType, amount);
                }
                else
                {
                    MemberConditionManager.Instance.DownCondition(member.memberType, amount);
                }
            }
        }

        /// <summary>
        /// 보상 적용 후 다음 스케줄 실행
        /// </summary>
        private void ApplyRewardAndContinueQueue(int rewardGold, int conditionDelta)
        {
            if (rewardGold > 0 && BandSupplyManager.Instance != null)
            {
                BandSupplyManager.Instance.AddBandFunds(rewardGold);
            }

            Bus<TutorialPartTimeCompletedEvent>.Raise(new TutorialPartTimeCompletedEvent(rewardGold, conditionDelta));

            if (_memberThrowDataSO != null)
            {
                _memberThrowDataSO.CleanupCompletedActivity();
            }

            if (_schedulePresenter != null)
            {
                _schedulePresenter.ExecuteNextSchedule();
            }
        }
        
        public float PartTimeGetGoldPlusValue { get; set; }
        public List<BaseUpgradeSO> Upgrades { get; } = new List<BaseUpgradeSO>();

        public void ApplyAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            { 
                upgrade.Upgrade(this);
            }
        }

        public void ResetUpgradeValue()
        {
            PartTimeGetGoldPlusValue = 0;
        }

        public void EventHandle()
        {
            Bus<TreeUpgradeEvent>.OnEvent += HandleAddUpgrade;
        }

        private void HandleAddUpgrade(TreeUpgradeEvent evt)
        {
            if(evt.Type != TreeUpgradeType.GoldGetToPartTime) return;
            Debug.Log(evt.UpgradeSO.effectDescription);
            Upgrades.Add(evt.UpgradeSO);
        }
    }

}
