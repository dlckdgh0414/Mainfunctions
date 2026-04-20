using Code.MainSystem.Song;
using UnityEngine;

namespace Code.Core.Bus.GameEvents.SongEvents
{
    public struct CompletedSongEvent : IEvent
    {
        public CompletedSongData Data { get; private set; }

        public CompletedSongEvent(CompletedSongData data)
        {
            Data = data;
        }
    }
}