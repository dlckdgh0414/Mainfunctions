using System;
using System.Collections.Generic;
using Code.MainSystem.Song.Director;
using UnityEngine;

namespace Code.MainSystem.Song.Database
{
    [Serializable]
    public struct ScoreCommentList
    {
        public int Score; // 일반 사용자는 출력에는 안쓰고 내부용
        public List<CommentDataSO> Comments;
    }
    
    [CreateAssetMenu(fileName = "CommentDatabase", menuName = "SO/Comment/Database", order = 0)]
    public class CommentDatabaseSO : ScriptableObject
    {
        // 일반 사용자 DB
        public List<ScoreCommentList> CommentDatabase;
        
        // 평론가 DB
        [Header("Score는 * 0.5를 한 값이 별 개수임 (1 ~ 10 = 0.5 ~ 5.0)")]
        public List<ScoreCommentList> CirticCommentDatabase;
    }
}