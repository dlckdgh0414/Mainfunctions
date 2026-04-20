using Code.MainSystem.Synergy.Interface;
using Code.MainSystem.Synergy.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitTagUI : TraitPanelBase, IUIElement<ActiveSynergy>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI currentValueText;

        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        public void EnableFor(ActiveSynergy data)
        {
            iconImage.sprite = data.SynergyData.SynergyIcon;
            currentValueText.SetText(data.CurrentCount.ToString());

            bool isActive = data.SynergyData.Thresholds.Count > 0 &&
                            data.CurrentCount >= data.SynergyData.Thresholds[0];

            currentValueText.color = isActive ? activeColor : inactiveColor;
        }

        public void Disable()
        {
           
        }
    }
}