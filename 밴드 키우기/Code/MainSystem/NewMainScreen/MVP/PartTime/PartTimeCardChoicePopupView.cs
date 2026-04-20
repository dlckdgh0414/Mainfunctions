using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 선택 팝업 뷰 구현.
    /// </summary>
    public class PartTimeCardChoicePopupView : MonoBehaviour, IPartTimeCardChoiceView
    {
        [Header("Panel")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI baseRewardText;

        [Header("Card Dynamic List")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private PartTimeCardItemView cardItemPrefab;

        private UniTaskCompletionSource<PartTimeCardId> _selectionCompletionSource;
        private readonly List<PartTimeCardItemView> _spawnedCardItems = new List<PartTimeCardItemView>();

        private void Awake()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        /// <summary>
        /// 카드 선택 UI 표시 후 유저 선택 대기 수행.
        /// </summary>
        /// <param name="members">참여 멤버 목록.</param>
        /// <param name="cardOptions">카드 잠금 상태 목록.</param>
        /// <param name="baseReward">카드 적용 전 기본 보상.</param>
        /// <returns>유저 선택 카드 식별자.</returns>
        public async UniTask<PartTimeCardId> ShowAndSelectAsync(
            IReadOnlyList<MemberDataSO> members,
            IReadOnlyList<PartTimeCardOption> cardOptions,
            int baseReward)
        {
            PartTimeCardId fallbackCardId = PartTimeCardId.StandardWork;

            if (popupRoot == null || cardOptions == null || cardOptions.Count == 0)
            {
                return fallbackCardId;
            }

            popupRoot.SetActive(true);
            BindBaseReward(baseReward);
            BuildCardItems(cardOptions);

            _selectionCompletionSource = new UniTaskCompletionSource<PartTimeCardId>();
            PartTimeCardId selectedCardId = await _selectionCompletionSource.Task;

            CleanupCardItems();
            _selectionCompletionSource = null;
            popupRoot.SetActive(false);
            return selectedCardId;
        }

        /// <summary>
        /// 기본 보상 텍스트 바인딩 수행.
        /// </summary>
        /// <param name="baseReward">기본 보상 금액.</param>
        private void BindBaseReward(int baseReward)
        {
            if (baseRewardText == null)
            {
                return;
            }

            baseRewardText.SetText(string.Format(PartTimeTextConstants.BASE_REWARD_FORMAT, baseReward));
        }

        /// <summary>
        /// 카드 프리팹 목록 생성 및 바인딩 수행.
        /// </summary>
        /// <param name="cardOptions">카드 옵션 목록.</param>
        private void BuildCardItems(IReadOnlyList<PartTimeCardOption> cardOptions)
        {
            CleanupCardItems();

            if (cardOptions == null || cardContainer == null || cardItemPrefab == null)
            {
                return;
            }

            int optionCount = cardOptions.Count;
            for (int i = 0; i < optionCount; i++)
            {
                PartTimeCardOption option = cardOptions[i];
                PartTimeCardItemView cardItemView = Instantiate(cardItemPrefab, cardContainer);
                if (cardItemView == null)
                {
                    continue;
                }

                cardItemView.Bind(option, HandleCardSelected);
                _spawnedCardItems.Add(cardItemView);
            }
        }

        /// <summary>
        /// 카드 선택 이벤트 처리 수행.
        /// </summary>
        /// <param name="cardId">선택 카드 식별자.</param>
        private void HandleCardSelected(PartTimeCardId cardId)
        {
            if (_selectionCompletionSource != null)
            {
                _selectionCompletionSource.TrySetResult(cardId);
            }
        }

        /// <summary>
        /// 생성 카드 아이템 목록 정리 수행.
        /// </summary>
        private void CleanupCardItems()
        {
            int itemCount = _spawnedCardItems.Count;
            for (int i = 0; i < itemCount; i++)
            {
                PartTimeCardItemView cardItemView = _spawnedCardItems[i];
                if (cardItemView == null)
                {
                    continue;
                }

                cardItemView.Unbind();
                Destroy(cardItemView.gameObject);
            }

            _spawnedCardItems.Clear();
        }

        private void OnDestroy()
        {
            CleanupCardItems();
        }
    }
}
