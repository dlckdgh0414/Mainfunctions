using Code.Core;
using Code.MainSystem.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.MusicProduction
{
    public class MusicGenresPrefabs : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI musicGenresNameText;
        [SerializeField] private TextMeshProUGUI musicGenresDifficultyText;
        [SerializeField] private Image leftBar;
        [SerializeField] private Image background;
        [SerializeField] private Image difficultyBackground;
        [field: SerializeField] public Button musicGenresButton;
        
        private static readonly Color SelectedBarColor = new Color(0.357f, 0.722f, 0.910f);
        private static readonly Color DefaultBarColor  = new Color(0.659f, 0.831f, 0.925f);
        private static readonly Color SelectedBgColor  = new Color(0.922f, 0.965f, 0.992f);
        private static readonly Color DefaultBgColor   = new Color(0.961f, 0.980f, 1.000f);

        private static readonly Color EasyBgColor     = new Color(0.831f, 0.941f, 0.878f);
        private static readonly Color EasyTextColor   = new Color(0.165f, 0.541f, 0.333f);
        private static readonly Color NormalBgColor   = new Color(0.996f, 0.941f, 0.831f);
        private static readonly Color NormalTextColor = new Color(0.690f, 0.439f, 0.125f);
        private static readonly Color HardBgColor     = new Color(0.992f, 0.878f, 0.878f);
        private static readonly Color HardTextColor   = new Color(0.753f, 0.251f, 0.251f);

        public void SetUpPrefab(string musicGenresName, string musicGenresDifficulty)
        {
            musicGenresNameText.SetText(musicGenresName);
            musicGenresDifficultyText.SetText($"{musicGenresDifficulty}");
        }
        
        public void SetDifficulty(MusicDifficultyType difficulty)
        {
            Color bgColor, textColor;
            switch (difficulty)
            {
                case MusicDifficultyType.Easy:
                    bgColor = EasyBgColor; textColor = EasyTextColor; break;
                case MusicDifficultyType.Hard:
                    bgColor = HardBgColor; textColor = HardTextColor; break;
                default:
                    bgColor = NormalBgColor; textColor = NormalTextColor; break;
            }
            if (difficultyBackground) difficultyBackground.color = bgColor;
            if (musicGenresDifficultyText) musicGenresDifficultyText.color = textColor;
        }

        public void SetSelected(bool selected)
        {
            if (leftBar) leftBar.color = selected ? SelectedBarColor : DefaultBarColor;
            if (background) background.color = selected ? SelectedBgColor : DefaultBgColor;
        }
    }
}