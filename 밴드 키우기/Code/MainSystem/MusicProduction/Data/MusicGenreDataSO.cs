using Code.Core;
using UnityEngine;

namespace Code.MainSystem.MusicProduction.Data
{
    [CreateAssetMenu(fileName = "Genre", menuName = "SO/Production/Genre", order = 0)]
    public class MusicGenreDataSO : ScriptableObject
    {
        public MusicGenreType genre;
        public MusicDifficultyType difficultyType;
        public string genreName;
        public string difficultyName;
    }
}