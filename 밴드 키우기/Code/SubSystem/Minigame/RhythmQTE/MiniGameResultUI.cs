using System;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.Song;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.Minigame.Common.Management;
using Code.SubSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    public class MiniGameResultUI : MonoBehaviour
    {
        [Header("SO")]
        [SerializeField] protected MemberThrowDataSO memberThrowSO;
        [SerializeField] private MiniGameResultSenderSO senderSO;

        [Header("MusicStat")]
        [SerializeField] private MusicStatBar musicStatBar;

        [Header("MemberStat")]
        [SerializeField] private PersonalStatBarContainer personalStatBar;
        
        [Header("UI")]
        [SerializeField] private Button nextBtn;
        [SerializeField] private TextMeshProUGUI nextBtnText;
        
        private int currentTeamStat;
        private int currentProficiencyStat;

        private void OnEnable()
        {
            nextBtn.onClick.AddListener(GoToMain);
            if (memberThrowSO.HasPendingSchedule)
            {
                nextBtnText.SetText("다음일정으로");
            }
            else
            {
                nextBtnText.SetText("돌아가기");
            }
        }

        public async void OpenResultUI()
        {
            gameObject.SetActive(true);

            await musicStatBar.PlayUIAnimation();
            
            await personalStatBar.PlayUIAnimation();
        }
        
        private void GoToMain()
        {
            var next = memberThrowSO.DequeueNextSchedule();
    
            if (next != null)
            {
                memberThrowSO.PrepareCurrentMembersForExecution(next.Value);
                string sceneName = GetSceneName(next.Value);
                Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(sceneName));
            }
            else
            {
                Bus<FadeSceneEvent>.Raise(new FadeSceneEvent("Main"));
            }
        }

        private string GetSceneName(ManagementBtnType type)
        {
            return type switch
            {
                ManagementBtnType.Concert => "Rhythm",
                ManagementBtnType.Song => "SongMinigame", 
                _ => "Main"
            };
        }

        private void OnDisable()
        {
            nextBtn.onClick.RemoveAllListeners();
        }
    }
}