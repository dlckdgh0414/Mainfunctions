using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime
{
    /// <summary>
    /// PartTime 결과 연출 팝업 뷰
    /// </summary>
    public class PartTimeResultPopupView : MonoBehaviour, IPartTimeResultView
    {
        private const float STEP_APPEAR_DURATION = 0.18f;
        private const float STEP_INTERVAL_DURATION = 0.1f;
        private const float AFTER_GOLD_BASE_COUNT_DURATION = 0.7f;
        private const float AFTER_GOLD_MAX_COUNT_DURATION = 1.4f;
        private const float AFTER_GOLD_PUNCH_DURATION = 0.2f;
        private const float CONFIRM_PULSE_DURATION = 0.6f;
        private const float CONFIRM_PULSE_SCALE = 1.05f;

        [Header("Panel")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private RectTransform popupPanel;
        [SerializeField] private Button confirmButton;

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image moneyIconImage;
        [SerializeField] private TextMeshProUGUI beforeGoldText;
        [SerializeField] private Image flowSymbolIconImage;
        [SerializeField] private TextMeshProUGUI afterGoldText;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private TextMeshProUGUI baseRewardText;
        [SerializeField] private TextMeshProUGUI conditionDeltaText;
        [SerializeField] private List<GameObject> memberImages;

        private UniTaskCompletionSource _confirmCompletionSource;
        private UnityAction _confirmAction;
        private Tween _confirmPulseTween;

        /// <summary>
        /// 초기 표시 상태 구성
        /// </summary>
        private void Awake()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
        }

        /// <summary>
        /// 결과 연출 출력 후 확인 입력 대기
        /// </summary>
        /// <param name="members">PartTime 참여 멤버 목록</param>
        /// <param name="selectionResult">카드 선택 결과 데이터</param>
        /// <param name="beforeGold">알바 수행 이전 밴드 보유 금액</param>
        public async UniTask ShowAsync(IReadOnlyList<MemberDataSO> members, PartTimeCardSelectionResult selectionResult, int beforeGold)
        {
            if (popupRoot == null)
            {
                return;
            }

            int safeBeforeGold = Mathf.Max(0, beforeGold);
            int safeRewardGold = Mathf.Max(0, selectionResult.FinalReward);
            int finalGold = safeBeforeGold + safeRewardGold;

            popupRoot.SetActive(true);
            SetMembers(members);
            ResetFinancialDisplay();
            BindCardResultInfo(selectionResult);

            if (titleText != null)
            {
                titleText.SetText(PartTimeTextConstants.PARTTIME_COMPLETE_TITLE);
            }

            if (popupPanel != null)
            {
                popupPanel.localScale = Vector3.one;
            }

            await PlayFinancialFlowAsync(safeBeforeGold, safeRewardGold, finalGold);
            EnableConfirmButtonPulse();
            await WaitForConfirmAsync();
            await ClosePopupAsync();
        }

        /// <summary>
        /// 카드 결과 정보 텍스트 바인딩
        /// </summary>
        /// <param name="selectionResult">카드 선택 결과 데이터</param>
        private void BindCardResultInfo(PartTimeCardSelectionResult selectionResult)
        {
            if (cardNameText != null)
            {
                cardNameText.SetText(selectionResult.SelectedCardName);
            }

            if (multiplierText != null)
            {
                multiplierText.SetText(string.Format("x{0:0.0}", selectionResult.AppliedMultiplier));
            }

            if (baseRewardText != null)
            {
                baseRewardText.SetText(string.Format(PartTimeTextConstants.BASE_REWARD_SHORT_FORMAT, selectionResult.BaseReward));
            }

            if (conditionDeltaText != null)
            {
                if (selectionResult.ConditionDelta > 0)
                {
                    conditionDeltaText.SetText(string.Format(PartTimeTextConstants.CONDITION_PLUS_FORMAT, selectionResult.ConditionDelta));
                }
                else if (selectionResult.ConditionDelta < 0)
                {
                    conditionDeltaText.SetText(string.Format(PartTimeTextConstants.CONDITION_MINUS_FORMAT, selectionResult.ConditionDelta));
                }
                else
                {
                    conditionDeltaText.SetText(PartTimeTextConstants.CONDITION_STAY_TEXT);
                }
            }
        }

        /// <summary>
        /// 참여 멤버 아이콘 표시 구성
        /// </summary>
        /// <param name="members">PartTime 참여 멤버 목록</param>
        private void SetMembers(IReadOnlyList<MemberDataSO> members)
        {
            if (memberImages == null)
            {
                return;
            }

            int iconCount = memberImages.Count;
            for (int i = 0; i < iconCount; i++)
            {
                GameObject iconImage = memberImages[i];
                if (iconImage == null)
                {
                    continue;
                }

                bool shouldShow = members != null && i < members.Count && members[i] != null;
                iconImage.gameObject.SetActive(shouldShow);
            }
        }

        /// <summary>
        /// 정산 흐름 요소를 순차적으로 표시
        /// </summary>
        /// <param name="beforeGold">알바 이전 금액</param>
        /// <param name="rewardGold">보상 금액</param>
        /// <param name="finalGold">알바 이후 최종 금액</param>
        private async UniTask PlayFinancialFlowAsync(int beforeGold, int rewardGold, int finalGold)
        {
            await ShowMoneyIconAsync();
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_INTERVAL_DURATION));
            await ShowBeforeGoldAsync(beforeGold);
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_INTERVAL_DURATION));
            await ShowFlowSymbolAsync();
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_INTERVAL_DURATION));
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_INTERVAL_DURATION));
            await ShowAfterGoldCountUpAsync(beforeGold, finalGold);
        }

        /// <summary>
        /// 돈 아이콘 단계 노출
        /// </summary>
        private async UniTask ShowMoneyIconAsync()
        {
            if (moneyIconImage == null)
            {
                return;
            }

            moneyIconImage.gameObject.SetActive(true);
            RectTransform iconRect = moneyIconImage.rectTransform;
            iconRect.localScale = Vector3.one * 0.8f;
            iconRect.DOScale(1f, STEP_APPEAR_DURATION).SetEase(Ease.OutBack);
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_APPEAR_DURATION));
        }

        /// <summary>
        /// 이전 금액 단계 노출
        /// </summary>
        /// <param name="beforeGold">알바 이전 금액</param>
        private async UniTask ShowBeforeGoldAsync(int beforeGold)
        {
            if (beforeGoldText == null)
            {
                return;
            }

            beforeGoldText.SetText(FormatGold(beforeGold));
            beforeGoldText.gameObject.SetActive(true);
            beforeGoldText.rectTransform.localScale = Vector3.one * 0.8f;
            beforeGoldText.rectTransform.DOScale(1f, STEP_APPEAR_DURATION).SetEase(Ease.OutBack);
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_APPEAR_DURATION));
        }

        /// <summary>
        /// 진행 심볼 단계 노출
        /// </summary>
        private async UniTask ShowFlowSymbolAsync()
        {
            if (flowSymbolIconImage == null)
            {
                return;
            }

            flowSymbolIconImage.gameObject.SetActive(true);
            flowSymbolIconImage.rectTransform.localScale = Vector3.one * 0.8f;
            flowSymbolIconImage.rectTransform.DOScale(1f, STEP_APPEAR_DURATION).SetEase(Ease.OutBack);
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_APPEAR_DURATION));
        }

        

        /// <summary>
        /// 최종 금액을 이전 금액에서 카운트업 연출
        /// </summary>
        /// <param name="beforeGold">알바 이전 금액</param>
        /// <param name="finalGold">알바 이후 최종 금액</param>
        private async UniTask ShowAfterGoldCountUpAsync(int beforeGold, int finalGold)
        {
            if (afterGoldText == null)
            {
                return;
            }

            afterGoldText.SetText(FormatGold(beforeGold));
            afterGoldText.gameObject.SetActive(true);
            afterGoldText.rectTransform.localScale = Vector3.one * 0.8f;
            afterGoldText.rectTransform.DOScale(1f, STEP_APPEAR_DURATION).SetEase(Ease.OutBack);
            await UniTask.Delay(System.TimeSpan.FromSeconds(STEP_APPEAR_DURATION));

            if (finalGold <= beforeGold)
            {
                return;
            }

            int displayValue = beforeGold;
            float durationRatio = Mathf.Clamp01((float)(finalGold - beforeGold) / 1500f);
            float countDuration = Mathf.Lerp(AFTER_GOLD_BASE_COUNT_DURATION, AFTER_GOLD_MAX_COUNT_DURATION, durationRatio);
            Tween tween = DOTween.To(() => displayValue, delegate(int value)
            {
                displayValue = value;
                afterGoldText.SetText(FormatGold(displayValue));
            }, finalGold, countDuration).SetEase(Ease.OutCubic);

            await UniTask.Delay(System.TimeSpan.FromSeconds(countDuration));

            Vector3 punch = new Vector3(0.15f, 0.15f, 0f);
            afterGoldText.rectTransform.DOPunchScale(punch, AFTER_GOLD_PUNCH_DURATION, 6, 0.5f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(AFTER_GOLD_PUNCH_DURATION));
        }

        /// <summary>
        /// 확인 버튼 입력 대기
        /// </summary>
        private async UniTask WaitForConfirmAsync()
        {
            if (confirmButton == null)
            {
                return;
            }

            _confirmCompletionSource = new UniTaskCompletionSource();

            if (_confirmAction != null)
            {
                confirmButton.onClick.RemoveListener(_confirmAction);
            }

            _confirmAction = HandleConfirmButtonClicked;
            confirmButton.onClick.AddListener(_confirmAction);
            await _confirmCompletionSource.Task;
            confirmButton.onClick.RemoveListener(_confirmAction);
            _confirmAction = null;
            _confirmCompletionSource = null;
        }

        /// <summary>
        /// 팝업 닫기 연출 수행
        /// </summary>
        private async UniTask ClosePopupAsync()
        {
            KillConfirmPulse();

            popupRoot.SetActive(false);
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 확인 버튼 클릭 처리
        /// </summary>
        private void HandleConfirmButtonClicked()
        {
            if (_confirmCompletionSource != null)
            {
                _confirmCompletionSource.TrySetResult();
            }
        }

        /// <summary>
        /// 정산 영역 초기 상태 적용
        /// </summary>
        private void ResetFinancialDisplay()
        {
            SetElementVisible(moneyIconImage != null ? moneyIconImage.gameObject : null, false);
            SetElementVisible(beforeGoldText != null ? beforeGoldText.gameObject : null, false);
            SetElementVisible(flowSymbolIconImage != null ? flowSymbolIconImage.gameObject : null, false);
            SetElementVisible(afterGoldText != null ? afterGoldText.gameObject : null, false);

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
                confirmButton.transform.localScale = Vector3.one;
            }

            KillConfirmPulse();
        }

        /// <summary>
        /// 확인 버튼 펄스 연출 시작
        /// </summary>
        private void EnableConfirmButtonPulse()
        {
            if (confirmButton == null)
            {
                return;
            }

            confirmButton.interactable = true;
            confirmButton.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 확인 버튼 펄스 연출 종료
        /// </summary>
        private void KillConfirmPulse()
        {
            if (_confirmPulseTween != null && _confirmPulseTween.IsActive())
            {
                _confirmPulseTween.Kill();
            }

            _confirmPulseTween = null;
        }

        /// <summary>
        /// 금액 문자열 포맷 반환
        /// </summary>
        /// <param name="amount">출력 금액</param>
        /// <returns>천 단위 구분 문자열</returns>
        private static string FormatGold(int amount)
        {
            return string.Format("{0:N0}원", amount);
        }

        /// <summary>
        /// UI 요소 표시 상태 설정
        /// </summary>
        /// <param name="target">대상 오브젝트</param>
        /// <param name="visible">표시 여부</param>
        private static void SetElementVisible(GameObject target, bool visible)
        {
            if (target == null)
            {
                return;
            }

            target.SetActive(visible);

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 리스너 및 트윈 정리
        /// </summary>
        private void OnDestroy()
        {
            KillConfirmPulse();

            if (confirmButton != null && _confirmAction != null)
            {
                confirmButton.onClick.RemoveListener(_confirmAction);
            }
        }
    }
}
