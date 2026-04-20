using System;
using Code.Core;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.MusicRelated
{
    public class MusicSocreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI musicSocreText;
        [SerializeField] private MusicRelatedStatsType musicSocreType;

        private void Start()
        {
            if (GameStatManager.Instance != null)
            {
                GameStatManager.Instance.OnStatsChanged += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (GameStatManager.Instance != null)
            {
                GameStatManager.Instance.OnStatsChanged -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            if (GameStatManager.Instance != null)
            {
                musicSocreText.SetText($"{GameStatManager.Instance.GetScore(musicSocreType)}");
            }
        }

        public void Refresh()
        {
            UpdateUI();
        }
    }
}