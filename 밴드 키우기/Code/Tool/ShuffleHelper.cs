using System.Collections.Generic;
using UnityEngine;

namespace Code.Tool
{
    public static class ShuffleHelper
    {
        /// <summary>
        /// 리스트 확장 메서드
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
        
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}