using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;
using UnityEngine;

namespace Code.MainSystem.behavior
{
    [Serializable]
    public class StatWeight
    {
        public MusicRelatedStatsType statType;
        [Range(0f, 1f)] public float weight;
        [Range(1, 50)] public int minIncrease;
        [Range(1, 50)] public int maxIncrease;
    }

    [Serializable]
    public class RoundProjectileRange
    {
        public int round;
        public int minProjectiles;
        public int maxProjectiles;
    }

    [Serializable]
    public class RankContinueChance
    {
        [Range(0f, 1f)] public float baseChance;
        [Range(0.1f, 0.95f)] public float decayPerRound;
    }

    /// <summary>
    /// 랭크별 멤버당 프로젝타일 발사 수 배수.
    /// F=0.8 기준으로 등급이 오를수록 배수가 커져
    /// 고랭크일수록 세부 스탯이 더 많이 쌓인다.
    /// </summary>
    [Serializable]
    public class RankProjectileMultiplier
    {
        public StatRankType rank;
        [Range(0.5f, 25f)] public float multiplier;
    }

    [CreateAssetMenu(fileName = "ActivityStatConfig", menuName = "SO/Behavior/ActivityStatConfig")]
    public class ActivityStatConfigSO : ScriptableObject
    {
        public const int MAX_ROUND = 6;

        [Header("활동 타입")]
        public ManagementBtnType activityType;

        [Header("주력 스탯 (높은 확률)")]
        public List<StatWeight> primaryStats = new();

        [Header("서브 스탯 (낮은 확률)")]
        public List<StatWeight> secondaryStats = new();

        [Header("멤버 1인당 프로젝타일 수 (F등급 기준)")]
        [Range(1, 10)] public int minProjectilesPerMember = 5;
        [Range(1, 10)] public int maxProjectilesPerMember = 7; // 5명×회차당 평균 6발 = 총합 약 30/회차

        [Header("랭크별 프로젝타일 배수 (F=1.0 기준, 고랭크 폭발적 증가)")]
        public List<RankProjectileMultiplier> rankProjectileMultipliers = new()
        {
            new RankProjectileMultiplier { rank = StatRankType.F,    multiplier = 1.0f  },
            new RankProjectileMultiplier { rank = StatRankType.E,    multiplier = 1.1f  },
            new RankProjectileMultiplier { rank = StatRankType.EP,   multiplier = 1.2f  },
            new RankProjectileMultiplier { rank = StatRankType.D,    multiplier = 1.4f  },
            new RankProjectileMultiplier { rank = StatRankType.DP,   multiplier = 1.6f  },
            new RankProjectileMultiplier { rank = StatRankType.C,    multiplier = 2.0f  },
            new RankProjectileMultiplier { rank = StatRankType.CP,   multiplier = 2.5f  },
            new RankProjectileMultiplier { rank = StatRankType.B,    multiplier = 3.2f  },
            new RankProjectileMultiplier { rank = StatRankType.BP,   multiplier = 4.2f  },
            new RankProjectileMultiplier { rank = StatRankType.A,    multiplier = 5.5f  },
            new RankProjectileMultiplier { rank = StatRankType.AP,   multiplier = 7.0f  },
            new RankProjectileMultiplier { rank = StatRankType.S,    multiplier = 9.0f  },
            new RankProjectileMultiplier { rank = StatRankType.SP,   multiplier = 11.5f },
            new RankProjectileMultiplier { rank = StatRankType.SS,   multiplier = 14.5f },
            new RankProjectileMultiplier { rank = StatRankType.SSP,  multiplier = 18.0f },
            new RankProjectileMultiplier { rank = StatRankType.SSS,  multiplier = 22.0f },
            new RankProjectileMultiplier { rank = StatRankType.SSSP, multiplier = 27.0f },
            new RankProjectileMultiplier { rank = StatRankType.L,    multiplier = 35.0f },
        };

        [Header("회차별 프로젝타일 설정 (스킵 계산용, 최대 6회차)")]
        public List<RoundProjectileRange> roundRanges = new()
        {
            new RoundProjectileRange { round = 1, minProjectiles = 1, maxProjectiles = 2 },
            new RoundProjectileRange { round = 2, minProjectiles = 1, maxProjectiles = 2 },
            new RoundProjectileRange { round = 3, minProjectiles = 1, maxProjectiles = 3 },
            new RoundProjectileRange { round = 4, minProjectiles = 1, maxProjectiles = 3 },
            new RoundProjectileRange { round = 5, minProjectiles = 2, maxProjectiles = 4 },
            new RoundProjectileRange { round = 6, minProjectiles = 2, maxProjectiles = 4 },
        };

