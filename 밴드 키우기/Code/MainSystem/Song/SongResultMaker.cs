using System;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SongEvents;
using Code.MainSystem.MusicProduction;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.Tree.Addon;
using Code.SubSystem.BandFunds;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.MainSystem.Song
{
    public struct MusicReleaseResultData
    {
        public float TotalScore;         // 총점(/ 8한 값)
        public List<float> AverageStars; // 각각 점수
        public int PlayCount;            // 재생 수
        public int EarnedMoney;          // 벌어들인 돈
        public int NewFans;              // 유입된 팬 수
        public int GetExp;               // 얻은 경험치
    }
    
    public class SongResultMaker : BaseSongAddon
    {
        [Header("Settings")]
        [SerializeField] private int industryStandardBase = 50; // 초반 기준치
        [SerializeField] private float industryGrowthRate = 1.2f; // 곡을 낼 때마다 기준치 상승
        
        private int _releasedSongCount = 0; // 출시한 곡 수 (누적)

        private MarketingQuality _songMVBoost = 0;
        private MarketingQuality _songThumbnailBoost = 0;
        
        private CompletedSongData _songData;
        private SongResultCalculator _songResultCalculator;
        
        
        private void Awake()
        {
            _songResultCalculator = new SongResultCalculator();
            Bus<SongUploadOptionEvent>.OnEvent += HandleSongOptionGet;
            EventHandle();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void HandleSongOptionGet(SongUploadOptionEvent evt)
        {
            _songMVBoost = evt.SongMVBoost;
            _songThumbnailBoost = evt.SongThumbnailBoost;
            
            _songData.songName = evt.SongName;
        }

        public MusicReleaseResultData CalculateResult()
        {
            ApplyAllUpgrades();
            var statManager = GameStatManager.Instance;
            var stats = statManager.GetAllStats(); // Lyrics, Teamwork, Proficiency, Melody
            
            _songData.songLyricsValue = stats[MusicRelatedStatsType.Lyrics];
            _songData.songTeamworkValue = stats[MusicRelatedStatsType.Teamwork];
            _songData.songProficiencyValue = stats[MusicRelatedStatsType.Proficiency];
            _songData.songMelodyValue = stats[MusicRelatedStatsType.Melody];
            _songData.songMadeYear = TurnManager.Instance.CurrentYear;
            
            _releasedSongCount++;
            
            Bus<CompletedSongEvent>.Raise(new CompletedSongEvent(_songData));
            
            ResetUpgradeValue();

            MusicProductionUnionType type = MusicProductionManager.Instance.GetCurrentMusicProductionUnionType();

            float mixture = type switch
            {
                MusicProductionUnionType.Bad => 0.8f,
                MusicProductionUnionType.Good => 1.2f,
                _ => 1f
            };

            return _songResultCalculator.GetResultData(
                stats[MusicRelatedStatsType.Lyrics],
                stats[MusicRelatedStatsType.Melody],
                stats[MusicRelatedStatsType.Teamwork],
                stats[MusicRelatedStatsType.Proficiency],
                BandSupplyManager.Instance.BandFans,
                _releasedSongCount, TurnManager.Instance.CurrentYear - TurnManager.Instance.StartYear,
                mixture,
                _songThumbnailBoost, _songMVBoost);
        }
    }
}