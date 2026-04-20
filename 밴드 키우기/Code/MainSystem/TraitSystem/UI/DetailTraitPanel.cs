using TMPro;
using UnityEngine;
using Code.Core.Bus;
using UnityEngine.UI;
using Reflex.Attributes;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Manager;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.UI
{
    public class DetailTraitPanel : TraitPanelBase, IUIElement<ActiveTrait>
    {
        [Header("Dependencies")]
        [Inject] private TraitManager _traitManager;
        
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button removeButton;
        [SerializeField] private TextMeshProUGUI traitNameText;
        
        private ActiveTrait _currentTrait;

        private void Awake()
        {
            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemoveButtonClicked);
        }

        private void OnDestroy()
        {
            if (removeButton != null)
                removeButton.onClick.RemoveListener(OnRemoveButtonClicked);
        }

        public void EnableFor(ActiveTrait trait)
        {
            _currentTrait = trait;
            UpdateUI();
            Show();
        }

        public void Disable()
        {
            Hide();
        }

        private async void UpdateUI()
        {
            if (_currentTrait?.Data is null)
                return;

            await SetIconSafeAsync(iconImage, _currentTrait.Data.TraitIcon);
            
            traitNameText.SetText(_currentTrait.Data.TraitName);
            descriptionText.SetText(_currentTrait.GetFormattedDescription());

            if (removeButton is null)
                return;
            
            bool canRemove = _traitManager is not null
                && (_traitManager.GetHolder(_traitManager.CurrentMember)?.IsAdjusting ?? false) 
                || _currentTrait.Data.IsRemovable;
            removeButton.interactable = canRemove;
        }

        private void OnRemoveButtonClicked()
        {
            if (_currentTrait == null || _traitManager == null)
                return;

            RemoveTrait();
            RefreshTraitList();
            Hide();
        }

        private void RemoveTrait()
        {
            Bus<TraitRemoveRequestedUI>.Raise(new TraitRemoveRequestedUI(_currentTrait,
                _traitManager.GetHolder(_traitManager.CurrentMember)));
        }

        private void RefreshTraitList()
        {
            Bus<TraitShowRequested>.Raise(new TraitShowRequested(_traitManager.CurrentMember));
        }
        
        public void Close()
        {
            Hide();
        }
    }
}