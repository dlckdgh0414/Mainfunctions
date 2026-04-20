using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.StatSystem.BaseStats;

namespace Code.SubSystem.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public string savedAtUtc;

        // 튜토리얼 / 플레이어 정보 
        public string bandName;
        public string userName;   
        public bool tutorialCompleted;

        // 재화 
        public int bandFunds = 2500;
        public int bandExp;
        public int bandFans;

        public int elapsedDays;

        // 음악 제작
        public bool hasMusicProduction;
        public MusicGenreType musicGenre;
        public MusicDirectionType musicDirection;
        public MusicDifficultyType musicDifficulty;
        public string musicGenreName;
        public string musicDirectionName;

        // 멤버 / 음악 스탯
        public List<MemberSaveEntry> memberStats = new List<MemberSaveEntry>();
        public List<MusicStatEntry> musicStats = new List<MusicStatEntry>();
        public int musicPerfectionPercent;

        public List<MemberConditionEntry> memberConditions = new List<MemberConditionEntry>();

        public void SetMemberStat(MemberType type, int composition, StatRankType compRank,
                                                    int instrument, StatRankType instRank)
        {
            var existing = memberStats.Find(e => e.memberType == type);
            if (existing == null)
            {
                existing = new MemberSaveEntry { memberType = type };
                memberStats.Add(existing);
            }
            existing.composition = composition;
            existing.compositionRank = compRank;
            existing.instrumentProficiency = instrument;
            existing.instrumentRank = instRank;
        }

        public MemberSaveEntry GetMemberStat(MemberType type)
            => memberStats.Find(e => e.memberType == type);

        public void SetMusicStat(MusicRelatedStatsType type, int value)
        {
            var existing = musicStats.Find(e => e.statType == type);
            if (existing != null) existing.value = value;
            else musicStats.Add(new MusicStatEntry { statType = type, value = value });
        }

        public int GetMusicStat(MusicRelatedStatsType type)
        {
            var entry = musicStats.Find(e => e.statType == type);
            return entry?.value ?? 0;
        }

        public void SetCondition(MemberType type, int condition)
        {
            var existing = memberConditions.Find(e => e.memberType == type);
            if (existing != null) existing.condition = condition;
            else memberConditions.Add(new MemberConditionEntry { memberType = type, condition = condition });
        }

        public int GetCondition(MemberType type)
        {
            var entry = memberConditions.Find(e => e.memberType == type);
            return entry?.condition ?? -1;
        }
    }

    [Serializable]
    public class MemberSaveEntry
    {
        public MemberType memberType;
        public int composition;
        public StatRankType compositionRank;
        public int instrumentProficiency;
        public StatRankType instrumentRank;
    }

    [Serializable]
    public class MusicStatEntry
    {
        public MusicRelatedStatsType statType;
        public int value;
    }

    [Serializable]
    public class MemberConditionEntry
    {
        public MemberType memberType;
        public int condition;
    }
}