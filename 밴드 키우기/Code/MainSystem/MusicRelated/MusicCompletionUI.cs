using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.MusicRelated
{
    public class MusicCompletionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private Image gaugeImage;

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
                int percent = GameStatManager.Instance.GetMusicPerfectionPercent();
                percentText.SetText($"{percent}%");
                gaugeImage.fillAmount = percent / 100f;
            }
        }

        public void Refresh()
        {
            UpdateUI();
        }
    }
}