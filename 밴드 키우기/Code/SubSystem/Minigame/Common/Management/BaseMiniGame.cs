using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.MiniGameEvent;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.Minigame.Common.Contexts;
using Code.SubSystem.Minigame.Common.Interface;
using Code.SubSystem.Minigame.RhythmQTE;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SubSystem.Minigame.Common.Management
{
    public abstract class BaseMiniGame : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] private MiniGameResultUI resultUI;
        [SerializeField] protected MemberThrowDataSO memberThrowSO;
        [SerializeField] protected StatType needStatType; // 이 미니게임에 영향을 주는 스텟, 맴버의 스텟이어야함

        [SerializeField] protected List<MiniGameStatDataSO> miniGameStatData; // 미니게임으로 오르는 스텟들
        [SerializeField] protected MiniGameResultSenderSO senderSO;
        
        protected int _statUpValue; // 스텟 비례로 오를 스텟의 기본 값
        protected bool _isPlaying;
        
        private void Awake()
        {
            Bus<StartCountingEndEvent>.OnEvent += HandleStart;
        }


        private void OnDestroy()
        {
            Bus<StartCountingEndEvent>.OnEvent -= HandleStart;
        }
        
        protected void Init()
        {
            resultUI.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 자식에서 재정의하고, base 호출하고 게임 시작시키셈
        /// </summary>
        protected virtual void HandleStart(StartCountingEndEvent evt)
        {
            _isPlaying = true;
        }
        
        protected virtual void GameEnd()
        {
            _isPlaying = false;
            foreach (var stat in miniGameStatData)
            {
                if (stat.statType > StatType.MusicPerfection)
                    senderSO.ChangeMusicStats.Add((stat.statType, (int)(stat.defaultValue + _statUpValue * stat.addMultiplier)));
                else if (stat.statType < StatType.MusicPerfection)
                {
                    foreach (var member in memberThrowSO.CurrentMembers)
                    {
                        senderSO.ChangeMemberStats.Add((member.memberType, stat.statType, (int)(stat.defaultValue + _statUpValue * stat.addMultiplier)));
                    }
                }
            }
            resultUI.OpenResultUI();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (needStatType > StatType.MusicPerfection)
            {
                Debug.LogError("needStatType에 Composition이나 InstrumentProficiency을 넣으세요");
            }
        }
#endif
    }
}