using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Interface;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitMainPanelUI : TraitPanelBase, IUIElement<MemberType>
    {
        [SerializeField] private TextMeshProUGUI label;

        public void EnableFor(MemberType memberType)
        {
            label.SetText($"{memberType} 특성 UI");
            Bus<TraitShowRequested>.Raise(new TraitShowRequested(memberType));
            
            Show(); 
        }

        public void Disable()
        {
            Hide();
        }
    }
}