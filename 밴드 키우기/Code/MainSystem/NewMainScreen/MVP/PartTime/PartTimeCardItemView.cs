using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// 아르바이트 카드 단일 아이템 뷰.
    /// </summary>
    public class PartTimeCardItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button cardButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI conditionText;
        [SerializeField] private GameObject lockRoot;
        [SerializeField] private TextMeshProUGUI lockReasonText;

        private Action<PartTimeCardId> _onSelected;
        private PartTimeCardId _cardId;

        /// <summary>
        /// 카드 옵션 데이터 바인딩 수행.
        /// </summary>
        /// <param name="option">표시 대상 카드 옵션.</param>
        /// <param name="onSelected">선택 콜백.</param>
        public void Bind(PartTimeCardOption option, Action<PartTimeCardId> onSelected)
        {
            _onSelected = onSelected;
            _cardId = option.CardDefinition.CardId;

            PartTimeCardDefinition definition = option.CardDefinition;

            if (titleText != null)
            {
                titleText.SetText(definition.DisplayName);
            }

            if (descriptionText != null)
            {
                descriptionText.SetText(definition.Description);
            }

            if (rewardText != null)
            {
                rewardText.SetText(string.Format(PartTimeTextConstants.REWARD_FORMAT, definition.RewardMultiplier));
            }

            if (conditionText != null)
            {
                conditionText.SetText(GetConditionText(definition.ConditionDelta));
            }

            bool isLocked = option.IsLocked;
            if (cardButton != null)
            {
                cardButton.interactable = !isLocked;
                cardButton.onClick.RemoveListener(HandleClick);
                cardButton.onClick.AddListener(HandleClick);
            }

            if (lockRoot != null)
            {
                lockRoot.SetActive(isLocked);
            }

            if (lockReasonText != null)
            {
                lockReasonText.SetText(option.LockReasonMessage);
            }
        }

        /// <summary>
        /// 카드 이벤트 바인딩 해제 수행.
        /// </summary>
        public void Unbind()
        {
            _onSelected = null;
            if (cardButton != null)
            {
                cardButton.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>
        /// 카드 클릭 이벤트 처리 수행.
        /// </summary>
        private void HandleClick()
        {
            if (_onSelected != null)
            {
                _onSelected.Invoke(_cardId);
            }
        }

        /// <summary>
        /// 컨디션 변화 표시 문자열 반환.
        /// </summary>
        /// <param name="conditionDelta">컨디션 변화량.</param>
        /// <returns>표시 문자열.</returns>
        private static string GetConditionText(int conditionDelta)
        {
            if (conditionDelta > 0)
            {
                return string.Format(PartTimeTextConstants.CONDITION_PLUS_FORMAT, conditionDelta);
            }

            if (conditionDelta < 0)
            {
                return string.Format(PartTimeTextConstants.CONDITION_MINUS_FORMAT, conditionDelta);
            }

            return PartTimeTextConstants.CONDITION_STAY_TEXT;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
