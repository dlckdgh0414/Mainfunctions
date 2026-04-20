using System; // Enum 처리를 위해 추가
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.Synergy.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.Synergy.Manager;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitBar : TraitPanelBase, IUIElement<ActiveTrait>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelPointText;

        [SerializeField] private Transform traitTagRoot;
        [SerializeField] private TraitTagUI traitTagPrefab;
        
        private readonly List<TraitTagUI> _activeTagUIs = new();
        private ActiveTrait _currentItem;

        public async void EnableFor(ActiveTrait item)
        {
            _currentItem = item;

            await SetIconSafeAsync(iconImage, item.Data.TraitIcon);
            nameText.SetText(_currentItem.Data.TraitName);
            levelPointText.SetText(_currentItem.Data.Point.ToString());
            
            RefreshTagUIs(item.Data.TraitTag);
            
            gameObject.SetActive(true);
        }

        private void RefreshTagUIs(TraitTag tags)
        {
            var synergy = SynergyEffectManager.Instance;
            
            foreach (var ui in _activeTagUIs)
                ui.gameObject.SetActive(false);
            
            _activeTagUIs.Clear();
            
            foreach (TraitTag traitTag in Enum.GetValues(typeof(TraitTag)))
            {
                if (traitTag == TraitTag.None)
                    continue;

                if ((tags & traitTag) != traitTag)
                    continue;
                
                var synergyData = synergy.GetActiveSynergy(traitTag);
                
                if (synergyData != null)
                    CreateTagUI(synergyData);
            }
        }

        private void CreateTagUI(Code.MainSystem.Synergy.Runtime.ActiveSynergy synergy)
        {
            TraitTagUI tagUI = Instantiate(traitTagPrefab, traitTagRoot);
            tagUI.EnableFor(synergy);
            _activeTagUIs.Add(tagUI);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            
            foreach (var ui in _activeTagUIs) 
                Destroy(ui.gameObject);
            
            _activeTagUIs.Clear();
        }
        
        public void Click()
        {
            Bus<TraitShowItem>.Raise(new TraitShowItem(_currentItem));
        }
    }
}