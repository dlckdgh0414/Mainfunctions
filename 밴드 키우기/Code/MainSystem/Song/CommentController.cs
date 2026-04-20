using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.Song.Database;
using Code.MainSystem.Song.Director;
using Code.SubSystem.BandFunds;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.MainSystem.Song
{
    public class CommentController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CommentDatabaseSO commentDatabase;
        [SerializeField] private CommentObjectUI commentPrefab;
        [SerializeField] private Transform commentParent;
        
        private List<CommentObjectUI> _comments = new List<CommentObjectUI>();
        private HashSet<CommentDataSO> _usedCommentDatas = new HashSet<CommentDataSO>();
        private CancellationTokenSource _cts;
        
        public void Reset()
        {
            foreach (CommentObjectUI commentObj in _comments)
            {
                if (commentObj != null) Destroy(commentObj.gameObject);
            }
            _comments.Clear();
            _usedCommentDatas.Clear();
        }
        
        public async UniTask ShowComments(MusicReleaseResultData data)
        {
            Reset();
            ApplyData(data);
            
            // 1. 비평가 코멘트 생성 (리스트 앞부분)
            GenerateCriticComments(data.AverageStars);
            
            // 2. 일반 유저 코멘트 생성 (리스트 뒷부분)
            GenerateUserComments(data); 
            
            float waitTime = Mathf.Max(6f / _comments.Count, 0.25f);
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // 리스트에 추가된 순서대로 화면에 출력
            for (var i = 0; i < _comments.Count; i++)
            {
                var commentObj = _comments[i];
                if(i < 4)await UniTask.WaitForSeconds(1f).AttachExternalCancellation(_cts.Token);
                else await UniTask.WaitForSeconds(waitTime).AttachExternalCancellation(_cts.Token);
                if (commentObj == null) return;

                commentObj.transform.localScale = new Vector3(1, 0.5f, 1);
                commentObj.gameObject.SetActive(true);
                commentObj.transform.DOScaleY(1f, 0.25f).SetLink(commentObj.gameObject);
                commentObj.transform.SetAsLastSibling();
            }
        }
        
        public void GenerateCriticComments(List<float> criticScores)
        {
            foreach (float rawScore in criticScores)
            {
                // 랜덤 변동 없이 정확한 점수 인덱스 사용
                int targetScore = Mathf.Clamp(Mathf.RoundToInt(rawScore * 2), 1, 10);

                var commentList = commentDatabase.CirticCommentDatabase
                    .FirstOrDefault(x => x.Score == targetScore);
                
                if (commentList.Comments != null && commentList.Comments.Count > 0)
                {
                    // 중복 방지 필터링
                    var available = commentList.Comments.Where(c => !_usedCommentDatas.Contains(c)).ToList();
                    
                    CommentDataSO selected = available.Count > 0 
                        ? available[Random.Range(0, available.Count)] 
                        : commentList.Comments[Random.Range(0, commentList.Comments.Count)];

                    _usedCommentDatas.Add(selected);
                    CreateCommentUI(selected);
                }
            }
        }

        public void GenerateUserComments(MusicReleaseResultData data)
        {
            float scoreWeight = data.TotalScore;
            float fanImpactWeight = data.PlayCount / 150f; 
            int targetCount = (int)Mathf.Clamp(scoreWeight + fanImpactWeight, 5f, 20f);
    
            // 1. 기준 인덱스 계산
            int maxIndex = commentDatabase.CommentDatabase.Count - 1;
            int targetIndex = Mathf.Clamp(Mathf.RoundToInt(data.TotalScore), 0, maxIndex);
    
            // 2. 인덱스 범위 결정 (좌우 한 칸 포함 총 3개)
            List<int> indicesToUse = new List<int>();
    
            if (commentDatabase.CommentDatabase.Count <= 3)
            {
                // 전체 데이터가 3개 이하일 경우 모든 인덱스 사용
                for (int i = 0; i < commentDatabase.CommentDatabase.Count; i++) indicesToUse.Add(i);
            }
            else
            {
                int start = targetIndex - 1;
                // 하한선 돌파 시: 0, 1, 2 사용
                if (start < 0) start = 0;
                // 상한선 돌파 시: 끝에서 3개 사용
                if (start + 2 > maxIndex) start = maxIndex - 2;

                indicesToUse.Add(start);
                indicesToUse.Add(start + 1);
                indicesToUse.Add(start + 2);
            }

            // 3. 선택된 모든 인덱스의 댓글을 하나의 풀(Pool)로 합치기
            List<CommentDataSO> allPotentialComments = new List<CommentDataSO>();
            foreach (int idx in indicesToUse)
            {
                if (commentDatabase.CommentDatabase[idx].Comments != null)
                {
                    allPotentialComments.AddRange(commentDatabase.CommentDatabase[idx].Comments);
                }
            }

            if (allPotentialComments.Count == 0) return;

            // 4. 중복 제거 및 랜덤 추출
            List<CommentDataSO> uniqueAvailable = allPotentialComments
                .Where(c => !_usedCommentDatas.Contains(c))
                .ToList();

            int finalCount = Mathf.Min(targetCount, uniqueAvailable.Count);
            // 풀이 부족할 경우 대비 (중복 허용하여 모자란 수 채우기 - 선택 사항)
            var selectedOnes = uniqueAvailable.OrderBy(x => Random.value).Take(finalCount);

            foreach (var comment in selectedOnes)
            {
                _usedCommentDatas.Add(comment);
                CreateCommentUI(comment);
            }
        }
        
        public void ApplyData(MusicReleaseResultData data)
        {
            BandSupplyManager.Instance.AddBandFunds(data.EarnedMoney);
            BandSupplyManager.Instance.AddBandExp(data.GetExp);
            BandSupplyManager.Instance.AddBandFans(data.NewFans);
            GameStatManager.Instance.ResetMusicAll();
        }
        
        private void CreateCommentUI(CommentDataSO data)
        {
            CommentObjectUI obj = Instantiate(commentPrefab, commentParent);
            obj.SetupData(data);
            obj.gameObject.SetActive(false);
            _comments.Add(obj); // 추가된 순서가 출력 순서
        }
    }
}