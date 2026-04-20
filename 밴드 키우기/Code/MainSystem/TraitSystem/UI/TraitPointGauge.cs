using System;
using UnityEngine;
using Code.MainSystem.TraitSystem.Interface;
using TMPro;
using UnityEngine.UI;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitPointGauge : MonoBehaviour, IUIElement<int, int>
    {
        [SerializeField] private TextMeshProUGUI pointText;
        [SerializeField] private Slider pointSlider;
        [SerializeField] private Image fillImage;

        private void Awake()
        {
            pointSlider.interactable = false;
        }

        public void EnableFor(int totalPoint, int maxPoint)
        {
            pointText.SetText($"{totalPoint} / {maxPoint}");
            pointText.color = GetColor(totalPoint, maxPoint);
            
            pointSlider.maxValue = maxPoint;
            pointSlider.value = totalPoint;
            fillImage.color = GetColor(totalPoint, maxPoint);
        }
        
        public void Disable()
        {
            pointText.SetText("");
            pointText.color = Color.black;
        }
        
        private Color GetColor(int totalPoint, int maxPoint)
            => totalPoint > maxPoint ? Color.red : Color.black;
    }
}