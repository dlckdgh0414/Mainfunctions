using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Tool.UIBaseScript
{
    /// <summary>
    /// VariationUI에 바가 추가된 UI
    /// </summary>
    public class BarVariationUI : BaseVariationUI
    {
        [SerializeField] private Image variationBar;
        [Header("Data Setting")]
        [SerializeField] private float maxValue;
        
        private readonly string SLASH_FORMAT = "{0:F1}/{1:F1}";
        
        public void SetMaxValue(float max)
        {
            maxValue = max;
        }
        
        public override void VariableChange(float variationValue, float fromValue = 0, string variationName = null)
        {
            base.VariableChange(variationValue, fromValue, variationName);
            variationText.SetText(string.Format(SLASH_FORMAT, variationValue, 5.0f));
            variationBar.fillAmount = variationValue / maxValue;
        }

        public override async UniTask VariableChangeToAnim(float variationValue, float fromValue = 0, float duration = 1, Ease ease = Ease.OutQuad,
            string variationName = null)
        {
            KillCurrentTween();
            
            if (variationNameText != null && variationName != null)
                variationNameText.SetText(variationName);
            
            float currentValue = fromValue;

            _currentTween = DOTween.To(() => currentValue, x => currentValue = x, variationValue, duration)
                .SetEase(ease)
                .OnUpdate(() =>
                {
                    variationText.SetText(string.Format(SLASH_FORMAT, currentValue, maxValue));
                    variationBar.fillAmount = currentValue / maxValue;
                })
                .SetLink(gameObject);

            var cts = this.GetCancellationTokenOnDestroy();
            while (_currentTween != null && _currentTween.IsActive() && !_currentTween.IsComplete())
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cts);
            }
            
            variationText.SetText(string.Format(SLASH_FORMAT, variationValue, maxValue));
            _currentTween = null;

        }
    }
}