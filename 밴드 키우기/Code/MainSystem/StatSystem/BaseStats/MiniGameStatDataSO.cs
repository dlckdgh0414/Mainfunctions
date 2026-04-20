using UnityEngine;
using UnityEngine.Serialization;

namespace Code.MainSystem.StatSystem.BaseStats
{
    /// <summary>
    /// 미니게임에서 올라가는 스텟 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "MiniGameStatData", menuName = "SO/MiniGame/StatData", order = 0)]
    public class MiniGameStatDataSO : ScriptableObject
    {
        public StatType statType; // 스텟 타입
        public int defaultValue; // 기본 상승량
        public float addMultiplier; // 추가 상승량 계산시 스텟에 얼마 곱할지 
        
    }
}