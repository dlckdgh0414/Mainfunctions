
using System;
using UnityEngine;

namespace Code.MainSystem.Song.Director
{
    /// <summary>
    /// 곡 댓글(평가)부분에서 사용할 데이터
    /// 일단 생각중인거는 일반 평가와, 평론가 평이 나뉘는거.
    /// 일반 평가는 별이 안붙고, 평론가는 별이 붙는다. 평론가는 항상 4명씩 나오게끔
    /// </summary>
    [CreateAssetMenu(fileName = "CommentData", menuName = "SO/Comment/Data", order = 0)]
    public class CommentDataSO : ScriptableObject
    {
        [field: SerializeField] public string UserName { get; private set; }
        [field: SerializeField, TextArea] public string CommentText { get; private set; }

        /// <summary>
        /// .5 단위로 쪼개짐(0.0 ~ 5.0)
        /// </summary>
        [field: SerializeField, Range(0.5f, 5.0f)] public float Star{ get; private set; } // 일반 사용자는 UI상에서 표시하지는 않는다.

        [field: SerializeField] public bool IsCritic { get; private set; } // 평론가인지
        
        #if UNITY_EDITOR
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(UserName))
                UserName = Guid.NewGuid().ToString()[..13];
        }
        
        #endif
    }
}