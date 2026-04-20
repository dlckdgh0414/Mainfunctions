using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;
using UnityEngine;

namespace Code.SubSystem.Minigame.Common.Management
{
    [CreateAssetMenu(fileName = "MiniGameResultSender", menuName = "SO/MiniGame/ResultSender", order = 0)]
    public class MiniGameResultSenderSO : ScriptableObject
    {
        public List<(StatType, int)> ChangeMusicStats = new();
        public List<(MemberType, StatType, int)> ChangeMemberStats = new();
    }
}