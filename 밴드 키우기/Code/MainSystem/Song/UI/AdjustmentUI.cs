using System;
using System.Threading;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SongEvents;
using Code.MainSystem.Song.Director;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Song.UI
{
    /// <summary>
    /// Comment 연출이나 그래프 그리는거 담당한다.
    /// 일단 지금은 다른거 말고 Comment 연출만 한다.
    /// 평론가는 총 4개, 일반 평가는 팬같은 특정 수치, 곡의 스텟들을 기반으로 갯수가 증가함.
    /// </summary>
    public class AdjustmentUI : BaseUIComponent
    {
        [SerializeField] private Button showResultBtn;
        [SerializeField] private SongResultMaker songResultMaker;
        
        [SerializeField] private CommentController controller;
        [SerializeField] private PlayCountGraph playCountGraph;
        [SerializeField] private PlayCountShowUI playCountShowUI;
        
        private RectTransform _btnRect;
        private Vector2 _originalPos;
        private MusicReleaseResultData _data;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        public event Action OnShowResult;
        
        private void Awake()
        {
            _btnRect = showResultBtn.GetComponent<RectTransform>();
            _originalPos = _btnRect.anchoredPosition;
            HideButton();
            showResultBtn.onClick.AddListener(ShowResult);
        }

        private void OnDestroy()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public async void Open()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
    
            CancellationToken linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                _cts.Token, 
                this.GetCancellationTokenOnDestroy()
            ).Token;

            gameObject.SetActive(true);
            _data = songResultMaker.CalculateResult();

            var isCanceled =
                // 첫 번째 연출
                await controller.ShowComments(_data)
                .AttachExternalCancellation(linkedToken)
                .SuppressCancellationThrow();
            
            if (isCanceled || this == null) return;
            isCanceled = await playCountGraph.PlayGraphAnimation(_data.PlayCount)
                .AttachExternalCancellation(linkedToken)
                .SuppressCancellationThrow();
            
            if (isCanceled || this == null) return;
            playCountShowUI.ShowPlayCountText(_data.PlayCount)
                .AttachExternalCancellation(linkedToken);

            ShowNextButton();
        }

        public void Hide()
        {
            _cts?.Cancel();
            Bus<MusicReleaseResultEvent>.Raise(new MusicReleaseResultEvent(_data.EarnedMoney, _data.NewFans,
                _data.TotalScore, _data.PlayCount, _data.GetExp));
            gameObject.SetActive(false);
        }
        
        public void HideButton()
        {
            // float screenWidth = Screen.width;
            // _btnRect.anchoredPosition = new Vector2(screenWidth + 500, _originalPos.y);
        }
        
        public void ShowNextButton()
        {
            _btnRect.DOAnchorPos(_originalPos, 0.5f).SetEase(Ease.OutBack);
        }
        
        private void ShowResult()
        {
            OnShowResult?.Invoke();
        }

        public override void Reset()
        {
            _cts.Cancel();
            playCountGraph.Reset();
            controller.Reset();
            playCountShowUI.Hide();
        }
    }
}