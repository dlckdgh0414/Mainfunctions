using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.SubSystem.BandFunds;
using System;
using Code.MainSystem.MusicProduction;
using UnityEngine;

namespace Code.SubSystem.Save
{
    [DefaultExecutionOrder(-200)]
    public class SaveBinder : MonoBehaviour
    {
        private bool _bound;

        private void Start() => Bind();

        private void Bind()
        {
            if (_bound) return;
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[SaveBinder] SaveManager 가 없음. Script Execution Order 확인.");
                return;
            }
            
            var music = MusicProductionManager.Instance;
            if (music != null)
                music.OnMusicDataChanged += SyncMusicProduction;

            var supply = BandSupplyManager.Instance;
            if (supply != null)
            {
                supply.OnFundsChanged += SyncSupply;
                supply.OnExpChanged   += SyncSupply;
                supply.OnFansChanged  += SyncSupply;
            }

            var turn = TurnManager.Instance;
            if (turn != null)
                turn.OnDateChanged += OnDateChanged;

            var stat = GameStatManager.Instance;
            if (stat != null)
            {
                stat.OnStatsChanged += SyncStats;
                // 음악 완성도 변경 이벤트 구독 (100% 달성 시 저장 보장)
                stat.OnMusicPercentChanged += SyncMusicPercent;
            }

            _bound = true;
            Debug.Log("[SaveBinder] 이벤트 구독 완료");
        }

        private void OnDestroy()
        {
            var supply = BandSupplyManager.Instance;
            if (supply != null)
            {
                supply.OnFundsChanged -= SyncSupply;
                supply.OnExpChanged   -= SyncSupply;
                supply.OnFansChanged  -= SyncSupply;
            }
            
            var music = MusicProductionManager.Instance;
            if (music != null)
                music.OnMusicDataChanged -= SyncMusicProduction;

            var turn = TurnManager.Instance;
            if (turn != null)
                turn.OnDateChanged -= OnDateChanged;

            var stat = GameStatManager.Instance;
            if (stat != null)
            {
                stat.OnStatsChanged -= SyncStats;
                stat.OnMusicPercentChanged -= SyncMusicPercent;
            }
        }
        
        private void SyncMusicProduction()
        {
            var data = SaveManager.Instance.Data;
            var m = MusicProductionManager.Instance;
            data.hasMusicProduction = m.HasMusicData;
            if (m.HasMusicData)
            {
                data.musicGenre     = m.GetCurrentMusicGenreType();
                data.musicDirection = m.GetCurrentMusicDirectionType();
                data.musicDifficulty = m.GetCurrentMusicDifficultyType();
                // 이름 필드도 함께 동기화
                data.musicGenreName     = m.GetCurrentMusicGenreName();
                data.musicDirectionName = m.GetCurrentMusicDirectionName();
            }
            Debug.Log($"[SaveBinder] 음악 sync: hasMusic={data.hasMusicProduction}");
            SaveManager.Instance.MarkDirty();
        }

        private void SyncSupply()
        {
            var data = SaveManager.Instance.Data;
            var s = BandSupplyManager.Instance;
            data.bandFunds = s.BandFunds;
            data.bandExp   = s.BandExp;
            data.bandFans  = s.BandFans;
            SaveManager.Instance.MarkDirty();
        }

        private void OnDateChanged(int year, int month, int day)
        {
            SaveManager.Instance.Data.elapsedDays = TurnManager.Instance.TotalDays;
            SaveManager.Instance.MarkDirty();
        }

        private void SyncStats()
        {
            var data = SaveManager.Instance.Data;
            var stat = GameStatManager.Instance;

            foreach (MemberType type in Enum.GetValues(typeof(MemberType)))
            {
                var comp = stat.GetMemberStatData(type, MusicRelatedStatsType.Composition);
                var inst = stat.GetMemberStatData(type, MusicRelatedStatsType.InstrumentProficiency);
                data.SetMemberStat(type, comp.currentValue, comp.currentRank,
                                          inst.currentValue, inst.currentRank);
            }

            data.SetMusicStat(MusicRelatedStatsType.Lyrics,      stat.GetScore(MusicRelatedStatsType.Lyrics));
            data.SetMusicStat(MusicRelatedStatsType.Teamwork,    stat.GetScore(MusicRelatedStatsType.Teamwork));
            data.SetMusicStat(MusicRelatedStatsType.Proficiency, stat.GetScore(MusicRelatedStatsType.Proficiency));
            data.SetMusicStat(MusicRelatedStatsType.Melody,      stat.GetScore(MusicRelatedStatsType.Melody));
            data.musicPerfectionPercent = stat.GetMusicPerfectionPercent();

            SaveManager.Instance.MarkDirty();
        }

        /// <summary>
        /// 음악 완성도 변경 시 호출. 100% 달성 순간 즉시 강제 저장하여
        /// 앱 종료/재시작 후에도 업로드 버튼이 복원되게 한다.
        /// </summary>
        private void SyncMusicPercent(int percent)
        {
            var data = SaveManager.Instance.Data;
            data.musicPerfectionPercent = percent;

            if (percent >= 100)
            {
                Debug.Log("[SaveBinder] 음악 완성도 100% 달성 → 즉시 강제 저장");
                SaveManager.Instance.ForceSaveNow();
            }
            else
            {
                SaveManager.Instance.MarkDirty();
            }
        }
    }
}