using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.SubSystem.BandFunds;
using Code.MainSystem.Song;
using UnityEngine;

namespace Code.SubSystem.Award
{
    public enum AwardType
    {
        None,
        CompositionAward,
        LyricsAward,
        SubscriberGrowthAward,
        WorstSongAward,
        RookieBandAward,
        BandOfTheYearAward,
        BestMusicAward
    }

    [Serializable]
    public class AwardResult
    {
        public AwardType awardType;
        public bool isPlayerWinner;
        public string npcWinnerName;
        public string npcSongTitle;
    }

    [Serializable]
    public class AwardConfig
    {
        public AwardType awardType;
        public string awardName;
        [TextArea] public string awardMent;
        public int prizeMoney;
    }

    [CreateAssetMenu(fileName = "AwardSystemSO", menuName = "SO/Award/AwardSystemSO")]
    public class AwardSystemSO : ScriptableObject
    {
        [Header("데이터 참조")]
        [SerializeField] private CompletedSongListSO completedSongList;

        [Header("상위상 기준 (곡 총점)")]
        [SerializeField] private int bestMusicThreshold  = 400;
        [SerializeField] private int bandOfYearThreshold = 250;

        [Header("하위상 기준")]
        [SerializeField] private int compositionThreshold      = 100; // 멤버 평균 작곡 스탯
        [SerializeField] private int lyricsThreshold           = 100; // 곡의 가사 값
        [SerializeField] private int worstSongMaxThreshold     = 80;  // 곡 총점이 이 이하면 망한곡상
        [SerializeField] private int subscriberGrowthThreshold = 2000;
        [SerializeField] private int rookieFanThreshold        = 500;

        [Header("신인상 기준")]
        [SerializeField] private int rookieYearLimit = 3;

        [Header("상 설정 (이름/멘트/상금)")]
        [SerializeField] private List<AwardConfig> awardConfigs = new List<AwardConfig>
        {
            new AwardConfig { awardType = AwardType.CompositionAward,      awardName = "작곡상",          awardMent = "뛰어난 작곡 실력을 인정합니다.",               prizeMoney = 500  },
            new AwardConfig { awardType = AwardType.LyricsAward,           awardName = "작사상",          awardMent = "감동적인 가사로 많은 이들의 마음을 움직였습니다.", prizeMoney = 500  },
            new AwardConfig { awardType = AwardType.SubscriberGrowthAward, awardName = "팬성장상",        awardMent = "폭발적인 팬덤 성장을 이루었습니다.",             prizeMoney = 300  },
            new AwardConfig { awardType = AwardType.WorstSongAward,        awardName = "올해의 망한곡상", awardMent = "...수고했습니다.",                              prizeMoney = 50   },
            new AwardConfig { awardType = AwardType.RookieBandAward,       awardName = "신인밴드상",      awardMent = "빛나는 신인의 등장을 축하합니다.",              prizeMoney = 300  },
            new AwardConfig { awardType = AwardType.BandOfTheYearAward,    awardName = "올해의밴드상",    awardMent = "올 한 해 최고의 밴드로 선정되었습니다.",        prizeMoney = 1000 },
            new AwardConfig { awardType = AwardType.BestMusicAward,        awardName = "최고의음악상",    awardMent = "음악계 최고의 영예를 안았습니다.",              prizeMoney = 3000 },
        };

        [Header("밴드 이름 단어 풀")]
        [SerializeField] private List<string> bandPrefixes = new List<string> { "블랙", "레드", "블루", "골든", "네온", "다크", "루나", "노바", "스틸", "소닉" };
        [SerializeField] private List<string> bandSuffixes = new List<string> { "웨이브", "스타", "나이트", "드림", "러시", "홀릭", "팩토리", "바이브", "코드" };

        [Header("곡 이름 단어 풀")]
        [SerializeField] private List<string> songAdjectives = new List<string> { "불타는", "새벽의", "영원한", "찬란한", "빛나는", "차가운", "고요한", "비밀의" };
        [SerializeField] private List<string> songNouns      = new List<string> { "심장", "별빛", "파도", "꿈", "그림자", "기억", "시간", "바람", "목소리" };
        [SerializeField] private List<string> songSuffixes   = new List<string> { "", " (Remix)", " Ver.", " Pt.2" };

        private readonly HashSet<string> _usedBandNames  = new HashSet<string>();
        private readonly HashSet<string> _usedSongTitles = new HashSet<string>();

        /// <summary>
        /// 수상에 사용된 플레이어의 작년 곡. UI에서 참조 가능.
        /// 없으면 null.
        /// </summary>
        public CompletedSongData? PlayerAwardedSong { get; private set; }

