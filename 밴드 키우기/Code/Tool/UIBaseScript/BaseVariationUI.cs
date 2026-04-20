using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.Tool.UIBaseScript
{
    /// <summary>
    /// UI상에서 숫자로 변화를 줘야하는 부분의 기반 스크립트.
    /// 결과창 수치변화등에 사용하면 됨.
    /// </summary>
    public class BaseVariationUI : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI variationNameText; // 이거 텍스트 안바꿀거면 걍 None로 두면 됨.
        [SerializeField] protected TextMeshProUGUI variationText;
        [SerializeField] private string textFormat;
        
        protected Tween _currentTween;
        
        public virtual void VariableChange(float variationValue, float fromValue = 0, string variationName = null)
        {
            KillCurrentTween();
            if (variationNameText != null && variationName != null)
                variationNameText.SetText(variationName);
            variationText.SetText(variationValue + textFormat);
        }

        /// <summary>
        /// 드르륵 하면서 올라가는 에니메이션으로 보여줌.
        /// await로 걸어두면 종료까지 대기할 수 있다.
        /// </summary>
        /// <param name="variationValue">최종 값</param>
        /// <param name="fromValue">어디부터 시작할지(기본 0)</param>
        /// <param name="duration">지속 시간(기본 0.5)</param>
        /// <param name="ease">이징(기본 OutQuad)</param>
        /// <param name="variationName">변경 이름(없으면 변하지 않음)</param>
        public virtual async UniTask VariableChangeToAnim(float variationValue, float fromValue = 0, float duration = 1f, Ease ease = Ease.OutQuad,  string variationName = null) 
        {
            KillCurrentTween();
            
            if (variationNameText != null && variationName != null)
                variationNameText.SetText(variationName);
            
            float currentValue = 0;
            Debug.Log($"0{textFormat}");
            variationText.SetText($"0{textFormat}");
            _currentTween = DOTween.To(() => currentValue, x => currentValue = x, variationValue, duration)
                .SetEase(ease)
                .OnUpdate(() =>
                {
                    variationText.SetText((int)currentValue + textFormat);
                })
                .SetLink(gameObject);

            var cts = this.GetCancellationTokenOnDestroy();
            while (_currentTween != null && _currentTween.IsActive() && !_currentTween.IsComplete())
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cts);
            }
            
            variationText.SetText((int)variationValue + textFormat);
            _currentTween = null;
        }
        
        protected void KillCurrentTween()
        {
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
            }
        }
        
        private void OnDestroy()
        {
            KillCurrentTween();
        }
    }
}