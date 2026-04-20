using System.Collections.Generic;
using Code.MainSystem.Synergy.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Synergy.UI
{
    public class SynergyStatusUI : MonoBehaviour
    {
        [Header("UI Elements")] 
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Visual Feedback")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color activeIconColor = Color.white;
        [SerializeField] private Color inactiveIconColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        [Header("Pool Templates")]
        [SerializeField] private Transform procedureRoot;
        [SerializeField] private NextSynergyValueUI nextValuePrefab;
        [SerializeField] private Image arrowPrefab;

        private readonly List<NextSynergyValueUI> _valuePool = new();
        private readonly List<Image> _arrowPool = new();

        public void EnableFor(ActiveSynergy data)
        {
            ResetPool();
            
            bool isAnyThresholdMet = data.SynergyData.Thresholds.Count > 0 && 
                                     data.CurrentCount >= data.SynergyData.Thresholds[0];
            
            UpdateStaticInfo(data, isAnyThresholdMet);
            BuildProcedure(data);
            
            if (canvasGroup is not null)
                canvasGroup.alpha = isAnyThresholdMet ? 1.0f : 0.6f;

            contentRoot.SetActive(true);
        }

        private void UpdateStaticInfo(ActiveSynergy data, bool isActive)
        {
            iconImage.sprite = data.SynergyData.SynergyIcon;
            iconImage.color = isActive ? activeIconColor : inactiveIconColor;
            
            currentValueText.SetText(data.CurrentCount.ToString());
            currentValueText.color = isActive ? Color.black : Color.white;
            
            nameText.SetText(data.SynergyData.SynergyName);
            descriptionText.SetText(data.GetFormattedDescription());
        }

        private void BuildProcedure(ActiveSynergy data)
        {
            var thresholds = data.SynergyData.Thresholds;
            for (int i = 0; i < thresholds.Count; i++)
            {
                if (i > 0)
                {
                    var arrow = GetArrow(i - 1);
                    arrow.gameObject.SetActive(true);
                    arrow.color = thresholds[i] <= data.CurrentCount ? activeIconColor : inactiveIconColor;
                }
                
                var valueUI = GetValueUI(i);
                valueUI.gameObject.SetActive(true);
                
                bool isReached = thresholds[i] <= data.CurrentCount;
                valueUI.EnableFor(thresholds[i], isReached);
            }
        }

        private Image GetArrow(int index)
        {
            if (index < _arrowPool.Count) return _arrowPool[index];
            var arrow = Instantiate(arrowPrefab, procedureRoot);
            _arrowPool.Add(arrow);
            return arrow;
        }

        private NextSynergyValueUI GetValueUI(int index)
        {
            if (index < _valuePool.Count) return _valuePool[index];
            var valueUI = Instantiate(nextValuePrefab, procedureRoot);
            _valuePool.Add(valueUI);
            return valueUI;
        }

        private void ResetPool()
        {
            _arrowPool.ForEach(x => x.gameObject.SetActive(false));
            _valuePool.ForEach(x => x.Disable());
        }

        public void Disable()
        {
            ResetPool();
            contentRoot.SetActive(false);
        }
    }
}