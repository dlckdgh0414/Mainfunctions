using UnityEngine;

namespace Code.Core.Load
{
    /// <summary>
    /// 세이브, 로드용 SO.
    /// 나중에 서버 도입시 서버의 값과 같은지 비교 로직 필요할지도
    /// </summary>
    [CreateAssetMenu(fileName = "SaveLoad", menuName = "SO/SaveLoad", order = 0)]
    public class SaveLoadSO : ScriptableObject
    {
        public bool isLoaded = false;
    }
}