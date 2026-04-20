using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.Tool.UIBaseScript
{
    /// <summary>
    /// 이전 - 이후 변화를 보여주는 UI
    /// </summary>
    public class BeforeAfterVariationUI : BaseVariationUI
    {
        [SerializeField] private TextMeshProUGUI beforeVariationText;
        [SerializeField] private string formatText;

        public override void VariableChange(float variationValue, float fromValue = 0, string variationName = null)
        {
            beforeVariationText.SetText(fromValue + formatText);
            base.VariableChange(variationValue, fromValue, variationName);
        }

        public override async UniTask VariableChangeToAnim(float variationValue,  float fromValue = 0, float duration = 1,  Ease ease = Ease.OutQuad,
            string variationName = null)
        {
            beforeVariationText.SetText(fromValue + formatText);
            await base.VariableChangeToAnim(variationValue, fromValue, duration, ease, variationName);
        }
    }
}