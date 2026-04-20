using System;
using System.Threading;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SongEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.SubSystem.BandFunds;
using Code.Tool.UIBaseScript;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.MainSystem.Song.UI
{
    /// <summary>
    /// 최종 결과 UI를 담당한다.
    /// </summary>
    public class FinalResultUI : BaseUIComponent
    {
        [SerializeField] private BaseVariationUI goldVariationUI;
        [SerializeField] private BaseVariationUI penVariationUI;
        [SerializeField] private BaseVariationUI starVariationUI;
        [SerializeField] private BaseVariationUI numberOfPlaysUI;
        [SerializeField] private BaseVariationUI expVariationUI;
        
        public event Action CloseResult;
        
        private MusicReleaseResultEvent _data;
        private CancellationTokenSource _cts;
        private bool _isTutorialUploadCompletedRaised;
        
        private void Awake()
        {
            Button btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(HandleClicked);
            
            Bus<MusicReleaseResultEvent>.OnEvent += HandleMusicReleseResult;
            
            InitUI();
        }

        private void InitUI()
        {
            goldVariationUI.gameObject.SetActive(false);
            penVariationUI.gameObject.SetActive(false);
            starVariationUI.gameObject.SetActive(false);
            numberOfPlaysUI.gameObject.SetActive(false);
            expVariationUI.gameObject.SetActive(false);
        }

        private void HandleClicked()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                CompleteImmediately();
            }
            else
            {
                RaiseTutorialUploadCompleted();
                CloseResult?.Invoke();
            }
        }

        private void HandleMusicReleseResult(MusicReleaseResultEvent evt)
        {
            gameObject.SetActive(true);
            _data = evt;
            _isTutorialUploadCompletedRaised = false;
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            PlayUIAnimation(_cts.Token).Forget();
        }

        private async UniTask PlayUIAnimation(CancellationToken token)
        {
            try
            {
                goldVariationUI.gameObject.SetActive(true);
                await goldVariationUI
                    .VariableChangeToAnim(
                        _data.Gold, BandSupplyManager.Instance.BandFunds).AttachExternalCancellation(token);
                
                penVariationUI.gameObject.SetActive(true);
                await penVariationUI.VariableChangeToAnim(_data.Pen).AttachExternalCancellation(token);
                
                starVariationUI.gameObject.SetActive(true);
                await starVariationUI.VariableChangeToAnim(_data.Star).AttachExternalCancellation(token);
                
                numberOfPlaysUI.gameObject.SetActive(true);
                await numberOfPlaysUI.VariableChangeToAnim(_data.NumberOfPlays).AttachExternalCancellation(token);
                
                expVariationUI.gameObject.SetActive(true);
                await expVariationUI
                    .VariableChangeToAnim(_data.Exp, BandSupplyManager.Instance.BandExp).AttachExternalCancellation(token);
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// 튜토리얼 업로드 완료 신호를 1회 전송.
        /// </summary>
        private void RaiseTutorialUploadCompleted()
        {
            if (_isTutorialUploadCompletedRaised)
            {
                return;
            }

            _isTutorialUploadCompletedRaised = true;
            Bus<TutorialUploadCompletedEvent>.Raise(new TutorialUploadCompletedEvent(_data.Gold, _data.Pen, _data.Exp));
        }

        /// <summary>
        /// 모든 UI 요소를 최종 값으로 즉시 설정
        /// </summary>
        private void CompleteImmediately()
        {
            goldVariationUI.gameObject.SetActive(true);
            goldVariationUI.VariableChange(_data.Gold);
            
            penVariationUI.gameObject.SetActive(true);
            penVariationUI.VariableChange(_data.Pen);
            
            starVariationUI.gameObject.SetActive(true);
            starVariationUI.VariableChange(_data.Star);
            
            numberOfPlaysUI.gameObject.SetActive(true);
            numberOfPlaysUI.VariableChange(_data.NumberOfPlays);
            
            expVariationUI.gameObject.SetActive(true);
            expVariationUI.VariableChange(_data.Exp);
        }
        
        private void OnDestroy()
        {
            Bus<MusicReleaseResultEvent>.OnEvent -= HandleMusicReleseResult;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public override void Reset()
        {
            InitUI();
            _isTutorialUploadCompletedRaised = false;
        }
    }
}
