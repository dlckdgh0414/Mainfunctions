using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.UI.Test
{
    public class TraitControllerTest : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private MemberType memberType;
        
        public void ShowList()
        {
            label.SetText($"{memberType.ToString()}의 특성 패널");
            Bus<TraitShowRequested>.Raise(new TraitShowRequested(memberType));
        }
    }
}