        public List<AwardResult> EvaluateAwards()
        {
            var results = new List<AwardResult>();
            var gsm     = GameStatManager.Instance;
            var tm      = TurnManager.Instance;
            var bsm     = BandSupplyManager.Instance;

            PlayerAwardedSong = null;

            if (tm == null) return results;

            int currentYear = tm.CurrentYear;
            int startYear   = tm.StartYear;
            int lastYear    = currentYear - 1;
            int currentFans = bsm != null ? bsm.BandFans : 0;
            bool isRookie   = (currentYear - startYear) <= rookieYearLimit;

            // === 작년에 만든 곡 중 가장 마지막(최신) 곡 찾기 ===
            CompletedSongData? lastYearSong = FindLastSongOfYear(lastYear);

            AwardType playerAward = AwardType.None;

            if (lastYearSong.HasValue)
            {
                var song = lastYearSong.Value;
                PlayerAwardedSong = song;

                // 작년 곡의 실제 스탯을 기반으로 수상 판단
                int songLyrics      = song.songLyricsValue;
                int songTeamwork    = song.songTeamworkValue;
                int songProficiency = song.songProficiencyValue;
                int songMelody      = song.songMelodyValue;
                int songTotal       = songLyrics + songTeamwork + songProficiency + songMelody;

                // 작곡 스탯은 곡 자체에 없으니 멤버 평균을 씀 (기존 방식 유지)
                int compositionScore = gsm != null ? GetAverageComposition(gsm) : 0;

                if (songTotal >= bestMusicThreshold)
                    playerAward = AwardType.BestMusicAward;
                else if (songTotal >= bandOfYearThreshold)
                    playerAward = AwardType.BandOfTheYearAward;
                else
                    playerAward = EvaluateLowerAward(
                        compositionScore,
                        songLyrics,
                        songTotal,
                        currentFans,
                        isRookie
                    );
            }

            // 플레이어 결과 추가 (None이어도 리스트엔 포함)
            results.Add(new AwardResult { awardType = playerAward, isPlayerWinner = true });

            if (playerAward != AwardType.None)
                GrantPrizeMoney(playerAward);

            // 나머지 상 NPC 배정
            foreach (AwardType award in Enum.GetValues(typeof(AwardType)))
            {
                if (award == AwardType.None || award == playerAward) continue;
                if (award == AwardType.RookieBandAward && !isRookie) continue;

                results.Add(new AwardResult
                {
                    awardType      = award,
                    isPlayerWinner = false,
                    npcWinnerName  = GenerateBandName(),
                    npcSongTitle   = GenerateSongTitle()
                });
            }

            return results;
        }

        /// <summary>
        /// 특정 연도에 만들어진 곡 중 리스트에서 가장 마지막(최신) 곡을 반환.
        /// 없으면 null.
        /// </summary>
        private CompletedSongData? FindLastSongOfYear(int year)
        {
            if (completedSongList == null) return null;
            var songs = completedSongList.completedSongs;
            if (songs == null || songs.Count == 0) return null;

            for (int i = songs.Count - 1; i >= 0; i--)
            {
                if (songs[i].songMadeYear == year)
                    return songs[i];
            }
            return null;
        }

        private string GenerateBandName()
        {
            for (int i = 0; i < 50; i++)
            {
                string prefix = bandPrefixes[UnityEngine.Random.Range(0, bandPrefixes.Count)];
                string suffix = bandSuffixes[UnityEngine.Random.Range(0, bandSuffixes.Count)];
                string name   = $"{prefix} {suffix}";
                if (_usedBandNames.Add(name)) return name;
            }
            return $"밴드{UnityEngine.Random.Range(100, 999)}";
        }

        private string GenerateSongTitle()
        {
            for (int i = 0; i < 50; i++)
            {
                string adj    = songAdjectives[UnityEngine.Random.Range(0, songAdjectives.Count)];
                string noun   = songNouns[UnityEngine.Random.Range(0, songNouns.Count)];
                string suffix = songSuffixes[UnityEngine.Random.Range(0, songSuffixes.Count)];
                string title  = $"{adj} {noun}{suffix}";
                if (_usedSongTitles.Add(title)) return title;
            }
            return $"Unknown Track {UnityEngine.Random.Range(100, 999)}";
        }

        private AwardType EvaluateLowerAward(int comp, int lyrics, int total, int fans, bool isRookie)
        {
            int currentYear = TurnManager.Instance.CurrentYear - TurnManager.Instance.StartYear;
    
            // 항목별로 다른 증가율 적용
            int fanAdder = Mathf.Max(1, (int)Mathf.Pow(currentYear, 1.5f));
            int statAdder = Mathf.Max(1, (int)Mathf.Pow(currentYear, 1.2f)); 
            int worstAdder = Mathf.Max(1, currentYear);
            
            if (fans   >= subscriberGrowthThreshold * fanAdder) return AwardType.SubscriberGrowthAward;
            if (comp   >= compositionThreshold * statAdder)     return AwardType.CompositionAward;
            if (lyrics >= lyricsThreshold * statAdder)           return AwardType.LyricsAward;
            if (isRookie && fans >= rookieFanThreshold) return AwardType.RookieBandAward;
            if (total  <= worstSongMaxThreshold * worstAdder)     return AwardType.WorstSongAward;
            return AwardType.None;
        }

        private void GrantPrizeMoney(AwardType awardType)
        {
            var config = awardConfigs.Find(c => c.awardType == awardType);
            if (config == null) return;
            BandSupplyManager.Instance?.AddBandFunds(config.prizeMoney);
        }

        public AwardConfig GetConfig(AwardType awardType) => awardConfigs.Find(c => c.awardType == awardType);

        private int GetAverageComposition(GameStatManager gsm)
        {
            int total = 0, count = 0;
            foreach (MemberType memberType in Enum.GetValues(typeof(MemberType)))
            {
                var data = gsm.GetMemberStatData(memberType, MusicRelatedStatsType.Composition);
                if (data != null) { total += data.currentValue; count++; }
            }
            return count > 0 ? total / count : 0;
        }
    }
}