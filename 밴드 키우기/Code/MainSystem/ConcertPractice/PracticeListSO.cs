using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.ConcertPractice
{
    [Obsolete("RhythmGameDataSenderSO를 대신 사용하세요")]
    [CreateAssetMenu(fileName = "PracticeList", menuName = "SO/PracticeList", order = 0)]
    public class PracticeListSO : ScriptableObject
    {
        // 구조 변경으로 이거 이제 안씀.
        // RhythmGameDataSenderSO 사용 (양방향 통신용 SO. 구조가 좋은지는 모르겄다)
        //public List<UnitDataSO> members = new List<UnitDataSO>(); 
    }
}