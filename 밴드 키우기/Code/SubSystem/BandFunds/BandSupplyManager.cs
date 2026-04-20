using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.Tree.Addon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.SubSystem.BandFunds
{
    [System.Serializable]
    public class FixedExpenseEntry
    {
        public string label;
        public int    amount;
    }

    [System.Serializable]
    public class RankExpenseEntry
    {
        [HideInInspector] public StatRankType rank;
        [HideInInspector] public string rankLabel;
        public int monthlyExpense;
    }

    public class BandSupplyManager : BaseSupplyAddon
    {
        public static BandSupplyManager Instance;

        private int _bandFunds = 2500;
        public int BandFunds => _bandFunds;

        public int BandExp  { get; private set; }
        public int BandFans { get; private set; }

        [Header("고정 월 지출 항목")]
        [SerializeField] private List<FixedExpenseEntry> fixedExpenses = new()
        {
            new FixedExpenseEntry { label = "전기세",       amount = 300 },
            new FixedExpenseEntry { label = "식비",         amount = 500 },
            new FixedExpenseEntry { label = "연습실 임대료", amount = 700 },
        };
        public int MonthlyExpenses                  => GetTotalFixedExpenses();
        public List<FixedExpenseEntry> FixedExpenses => fixedExpenses;

        [Header("등급별 월 지출 설정 (멤버 1인당)")]
        [SerializeField] private bool useRankBasedExpenses = true;
        [SerializeField] private List<RankExpenseEntry> rankExpenses = new();

        [Header("월 명세서 UI")]
        [SerializeField] private MonthlyExpenseUI monthlyExpenseUI;

        [Header("어워드 씬")]
        [SerializeField] private string awardSceneName = "AwardScene";

        private int  _pendingExpense;
        private int  _pendingMonth;
        private bool _pendingYearEnd = false;

        private static readonly List<StatRankType> AllRanks = new()
        {
            StatRankType.F,  StatRankType.E,    StatRankType.EP,
            StatRankType.D,  StatRankType.DP,
            StatRankType.C,  StatRankType.CP,
            StatRankType.B,  StatRankType.BP,
            StatRankType.A,  StatRankType.AP,
            StatRankType.S,  StatRankType.SP,
            StatRankType.SS, StatRankType.SSP,
            StatRankType.SSS,StatRankType.SSSP,
            StatRankType.L,
        };

        private static readonly Dictionary<StatRankType, int> DefaultRankExpenses = new()
        {
            { StatRankType.F,    100   },
            { StatRankType.E,    200   },
            { StatRankType.EP,   350   },
            { StatRankType.D,    500   },
            { StatRankType.DP,   700   },
            { StatRankType.C,    1000  },
            { StatRankType.CP,   1300  },
            { StatRankType.B,    1700  },
            { StatRankType.BP,   2200  },
            { StatRankType.A,    2800  },
            { StatRankType.AP,   3500  },
            { StatRankType.S,    4500  },
            { StatRankType.SP,   5500  },
            { StatRankType.SS,   7000  },
            { StatRankType.SSP,  8500  },
            { StatRankType.SSS,  10000 },
            { StatRankType.SSSP, 12000 },
            { StatRankType.L,    15000 },
        };

        public event System.Action OnExpChanged;
        public event System.Action OnFansChanged;
        public event System.Action OnFundsChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeRankExpensesIfEmpty();
                EventHandle();
                Bus<TreeUpgradeEvent>.OnEvent += HandleTreeUpgrade;

                var save = Code.SubSystem.Save.SaveManager.Instance;
                if (save != null && save.HasSave)
                {
                    _bandFunds = save.Data.bandFunds;
                    BandExp    = save.Data.bandExp;
                    BandFans   = save.Data.bandFans;
                    Debug.Log($"[BandSupplyManager] 복원: 돈={_bandFunds}, 경험치={BandExp}, 팬={BandFans}");
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnYearEnd += HandleYearEnd;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Bus<TreeUpgradeEvent>.OnEvent -= HandleTreeUpgrade;
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnYearEnd -= HandleYearEnd;
            }
        }

        private void HandleTreeUpgrade(TreeUpgradeEvent evt)
        {
            if (evt.Type != TreeUpgradeType.Supply) return;
            Upgrades.Add(evt.UpgradeSO);
        }

        private void HandleYearEnd()
        {
            _pendingYearEnd = true;
        }

        public void HandleMonthEnd(int month)
        {
            int fixedExpense  = MonthlyExpenses;
            int memberExpense = useRankBasedExpenses && GameStatManager.Instance != null
                ? GameStatManager.Instance.GetTotalMemberExpenses(rankExpenses)
                : 0;

            if (fixedExpense <= 0 && memberExpense <= 0)
            {
                if (_pendingYearEnd)
                {
                    _pendingYearEnd = false;
                    Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(awardSceneName));
                }
                return;
            }

            StatRankType avgRank = GameStatManager.Instance != null
                ? GameStatManager.Instance.GetAverageMemberRank()
                : StatRankType.F;

            if (monthlyExpenseUI != null)
            {
                monthlyExpenseUI.OnConfirmed -= OnExpenseConfirmed;
                monthlyExpenseUI.OnConfirmed += OnExpenseConfirmed;
                monthlyExpenseUI.Show(fixedExpenses, memberExpense, avgRank);

                _pendingExpense = fixedExpense + memberExpense;
                _pendingMonth   = month;
            }
            else
            {
                ApplyExpense(fixedExpense + memberExpense, month);
                if (_pendingYearEnd)
                {
                    _pendingYearEnd = false;
                    Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(awardSceneName));
                }
            }
        }

        private void OnExpenseConfirmed()
        {
            monthlyExpenseUI.OnConfirmed -= OnExpenseConfirmed;
            ApplyExpense(_pendingExpense, _pendingMonth);

            if (_pendingYearEnd)
            {
                _pendingYearEnd = false;
                Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(awardSceneName));
            }
        }

        private void ApplyExpense(int amount, int month)
        {
            _bandFunds = Mathf.Max(0, _bandFunds - amount);
            Bus<MoneyChangedEvent>.Raise(new MoneyChangedEvent { TotalEarned = _bandFunds });
            OnFundsChanged?.Invoke();
            Debug.Log($"[BandSupplyManager] {month}월 지출 -{amount} → 잔액 {_bandFunds}");
        }

        public int GetCurrentMonthlyExpense()
        {
            int base_ = useRankBasedExpenses && GameStatManager.Instance != null
                ? GameStatManager.Instance.GetTotalMemberExpenses(rankExpenses)
                : 0;
            return GetTotalFixedExpenses() + base_;
        }

        private int GetTotalFixedExpenses()
        {
            int total = 0;
            foreach (var entry in fixedExpenses)
                total += entry.amount;
            return total;
        }

        private void InitializeRankExpensesIfEmpty()
        {
            foreach (var rank in AllRanks)
            {
                var existing = rankExpenses.Find(e => e.rank == rank);
                if (existing != null)
                    existing.rankLabel = rank.ToString();
                else
                    rankExpenses.Add(new RankExpenseEntry
                    {
                        rank           = rank,
                        rankLabel      = rank.ToString(),
                        monthlyExpense = DefaultRankExpenses.GetValueOrDefault(rank, 0),
                    });
            }
        }

        public void AddBandFunds(int amount)
        {
            _bandFunds = Mathf.Max(0, _bandFunds + amount);
            Bus<MoneyChangedEvent>.Raise(new MoneyChangedEvent { TotalEarned = _bandFunds });
            OnFundsChanged?.Invoke();
        }

        public bool SpendBandFunds(int amount)
        {
            if (_bandFunds < amount)
            {
                Debug.LogWarning($"[BandSupplyManager] 자금 부족 (보유: {_bandFunds}, 필요: {amount})");
                return false;
            }
            _bandFunds -= amount;
            Bus<MoneyChangedEvent>.Raise(new MoneyChangedEvent { TotalEarned = _bandFunds });
            OnFundsChanged?.Invoke();
            return true;
        }

        public bool CheckBandFunds(int amount) => _bandFunds >= amount;

        public void AddBandExp(int amount)
        {
            ApplyAllUpgrades();
            amount += ExpPlusValue;
            BandExp += amount;
            ResetUpgradeValue();
            OnExpChanged?.Invoke();
        }

        public bool SpendBandExp(int amount)
        {
            if (BandExp < amount) return false;
            BandExp -= amount;
            OnExpChanged?.Invoke();
            return true;
        }

        public bool CheckBandExp(int amount) => BandExp >= amount;

        public void AddBandFans(int amount)
        {
            BandFans += amount;
            OnFansChanged?.Invoke();
        }

        public void RemoveBandFans(int amount)
        {
            BandFans = Mathf.Max(0, BandFans - amount);
            OnFansChanged?.Invoke();
        }

        /// <summary>
        /// 튜토리얼 종료 시 밴드 재화 초기화.
        /// </summary>
        /// <param name="bandFunds">초기 자금 값.</param>
        /// <param name="bandExp">초기 경험치 값.</param>
        /// <param name="bandFans">초기 팬 수 값.</param>
        public void ResetBandSupplyForTutorial(int bandFunds, int bandExp, int bandFans)
        {
            _bandFunds = Mathf.Max(0, bandFunds);
            BandExp = Mathf.Max(0, bandExp);
            BandFans = Mathf.Max(0, bandFans);

            Bus<MoneyChangedEvent>.Raise(new MoneyChangedEvent { TotalEarned = _bandFunds });
            OnExpChanged?.Invoke();
            OnFansChanged?.Invoke();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                AddBandExp(100);

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                AddBandFunds(10000);
                Debug.Log($"[Debug] 돈 +10000 → 잔액 {_bandFunds}");
            }
        }
#endif
    }
}
