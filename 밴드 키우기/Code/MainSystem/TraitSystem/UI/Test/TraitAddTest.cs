using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Manager;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.UI.Test
{
    public class TraitAddTest : MonoBehaviour
    {
        [SerializeField] private TraitDataSO traitData;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TraitManager traitManager;

        private void Start()
        {
            label.SetText($"{traitData.TraitName}\n추가 하기");
        }

        public void AddTrait()
        {
            Bus<TraitAddRequested>.Raise(new TraitAddRequested(traitManager.CurrentMember, traitData));
            Bus<TraitShowRequested>.Raise(new TraitShowRequested(traitManager.CurrentMember));
        }
    }
}