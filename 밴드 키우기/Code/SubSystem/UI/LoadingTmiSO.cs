using System.Collections.Generic;
using UnityEngine;

namespace Code.SubSystem.UI
{
    [CreateAssetMenu(fileName = "LoadingTmiData", menuName = "SO/UI/LoadingTmiData", order = 0)]
    public class LoadingTmiSO : ScriptableObject
    {
        [TextArea(2, 4)]
        public List<string> tmiList;

        public string GetRandom()
        {
            if (tmiList == null || tmiList.Count == 0) return string.Empty;
            return tmiList[Random.Range(0, tmiList.Count)];
        }
    }
}