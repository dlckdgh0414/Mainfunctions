using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Song
{
    [Serializable]
    public struct CompletedSongData
    {
        public string songName;
        public int songMadeYear;
        public int songLyricsValue;
        public int songTeamworkValue;
        public int songProficiencyValue;
        public int songMelodyValue;
        public int plusPenValue;
    }
    
    [CreateAssetMenu(fileName = "MadeSongList", menuName = "SO/Song/MadeSongList", order = 0)]
    public class CompletedSongListSO : ScriptableObject
    {
        public List<CompletedSongData> completedSongs = new List<CompletedSongData>();
    }
}