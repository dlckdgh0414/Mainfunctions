using UnityEngine;

namespace Code.MainSystem.StatSystem.BaseStats
{
    [CreateAssetMenu(fileName = "Stat range", menuName = "SO/Stat/Stat data range", order = 0)]
    public class StatValueRange : ScriptableObject
    {
        public int Min;
        public int Max;
    }
}