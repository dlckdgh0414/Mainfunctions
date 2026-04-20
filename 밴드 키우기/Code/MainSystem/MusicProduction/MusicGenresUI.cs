using System;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.MainSystem.MusicProduction.Data;
using Code.MainSystem.Sound;
using UnityEngine;

namespace Code.MainSystem.MusicProduction
{
    public class MusicGenresUI : MonoBehaviour
    {
        [SerializeField] private List<MusicGenreDataSO> musicGenres = new List<MusicGenreDataSO>();
        [SerializeField] private Transform prefabSpawnTrm;
        [SerializeField] private MusicGenresPrefabs musicGenresPrefabs;
        
        [Header("Sound")]
        [SerializeField] private SoundSO clickSound;
        
        private readonly List<MusicGenresPrefabs> _spawnedPrefabs = new();
        private MusicGenresPrefabs _selectedPrefab;

        public Action<MusicGenreType, MusicDifficultyType, string> OnMusicGenresSelected;
        public Action OnHide;
        private void Start()
        {
            foreach (var genre in musicGenres)
            {
                if (genre == null) continue;
                var prefab = Instantiate(musicGenresPrefabs, prefabSpawnTrm);

                string displayGenre = string.IsNullOrEmpty(genre.genreName) ? genre.genre.ToString() : genre.genreName;
                string displayDifficulty = string.IsNullOrEmpty(genre.difficultyName) ? genre.difficultyType.ToString() : genre.difficultyName;

                prefab.SetUpPrefab(displayGenre, displayDifficulty);
                prefab.SetDifficulty(genre.difficultyType);
                prefab.SetSelected(false);
                prefab.musicGenresButton.onClick.AddListener(() => HandleClickUI(genre, prefab));
                _spawnedPrefabs.Add(prefab);
            }
        }

        private void HandleClickUI(MusicGenreDataSO genre, MusicGenresPrefabs prefab)
        {
            if (_selectedPrefab != null)
                _selectedPrefab.SetSelected(false);

            _selectedPrefab = prefab;
            prefab.SetSelected(true);
            
            Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(clickSound));
            OnMusicGenresSelected?.Invoke(genre.genre, genre.difficultyType, genre.genreName);
            Hide();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            foreach (var prefab in _spawnedPrefabs)
                prefab?.musicGenresButton?.onClick.RemoveAllListeners();
        }
    }
}