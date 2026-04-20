using System;
using Code.MainSystem.StatSystem.BaseStats;
using UnityEngine;

namespace Code.SubSystem.Minigame.Common.Management
{
    /// <summary>
    /// 메인 씬에서 미니게임의 결과를 받아서 처리해준다.
    /// </summary>
    public class MiniGameResultReceiver : MonoBehaviour
    {
        [SerializeField] private MiniGameResultSenderSO sender;

        private void Start()
        {
            foreach (var (stat, value) in sender.ChangeMusicStats)
            {
                if (stat <= StatType.InstrumentProficiency)
                {
                    Debug.LogError($"{stat} stat increase to {value}, but {stat} is not music stat.");
                    continue;
                }
            }
            sender.ChangeMusicStats.Clear();

            foreach (var (member, stat, value) in sender.ChangeMemberStats)
            {
                if (stat > StatType.InstrumentProficiency)
                {
                    Debug.LogError($"{stat} stat increase to {value}, but {stat} is not member stat.");
                    continue;
                }
            }
            sender.ChangeMemberStats.Clear();
        }
    }
}