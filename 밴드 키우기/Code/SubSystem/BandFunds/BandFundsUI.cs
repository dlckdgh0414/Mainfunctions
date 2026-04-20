using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.MainSystem.MusicRelated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.BandFunds
{
    public class BandFundsUI : MonoBehaviour
    {
        [SerializeField] private Image bandfundsImage;
        [SerializeField] private TextMeshProUGUI bandfundsText;
        [SerializeField] private TextMeshProUGUI expenseText;
        [SerializeField] private Sprite bandfundsSprite;

        private int _lastExpense = -1;
        private bool _statSubscribed = false;

        private void Awake()
        {
            bandfundsImage.sprite = bandfundsSprite;
        }

        private void Start()
        {
            RefreshText();
            Bus<MoneyChangedEvent>.OnEvent += HandleMoneyChangedEvent;
            TrySubscribeStats();
        }

        private void Update()
        {
            if (!_statSubscribed)
                TrySubscribeStats();
            
            if (BandSupplyManager.Instance != null)
            {
                int current = BandSupplyManager.Instance.GetCurrentMonthlyExpense();
                if (current != _lastExpense)
                {
                    _lastExpense = current;
                    RefreshExpenseText(current);
                }
            }
        }

        private void TrySubscribeStats()
        {
            if (GameStatManager.Instance == null) return;
            GameStatManager.Instance.OnStatsChanged += RefreshText;
            _statSubscribed = true;
        }

        private void OnDestroy()
        {
            Bus<MoneyChangedEvent>.OnEvent -= HandleMoneyChangedEvent;
            if (GameStatManager.Instance != null)
                GameStatManager.Instance.OnStatsChanged -= RefreshText;
        }

        private void HandleMoneyChangedEvent(MoneyChangedEvent evt)
        {
            RefreshText();
        }

        private void RefreshText()
        {
            if (BandSupplyManager.Instance == null) return;

            bandfundsText.SetText($"{BandSupplyManager.Instance.BandFunds}");
            int expense = BandSupplyManager.Instance.GetCurrentMonthlyExpense();
            _lastExpense = expense;
            RefreshExpenseText(expense);
        }

        private void RefreshExpenseText(int expense)
        {
            expenseText.SetText($"월 지출액 : {expense}");
        }
    }
}