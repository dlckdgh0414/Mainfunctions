using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Core.Addressable
{
    public class AddressableLoadUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private RectTransform progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI tmiText;
        
        [Header("Settings")]
        [SerializeField] private float tmiChangeInterval = 1.8f;
        
        private int _totalSteps = 0;
        private int _currentStep = 0;
        private CancellationTokenSource _cancellationTokenSource;

        public void ShowLoadingUI(int totalSteps)
        {
            _totalSteps = totalSteps;
            _currentStep = 0;
            
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }
            
            ShowRandomTMI();
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            _cancellationTokenSource = new CancellationTokenSource();
            ChangeTMIPeriodically(_cancellationTokenSource.Token).Forget();
            
            UpdateLoadingUI();
        }

        public void UpdateProgress(string message)
        {
            _currentStep++;
            
            if (loadingText != null)
            {
                loadingText.text = message;
            }
            
            UpdateLoadingUI();
        }

        public void HideLoadingUI()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// TMI 리스트에서 랜덤하게 하나를 선택하여 표시
        /// </summary>
        private void ShowRandomTMI()
        {
            if (tmiText == null)
            {
                Debug.LogWarning("[AddressableLoadUI] TMI Text component is not assigned");
                return;
            }
            
            // if (tmiList == null || tmiList.TMIList == null || tmiList.TMIList.Count == 0)
            // {
            //     Debug.LogWarning("[AddressableLoadUI] TMI List is empty or not assigned");
            //     tmiText.text = "";
            //     return;
            // }
            //
            // int randomIndex = UnityEngine.Random.Range(0, tmiList.TMIList.Count);
            // string selectedTMI = tmiList.TMIList[randomIndex];
            
            // tmiText.text = selectedTMI;
            //
            // Debug.Log($"[AddressableLoadUI] Showing TMI: {selectedTMI}");
        }

        /// <summary>
        /// 일정 시간마다 TMI를 자동으로 변경
        /// </summary>
        private async UniTaskVoid ChangeTMIPeriodically(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await UniTask.Delay((int)(tmiChangeInterval * 1000), cancellationToken: cancellationToken);
                    
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ShowRandomTMI();
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[AddressableLoadUI] TMI change task cancelled");
            }
        }

        private void UpdateLoadingUI()
        {
            if (_totalSteps == 0) return;

            float progress = (float)_currentStep / _totalSteps;
            
            if (progressBar != null)
            {
                progressBar.localScale = new Vector3(progress, 1f, 1f);
            }
            
            if (_currentStep >= _totalSteps)
            {
                Invoke(nameof(HideLoadingUI), 0.5f);
            }
        }
    }
}