        [Header("랭크별 연속 회차 확률 (18등급)")]
        // F~DP: 간간히 2회차 가능, 3회차는 거의 불가 (decayPerRound 낮게 유지)
        public RankContinueChance F    = new() { baseChance = 0.32f, decayPerRound = 0.20f };
        public RankContinueChance E    = new() { baseChance = 0.35f, decayPerRound = 0.21f };
        public RankContinueChance EP   = new() { baseChance = 0.38f, decayPerRound = 0.22f };
        public RankContinueChance D    = new() { baseChance = 0.41f, decayPerRound = 0.23f };
        public RankContinueChance DP   = new() { baseChance = 0.44f, decayPerRound = 0.25f };
        // C~B: 성장감 인식 구간, 2회차 자주 3회차 가끔
        public RankContinueChance C    = new() { baseChance = 0.40f, decayPerRound = 0.28f };
        public RankContinueChance CP   = new() { baseChance = 0.46f, decayPerRound = 0.30f };
        public RankContinueChance B    = new() { baseChance = 0.53f, decayPerRound = 0.33f };
        public RankContinueChance BP   = new() { baseChance = 0.60f, decayPerRound = 0.36f };
        // A~S: 멀티회차 기본, 보상감 확실
        public RankContinueChance A    = new() { baseChance = 0.68f, decayPerRound = 0.40f };
        public RankContinueChance AP   = new() { baseChance = 0.74f, decayPerRound = 0.45f };
        public RankContinueChance S    = new() { baseChance = 0.80f, decayPerRound = 0.52f };
        public RankContinueChance SP   = new() { baseChance = 0.85f, decayPerRound = 0.58f };
        // SS~L: 6회차까지 가는 느낌, 압도적 쾌감
        public RankContinueChance SS   = new() { baseChance = 0.90f, decayPerRound = 0.65f };
        public RankContinueChance SSP  = new() { baseChance = 0.93f, decayPerRound = 0.70f };
        public RankContinueChance SSS  = new() { baseChance = 0.95f, decayPerRound = 0.75f };
        public RankContinueChance SSSP = new() { baseChance = 0.97f, decayPerRound = 0.82f };
        public RankContinueChance L    = new() { baseChance = 0.99f, decayPerRound = 0.90f };

        [Header("크리티컬")]
        [Tooltip("프로젝타일 발사 시 크리티컬 발동 베이스 확률 (저랭크 거의 안 터짐)")]
        [Range(0f, 1f)] public float critBaseChance = 0.02f; // 0.05 → 0.02
        [Tooltip("랭크별 크리티컬 추가 확률 (F≈2%, L≈37%)")]
        [Range(0f, 1f)] public float critRankBonusMax = 0.35f; // 0.20 → 0.35
        [Tooltip("크리티컬 시 프로젝타일 amount 배수")]
        [Range(1, 10)] public int critAmountMultiplier = 3; // 유지 (터질 때 확실하게)
        [Tooltip("회차당 크리티컬 1회당 멤버 스탯 보너스 비율 (예: 0.4 = 40%)")]
        [Range(0f, 1f)] public float critMemberStatBonusPerCrit = 0.4f; // 0.3 → 0.4

        [Header("작업 중 대사")]
        [TextArea(2, 5)]
        public List<string> workingMessages = new()
        {
            "이렇게 하면 되겠지",
            "좋은데?",
            "흠...",
            "집중 집중!",
            "완벽해!"
        };

        public string GetRandomWorkingMessage()
        {
            if (workingMessages == null || workingMessages.Count == 0)
                return "작업 중...";
            return workingMessages[UnityEngine.Random.Range(0, workingMessages.Count)];
        }

        /// <summary>
        /// 랭크 반영 멤버당 프로젝타일 수 반환.
        /// F등급 기준(min~max)에 랭크 배수를 곱해서 반올림.
        /// </summary>
        public int GetRandomProjectilesPerMember(StatRankType rank)
        {
            float mult = GetRankProjectileMultiplier(rank);
            int min = Mathf.Max(1, Mathf.RoundToInt(minProjectilesPerMember * mult));
            int max = Mathf.Max(min, Mathf.RoundToInt(maxProjectilesPerMember * mult));
            return UnityEngine.Random.Range(min, max + 1);
        }

