using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.MiniGameEvent.QTEEvents;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.MainSystem.RhythmQTE;
using Code.MainSystem.Song;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.Minigame.Common.Contexts;
using Code.SubSystem.Minigame.Common.Management;
using Code.Tool;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    // 곡 데이터(BPM, 곡 로딩시킬 이름) 받아서 처리후 QTE 시작시킴
    // 메인 씬에서 QTEStartEvent를 보내면 이게 QTE 씬으로 전환시킴
    public class QTEStarter : MonoBehaviour
    {
        // 이 안에 bpm 이름 스텟 들어가있음
        // [SerializeField] private CompletedSongDataSO defaultSongDataSO;
        [SerializeField] private string miniGameSceneName = "Rhythm";
        
        private void Awake()
        {
            Bus<QTEStartEvent>.OnEvent += HandleQTEStart;
        }

        private void HandleQTEStart(QTEStartEvent evt)
        {
            // defaultSongDataSO = SongManager.Instance.CurrentSong;
            Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(miniGameSceneName));
        }

        public void StartQTE()
        {
            Bus<QTEStartEvent>.Raise(new QTEStartEvent());
        }
    }
}