using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Code.MainSystem.Song.Database;
using Code.MainSystem.Song.Director;

namespace Code.Editor
{
    [CustomEditor(typeof(CommentDatabaseSO))]
    public class CommentDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);
            GUI.backgroundColor = Color.cyan;

            if (GUILayout.Button("전체 Comment 데이터 자동 분류 및 업데이트", GUILayout.Height(30)))
            {
                OrganizeDatabase();
            }
        }

        private void OrganizeDatabase()
        {
            CommentDatabaseSO database = (CommentDatabaseSO)target;

            string[] guids = AssetDatabase.FindAssets("t:CommentDataSO");
            List<CommentDataSO> allComments = new List<CommentDataSO>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CommentDataSO so = AssetDatabase.LoadAssetAtPath<CommentDataSO>(path);
                if (so != null) allComments.Add(so);
            }

            Undo.RecordObject(database, "Organize Comment Database");
            database.CommentDatabase = new List<ScoreCommentList>();
            database.CirticCommentDatabase = new List<ScoreCommentList>();

            var grouped = allComments.GroupBy(c => new { c.IsCritic, Score = Mathf.RoundToInt(c.Star * 2f) });

            foreach (var group in grouped)
            {
                int scoreKey = Mathf.Clamp(group.Key.Score, 1, 10);
                
                ScoreCommentList newList = new ScoreCommentList
                {
                    Score = scoreKey,
                    Comments = group.ToList()
                };
                
                if (group.Key.IsCritic)
                {
                    database.CirticCommentDatabase.Add(newList);
                }
                else
                {
                    database.CommentDatabase.Add(newList);
                }
            }
            
            // 4. Score 순으로 정렬 (보기 편하게)
            database.CommentDatabase = database.CommentDatabase.OrderBy(x => x.Score).ToList();
            database.CirticCommentDatabase = database.CirticCommentDatabase.OrderBy(x => x.Score).ToList();
            
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[CommentDatabase] 분류 완료! (일반: {database.CommentDatabase.Count}그룹, 평론가: {database.CirticCommentDatabase.Count}그룹)");
        }
    }
}