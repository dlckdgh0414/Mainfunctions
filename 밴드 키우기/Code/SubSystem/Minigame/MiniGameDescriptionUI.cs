using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.MiniGameEvent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Code.SubSystem.Minigame
{
    public class MiniGameDescriptionUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MiniGameDescriptionDataSO minigameData;
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VideoClip videoClip;
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button startBtn;
        [SerializeField] private GameObject BG;

        private void Start()
        {
            videoPlayer.clip = videoClip;
            videoPlayer.Play();
            startBtn.onClick.AddListener(HandheldStartMiniGame);
            nameText.SetText(minigameData.name);
            descriptionText.SetText(minigameData.description);
            BG.SetActive(true);
        }

        private void HandheldStartMiniGame()
        {
            BG.SetActive(false);
            Bus<MiniGameStartCountingEvent>.Raise(new MiniGameStartCountingEvent(3));
        }

        private void OnDisable()
        {
            videoPlayer.Stop();
            startBtn.onClick.AddListener(HandheldStartMiniGame);
        }
    }
}