        /// <summary>
        /// 하위 호환용. 랭크 없이 호출 시 F등급 기준으로 동작.
        /// </summary>
        public int GetRandomProjectilesPerMember()
            => GetRandomProjectilesPerMember(StatRankType.F);

        /// <summary>
        /// 해당 랭크의 프로젝타일 배수를 반환. 테이블에 없으면 1.0.
        /// </summary>
        public float GetRankProjectileMultiplier(StatRankType rank)
        {
            var entry = rankProjectileMultipliers.Find(r => r.rank == rank);
            return entry != null ? entry.multiplier : 1.0f;
        }

        public MusicRelatedStatsType GetRandomStat()
        {
            var allStats = new List<StatWeight>();
            allStats.AddRange(primaryStats);
            allStats.AddRange(secondaryStats);

            float totalWeight = 0f;
            foreach (var stat in allStats)
                totalWeight += stat.weight;

            float random     = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var stat in allStats)
            {
                cumulative += stat.weight;
                if (random <= cumulative)
                    return stat.statType;
            }

            return allStats[0].statType;
        }

        public (int min, int max) GetIncreaseRange(MusicRelatedStatsType statType)
        {
            foreach (var stat in primaryStats)
                if (stat.statType == statType)
                    return (stat.minIncrease, stat.maxIncrease);

            foreach (var stat in secondaryStats)
                if (stat.statType == statType)
                    return (stat.minIncrease, stat.maxIncrease);

            return (1, 5);
        }

        public (int min, int max) GetProjectileRangeForRound(int round)
        {
            int clampedRound = Mathf.Clamp(round, 1, MAX_ROUND);
            var roundRange   = roundRanges.Find(r => r.round == clampedRound);
            if (roundRange != null)
                return (roundRange.minProjectiles, roundRange.maxProjectiles);
            return (1, 3);
        }

        public float GetContinueChance(StatRankType avgRank, int currentRound)
        {
            if (currentRound >= MAX_ROUND) return 0f;
            var entry = GetRankEntry(avgRank);
            return entry.baseChance * Mathf.Pow(entry.decayPerRound, currentRound - 1);
        }

        /// <summary>
        /// 랭크에 따른 크리티컬 발동 확률 (베이스 + 랭크 보너스)
        /// F≈2%, C≈9%, S≈27%, L≈37%
        /// </summary>
        public float GetCritChance(StatRankType avgRank)
        {
            float rankT = GetRankNormalized(avgRank);
            return critBaseChance + critRankBonusMax * rankT;
        }

        private RankContinueChance GetRankEntry(StatRankType rank) => rank switch
        {
            StatRankType.L    => L,
            StatRankType.SSSP => SSSP,
            StatRankType.SSS  => SSS,
            StatRankType.SSP  => SSP,
            StatRankType.SS   => SS,
            StatRankType.SP   => SP,
            StatRankType.S    => S,
            StatRankType.AP   => AP,
            StatRankType.A    => A,
            StatRankType.BP   => BP,
            StatRankType.B    => B,
            StatRankType.CP   => CP,
            StatRankType.C    => C,
            StatRankType.DP   => DP,
            StatRankType.D    => D,
            StatRankType.EP   => EP,
            StatRankType.E    => E,
            _                 => F
        };

        private float GetRankNormalized(StatRankType rank) => rank switch
        {
            StatRankType.F    => 0f  / 17f,
            StatRankType.E    => 1f  / 17f,
            StatRankType.EP   => 2f  / 17f,
            StatRankType.D    => 3f  / 17f,
            StatRankType.DP   => 4f  / 17f,
            StatRankType.C    => 5f  / 17f,
            StatRankType.CP   => 6f  / 17f,
            StatRankType.B    => 7f  / 17f,
            StatRankType.BP   => 8f  / 17f,
            StatRankType.A    => 9f  / 17f,
            StatRankType.AP   => 10f / 17f,
            StatRankType.S    => 11f / 17f,
            StatRankType.SP   => 12f / 17f,
            StatRankType.SS   => 13f / 17f,
            StatRankType.SSP  => 14f / 17f,
            StatRankType.SSS  => 15f / 17f,
            StatRankType.SSSP => 16f / 17f,
            StatRankType.L    => 17f / 17f,
            _                 => 0f
        };
    }
}