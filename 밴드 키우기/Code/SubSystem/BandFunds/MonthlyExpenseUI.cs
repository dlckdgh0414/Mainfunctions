using System;
using System.Collections.Generic;
using Code.MainSystem.StatSystem.BaseStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.BandFunds
{
    public class MonthlyExpenseUI : MonoBehaviour
    {
        [Header("패널")]
        [SerializeField] private GameObject panel;

        [Header("고정 지출 행 (최대 4개)")]
        [SerializeField] private TextMeshProUGUI fixedLabel1;  [SerializeField] private TextMeshProUGUI fixedAmount1;
        [SerializeField] private TextMeshProUGUI fixedLabel2;  [SerializeField] private TextMeshProUGUI fixedAmount2;
        [SerializeField] private TextMeshProUGUI fixedLabel3;  [SerializeField] private TextMeshProUGUI fixedAmount3;
        [SerializeField] private List<GameObject> fixedRows;

        [Header("멤버 지출 행")]
        [SerializeField] private TextMeshProUGUI memberLabelText;
        [SerializeField] private TextMeshProUGUI memberAmountText;

        [Header("총 지출액")]
        [SerializeField] private TextMeshProUGUI totalAmountText;

        [Header("확인 버튼")]
        [SerializeField] private Button confirmButton;

        public event Action OnConfirmed;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(HandleConfirm);
            Hide();
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();
        }

        public void Show(List<FixedExpenseEntry> fixedExpenses, int memberExpense, StatRankType avgRank)
        {
            var labels  = new[] { fixedLabel1,  fixedLabel2,  fixedLabel3};
            var amounts = new[] { fixedAmount1, fixedAmount2, fixedAmount3};

            int totalFixed = 0;
            for (int i = 0; i < labels.Length; i++)
            {
                bool hasEntry = i < fixedExpenses.Count;
                if (fixedRows != null && i < fixedRows.Count && fixedRows[i] != null)
                    fixedRows[i].SetActive(hasEntry);

                if (!hasEntry) continue;

                totalFixed += fixedExpenses[i].amount;
                if (labels[i]  != null) labels[i].text  = fixedExpenses[i].label;
                if (amounts[i] != null) amounts[i].text = $"{fixedExpenses[i].amount:N0}원";
            }

            if (memberLabelText  != null) memberLabelText.text  = $"멤버 지출";
            if (memberAmountText != null) memberAmountText.text = $"{memberExpense:N0}원";
            if (totalAmountText  != null) totalAmountText.text  = $"{totalFixed + memberExpense:N0}원";

            if (panel != null) panel.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            gameObject.SetActive(false);
        }

        private void HandleConfirm()
        {
            Hide();
            OnConfirmed?.Invoke();
        }
    }
}