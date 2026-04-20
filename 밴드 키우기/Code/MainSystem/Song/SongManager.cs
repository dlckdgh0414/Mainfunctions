using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SongEvents;
using UnityEngine;

namespace Code.MainSystem.Song
{
    public class SongManager : MonoBehaviour
    {
        [SerializeField] private CompletedSongListSO completedSongList;
        
        private void Awake()
        {
            Bus<CompletedSongEvent>.OnEvent += HandleSongCompleted;
        }

        private void OnDestroy()
        {
            Bus<CompletedSongEvent>.OnEvent -= HandleSongCompleted;
        }

        private void HandleSongCompleted(CompletedSongEvent evt)
        {
            completedSongList.completedSongs.Add(evt.Data);
        }
    }
}