using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.Tree.Addon;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.MusicRelated
{
    [System.Serializable]
    public class MemberStatData
    {
        public int currentValue;
        public StatRankType currentRank;

        public MemberStatData(int value, StatRankType rank)
        {
            currentValue = value;
            currentRank = rank;
        }
    }

    [System.Serializable]
    public class RankIconEntry
    {
        public StatRankType rank;
        public Sprite icon;
    }

    public class GameStatManager : BaseGameStatAddon
    {
        public static GameStatManager Instance;

        private int _musicPerfectionPercent = 0;
        private Dictionary<MusicRelatedStatsType, int> _musicPerfectionStats = new();

        private Dictionary<MemberType, MemberStatData> _memberCompositionStats  = new();
        private Dictionary<MemberType, MemberStatData> _memberInstrumentStats   = new();
        private Dictionary<MemberType, int>            _memberEarnedStats       = new();

        private int _activityEfficiencyBonus = 0;
        public int ActivityEfficiencyBonus => _activityEfficiencyBonus;

        [Header("완성도 증가 설정 (멤버 1명당 % 증가량)")]
        [SerializeField] private int percentIncreasePerActivity = 5;

        [Header("등급 아이콘 (F ~ L 순서대로 18개 할당)")]
        [SerializeField] private List<RankIconEntry> rankIcons = new();

        public event Action OnStatsChanged;

        /// <summary>
        /// 음악 완성도(%)가 변경될 때 발생. SaveBinder에서 구독해 100% 달성 시 즉시 강제 저장에 사용.
        /// </summary>
        public event Action<int> OnMusicPercentChanged;

        private const int MAX_STAT_VALUE = 2500;

        private static readonly List<(StatRankType rank, int min, int max)> DefaultRankRanges = new()
        {
            (StatRankType.F,    0,    49),
            (StatRankType.E,    50,   109),
            (StatRankType.EP,   110,  179),
            (StatRankType.D,    180,  264),
            (StatRankType.DP,   265,  364),
            (StatRankType.C,    365,  484),
            (StatRankType.CP,   485,  619),
            (StatRankType.B,    620,  784),
            (StatRankType.BP,   785,  964),
            (StatRankType.A,    965,  1179),
            (StatRankType.AP,   1180, 1409),
            (StatRankType.S,    1410, 1659),
            (StatRankType.SP,   1660, 1909),
            (StatRankType.SS,   1910, 2109),
            (StatRankType.SSP,  2110, 2284),
            (StatRankType.SSS,  2285, 2424),
            (StatRankType.SSSP, 2425, 2499),
            (StatRankType.L,    2500, int.MaxValue),
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStats();
                EventHandle();

                var save = Code.SubSystem.Save.SaveManager.Instance;
                if (save != null && save.HasSave)
                {
                    var data = save.Data;
                    foreach (var entry in data.memberStats)
                    {
                        _memberCompositionStats[entry.memberType] =
                            new MemberStatData(entry.composition, entry.compositionRank);
                        _memberInstrumentStats[entry.memberType] =
                            new MemberStatData(entry.instrumentProficiency, entry.instrumentRank);
                    }
                    foreach (var entry in data.musicStats)
                        _musicPerfectionStats[entry.statType] = entry.value;
                    _musicPerfectionPercent = data.musicPerfectionPercent;

                    Debug.Log($"[GameStatManager] 복원 완료: 멤버 {data.memberStats.Count}명, 완성도 {_musicPerfectionPercent}%");
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void InitializeStats()
        {
            _musicPerfectionStats[MusicRelatedStatsType.Lyrics]     = 0;
            _musicPerfectionStats[MusicRelatedStatsType.Teamwork]    = 0;
            _musicPerfectionStats[MusicRelatedStatsType.Proficiency] = 0;
            _musicPerfectionStats[MusicRelatedStatsType.Melody]      = 0;

            int initialValue         = GetInitialValue();
            StatRankType initialRank = CalculateRank(initialValue);

            foreach (MemberType memberType in Enum.GetValues(typeof(MemberType)))
            {
                _memberEarnedStats[memberType]      = 0;
                _memberCompositionStats[memberType] = new MemberStatData(initialValue, initialRank);
                _memberInstrumentStats[memberType]  = new MemberStatData(initialValue, initialRank);
            }

            Debug.Log($"[GameStatManager] 초기화: {initialValue} ({initialRank})");
        }

        /// <summary>
        /// 완성도를 직접 지정값으로 세팅. 내부적으로 OnMusicPercentChanged를 발생시킨다.
        /// </summary>
        public void SetMusicPercent(int percent)
        {
            _musicPerfectionPercent = percent;
            OnMusicPercentChanged?.Invoke(_musicPerfectionPercent);
            OnStatsChanged?.Invoke();
        }

        public void AddScore(int score, MusicRelatedStatsType type)
        {
            if (_musicPerfectionStats.ContainsKey(type))
                _musicPerfectionStats[type] = Mathf.Max(0, _musicPerfectionStats[type] + score);
            else
                _musicPerfectionStats.Add(type, Mathf.Max(0, score));

            OnStatsChanged?.Invoke();
        }

        public void AddMemberStat(MemberType memberType, int amount)
        {
            if (_memberEarnedStats.ContainsKey(memberType))
                _memberEarnedStats[memberType] += amount;
            else
                _memberEarnedStats[memberType] = amount;
        }

        public void ApplyAllMemberStats(MusicRelatedStatsType statType)
        {
            foreach (var kvp in _memberEarnedStats)
            {
                if (kvp.Value <= 0) continue;

                MemberType memberType = kvp.Key;
                int amount = kvp.Value;

                if (statType == MusicRelatedStatsType.Composition)
                {
                    if (!_memberCompositionStats.ContainsKey(memberType))
                    {
                        int iv = GetInitialValue();
                        _memberCompositionStats[memberType] = new MemberStatData(iv, CalculateRank(iv));
                    }

                    var statData = _memberCompositionStats[memberType];
                    statData.currentValue = Mathf.Min(statData.currentValue + amount, MAX_STAT_VALUE);
                    statData.currentRank  = CalculateRank(statData.currentValue);

                    Debug.Log($"[GameStatManager] {memberType} Composition 적용: +{amount} → {statData.currentValue} ({statData.currentRank})");
                }
                else if (statType == MusicRelatedStatsType.InstrumentProficiency)
                {
                    if (!_memberInstrumentStats.ContainsKey(memberType))
                    {
                        int iv = GetInitialValue();
                        _memberInstrumentStats[memberType] = new MemberStatData(iv, CalculateRank(iv));
                    }

                    var statData = _memberInstrumentStats[memberType];
                    statData.currentValue = Mathf.Min(statData.currentValue + amount, MAX_STAT_VALUE);
                    statData.currentRank  = CalculateRank(statData.currentValue);

                    Debug.Log($"[GameStatManager] {memberType} InstrumentProficiency 적용: +{amount} → {statData.currentValue} ({statData.currentRank})");
                }
            }

            foreach (var memberType in Enum.GetValues(typeof(MemberType)))
                _memberEarnedStats[(MemberType)memberType] = 0;

            OnStatsChanged?.Invoke();
            Debug.Log("[GameStatManager] 모든 멤버 스탯 적용 완료");
        }

        public void AddMemberStatDirect(MemberType memberType, MusicRelatedStatsType statType, int amount)
        {
            var targetDict = statType == MusicRelatedStatsType.Composition
                ? _memberCompositionStats
                : _memberInstrumentStats;

            if (!targetDict.ContainsKey(memberType))
            {
                int iv = GetInitialValue();
                targetDict[memberType] = new MemberStatData(iv, CalculateRank(iv));
            }

            var statData = targetDict[memberType];
            statData.currentValue = Mathf.Min(statData.currentValue + amount, MAX_STAT_VALUE);
            statData.currentRank  = CalculateRank(statData.currentValue);

            Debug.Log($"[GameStatManager] {memberType} {statType} 직접 추가: +{amount} → {statData.currentValue} ({statData.currentRank})");
            OnStatsChanged?.Invoke();
        }

        public void AddActivityEfficiencyBonus(int percent)
        {
            _activityEfficiencyBonus += percent;
            Debug.Log($"[GameStatManager] 인스턴스ID: {GetInstanceID()} / 행동 효율 보너스: {_activityEfficiencyBonus}%");
        }

        public StatRankType CalculateRankPublic(int value) => CalculateRank(value);

        private StatRankType CalculateRank(int value)
        {
            value = Mathf.Clamp(value, 0, MAX_STAT_VALUE);

            if (value >= MAX_STAT_VALUE) return StatRankType.L;

            foreach (var (rank, min, max) in DefaultRankRanges)
            {
                if (value >= min && value <= max)
                    return rank;
            }

            return StatRankType.F;
        }

        public int GetCurrentRankMin(MemberType memberType, MusicRelatedStatsType statType)
        {
            MemberStatData statData = GetStatData(memberType, statType);
            if (statData == null) return 0;

            foreach (var (rank, min, _) in DefaultRankRanges)
            {
                if (rank == statData.currentRank) return min;
            }
            return 0;
        }

        public int GetNextRankMax(MemberType memberType, MusicRelatedStatsType statType)
        {
            MemberStatData statData = GetStatData(memberType, statType);
            if (statData == null) return 10;

            foreach (var (rank, _, max) in DefaultRankRanges)
            {
                if (rank == statData.currentRank) return max;
            }
            return 10;
        }

        private MemberStatData GetStatData(MemberType memberType, MusicRelatedStatsType statType)
        {
            if (statType == MusicRelatedStatsType.Composition && _memberCompositionStats.ContainsKey(memberType))
                return _memberCompositionStats[memberType];
            if (statType == MusicRelatedStatsType.InstrumentProficiency && _memberInstrumentStats.ContainsKey(memberType))
                return _memberInstrumentStats[memberType];
            return null;
        }

        public Sprite GetRankIcon(StatRankType rank)
        {
            if (rankIcons == null) return null;
            var entry = rankIcons.Find(e => e.rank == rank);
            return entry?.icon;
        }

        private int GetInitialValue()
        {
            foreach (var (rank, min, _) in DefaultRankRanges)
            {
                if (rank == StatRankType.F) return min;
            }
            return 0;
        }

        public int GetMemberEarnedStat(MemberType memberType)
        {
            return _memberEarnedStats.GetValueOrDefault(memberType, 0);
        }

        public MemberStatData GetMemberStatData(MemberType memberType, MusicRelatedStatsType statType)
        {
            if (statType == MusicRelatedStatsType.Composition)
            {
                if (!_memberCompositionStats.ContainsKey(memberType))
                {
                    int iv = GetInitialValue();
                    _memberCompositionStats[memberType] = new MemberStatData(iv, CalculateRank(iv));
                }
                return _memberCompositionStats[memberType];
            }
            else if (statType == MusicRelatedStatsType.InstrumentProficiency)
            {
                if (!_memberInstrumentStats.ContainsKey(memberType))
                {
                    int iv = GetInitialValue();
                    _memberInstrumentStats[memberType] = new MemberStatData(iv, CalculateRank(iv));
                }
                return _memberInstrumentStats[memberType];
            }

            return new MemberStatData(0, StatRankType.F);
        }

        public Dictionary<MemberType, int> GetAllMemberEarnedStats()
        {
            return new Dictionary<MemberType, int>(_memberEarnedStats);
        }

        /// <summary>
        /// 활동 완료 시 완성도를 증가시킨다. 100% 도달 시 OnMusicPercentChanged를 통해
        /// SaveBinder가 즉시 강제 저장하도록 알린다.
        /// </summary>
        public void CompleteActivity(int memberCount = 1)
        {
            int increase = percentIncreasePerActivity * memberCount;
            _musicPerfectionPercent = Mathf.Min(100, _musicPerfectionPercent + increase);
            Debug.Log($"[GameStatManager] CompleteActivity: memberCount={memberCount}, +{increase}% → {_musicPerfectionPercent}%");

            OnMusicPercentChanged?.Invoke(_musicPerfectionPercent);
            OnStatsChanged?.Invoke();
        }

        public int GetMusicPerfectionPercent() => _musicPerfectionPercent;

        public int GetScore(MusicRelatedStatsType type)
        {
            return _musicPerfectionStats.ContainsKey(type) ? _musicPerfectionStats[type] : 0;
        }

        public Dictionary<MusicRelatedStatsType, int> GetAllStats()
        {
            return new Dictionary<MusicRelatedStatsType, int>(_musicPerfectionStats);
        }

        public void ResetStats()
        {
            foreach (var memberType in Enum.GetValues(typeof(MemberType)))
                _memberEarnedStats[(MemberType)memberType] = 0;

            Debug.Log("[GameStatManager] earned 스탯만 초기화 (세부스탯/완성도 유지)");
        }

        public void ResetMusicAll()
        {
            ApplyAllUpgrades();

            _musicPerfectionPercent  = 0;
            _activityEfficiencyBonus = 0;

            _musicPerfectionStats[MusicRelatedStatsType.Lyrics] = BaseSongLyricsValue;
            _musicPerfectionStats[MusicRelatedStatsType.Teamwork] = BaseSongTeamworkValue;
            _musicPerfectionStats[MusicRelatedStatsType.Proficiency] = BaseSongProficiencyValue;
            _musicPerfectionStats[MusicRelatedStatsType.Melody] = BaseSongMelodyValue;

            ResetUpgradeValue();

            OnMusicPercentChanged?.Invoke(_musicPerfectionPercent);
            OnStatsChanged?.Invoke();
        }
        
        public void ResetAllForTutorial()
        {
            _musicPerfectionPercent = 0;
            _activityEfficiencyBonus = 0;

            _musicPerfectionStats.Clear();
            _memberCompositionStats.Clear();
            _memberInstrumentStats.Clear();
            _memberEarnedStats.Clear();

            InitializeStats();
            OnMusicPercentChanged?.Invoke(_musicPerfectionPercent);
            OnStatsChanged?.Invoke();
        }

        public void ResetPercent(MusicRelatedStatsType type)
        {
            if (_musicPerfectionStats.ContainsKey(type))
            {
                _musicPerfectionStats[type] = 0;
                OnStatsChanged?.Invoke();
            }
        }

        public void ApplyMemberStatForAnimation(MemberType memberType, MusicRelatedStatsType statType, int amount)
        {
            Debug.Log($"[GameStatManager] {memberType} 애니메이션: +{amount}");
        }

        public int GetTotalMemberExpenses(List<RankExpenseEntry> rankExpenses)
        {
            var memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));
            int total = 0;

            foreach (MemberType memberType in memberTypes)
            {
                int comp = _memberCompositionStats.ContainsKey(memberType)
                    ? _memberCompositionStats[memberType].currentValue : GetInitialValue();
                int inst = _memberInstrumentStats.ContainsKey(memberType)
                    ? _memberInstrumentStats[memberType].currentValue : GetInitialValue();

                int avg = (comp + inst) / 2;
                StatRankType rank = CalculateRank(avg);

                var entry = rankExpenses.Find(e => e.rank == rank);
                int expense = entry?.monthlyExpense ?? 0;

                Debug.Log($"[GameStatManager] {memberType} 평균 스탯: {avg} ({rank}) → 개인 지출: {expense}");
                total += expense;
            }

            Debug.Log($"[GameStatManager] 전체 멤버 지출 합산: {total}");
            return total;
        }

        public StatRankType GetAverageMemberRank()
        {
            var memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));
            if (memberTypes.Length == 0) return StatRankType.F;

            int total = 0;
            int count = 0;

            foreach (MemberType memberType in memberTypes)
            {
                int comp = _memberCompositionStats.ContainsKey(memberType)
                    ? _memberCompositionStats[memberType].currentValue : GetInitialValue();
                int inst = _memberInstrumentStats.ContainsKey(memberType)
                    ? _memberInstrumentStats[memberType].currentValue : GetInitialValue();

                total += (comp + inst) / 2;
                count++;
            }

            int avg = count > 0 ? total / count : 0;
            return CalculateRank(avg);
        }
    }
}