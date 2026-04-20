using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicProduction.Data;
using UnityEngine;

namespace Code.MainSystem.MusicProduction
{
    public class MusicProductionManager : MonoBehaviour
    {
        [SerializeField] private List<MusicProductionDataSO> musicProductionDataSOs;
        private MusicProductionDataSO _currentMusicProductionDataSO = null;
        private MusicGenreType _currentMusicGenreType;
        private MusicDirectionType _currentMusicDirectionType;
        private MusicDifficultyType _currentMusicDifficultyType;
        private string _currentMusicGenreName;
        private string _currentMusicDirectionName;

        public static MusicProductionManager Instance;
        public bool HasMusicData => _currentMusicProductionDataSO != null;
        private MusicProductionUnionType _currentMusicProductionUnionType;

        public event System.Action OnMusicDataChanged;

        private void Awake()
        {
            Debug.Log($"[MPM] Awake. 기존Instance={(Instance==null?"null":Instance.GetInstanceID().ToString())}, 내ID={GetInstanceID()}, 씬={gameObject.scene.name}");

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                TryRestoreFromSave();
            }
            else
            {
                Instance.TryRestoreFromSave();
                Destroy(gameObject);
            }
        }

        private void TryRestoreFromSave()
        {
            if (HasMusicData) return;

            var save = Code.SubSystem.Save.SaveManager.Instance;
            Debug.Log($"[MPM] 복원시도. save={save!=null}, hasSave={save?.HasSave}, hasMusic={save?.Data?.hasMusicProduction}");

            if (save != null && save.HasSave && save.Data.hasMusicProduction)
            {
                RestoreWithoutSave(
                    save.Data.musicGenre,
                    save.Data.musicDirection,
                    save.Data.musicDifficulty,
                    save.Data.musicGenreName,
                    save.Data.musicDirectionName);
                Debug.Log($"[MPM] 복원완료: {_currentMusicGenreType}/{_currentMusicDirectionType} | 이름: {_currentMusicGenreName}/{_currentMusicDirectionName}");
            }
        }

        private void RestoreWithoutSave(MusicGenreType genre, MusicDirectionType direction,
            MusicDifficultyType difficulty, string genreName = null, string directionName = null)
        {
            _currentMusicGenreType = genre;
            _currentMusicDirectionType = direction;
            _currentMusicDifficultyType = difficulty;
            _currentMusicGenreName     = string.IsNullOrEmpty(genreName)     ? genre.ToString()     : genreName;
            _currentMusicDirectionName = string.IsNullOrEmpty(directionName) ? direction.ToString() : directionName;

            foreach (var musicProductionDataSO in musicProductionDataSOs)
            {
                if (musicProductionDataSO.genre == genre && musicProductionDataSO.direction == direction)
                {
                    _currentMusicProductionDataSO = musicProductionDataSO;
                    _currentMusicProductionUnionType = musicProductionDataSO.unionType;
                }
            }
            OnMusicDataChanged?.Invoke();
        }

        public void Setup(MusicGenreType genre, MusicDirectionType direction,
            MusicDifficultyType difficulty, string genreName = null, string directionName = null)
        {
            _currentMusicGenreType     = genre;
            _currentMusicDirectionType = direction;
            _currentMusicDifficultyType = difficulty;
            _currentMusicGenreName     = string.IsNullOrEmpty(genreName)     ? genre.ToString()     : genreName;
            _currentMusicDirectionName = string.IsNullOrEmpty(directionName) ? direction.ToString() : directionName;

            foreach (var musicProductionDataSO in musicProductionDataSOs)
            {
                if (musicProductionDataSO.genre == genre && musicProductionDataSO.direction == direction)
                {
                    _currentMusicProductionDataSO = musicProductionDataSO;
                    _currentMusicProductionUnionType = musicProductionDataSO.unionType;
                }
            }
            
            var save = Code.SubSystem.Save.SaveManager.Instance;
            if (save != null)
            {
                save.Data.hasMusicProduction = true;
                save.Data.musicGenre         = genre;
                save.Data.musicDirection     = direction;
                save.Data.musicDifficulty    = difficulty;
                save.Data.musicGenreName     = _currentMusicGenreName;
                save.Data.musicDirectionName = _currentMusicDirectionName;
                save.ForceSaveNow();
                Debug.Log($"[MPM] Setup 강제저장: {genre}/{direction}/{difficulty} | 이름: {_currentMusicGenreName}/{_currentMusicDirectionName}");
            }

            OnMusicDataChanged?.Invoke();
        }

        public void ResetMusicData()
        {
            _currentMusicProductionDataSO = null;
            _currentMusicGenreName        = string.Empty;
            _currentMusicDirectionName    = string.Empty;

            var save = Code.SubSystem.Save.SaveManager.Instance;
            if (save != null)
            {
                save.Data.hasMusicProduction = false;
                save.Data.musicGenreName     = string.Empty;
                save.Data.musicDirectionName = string.Empty;
                save.ForceSaveNow();
                Debug.Log("[MPM] Reset 강제저장");
            }

            OnMusicDataChanged?.Invoke();
        }

        public MusicProductionDataSO GetData(MusicGenreType genre, MusicDirectionType direction)
        {
            foreach (var data in musicProductionDataSOs)
            {
                if (data.genre == genre && data.direction == direction) return data;
            }
            return null;
        }

        public MusicGenreType GetCurrentMusicGenreType()           => _currentMusicGenreType;
        public MusicDirectionType GetCurrentMusicDirectionType()   => _currentMusicDirectionType;
        public MusicDifficultyType GetCurrentMusicDifficultyType() => _currentMusicDifficultyType;
        public string GetCurrentMusicGenreName()                   => _currentMusicGenreName;
        public string GetCurrentMusicDirectionName()               => _currentMusicDirectionName;
        public MusicProductionUnionType GetCurrentMusicProductionUnionType() => _currentMusicProductionUnionType;
    }
}