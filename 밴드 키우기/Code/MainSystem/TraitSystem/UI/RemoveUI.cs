using TMPro;
using UnityEngine;
using Code.Core.Bus;
using UnityEngine.UI;
using Reflex.Attributes;
using Code.MainSystem.TraitSystem.Manager;
using Code.MainSystem.TraitSystem.Runtime;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.UI
{
    public class RemoveUI : TraitPanelBase, IUIElement<ActiveTrait, ITraitHolder>
    {
        [Header("Dependencies")]
        [Inject] private TraitManager _traitManager;
        
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Toggle toggle;
        
        public bool IsCheck { get; private set; }
        
        private ActiveTrait _currentTrait;
        private ITraitHolder _currentHolder;
        
        private void Awake()
        {
            toggle.isOn = false;
            IsCheck = false;
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        public void EnableFor(ActiveTrait trait, ITraitHolder holder)
        {
            if (IsCheck)
            {
                Bus<TraitRemoveRequested>.Raise(new TraitRemoveRequested(TraitManager.Instance.CurrentMember,
                    trait.Data));
                return;
            }

            _currentTrait = trait;
            _currentHolder = holder;
            UpdateUI();
            Show();
        }

        public void Disable()
        {
            _currentTrait = null;
            _currentHolder = null;
            Hide();
            ClearUI();
        }

        private async void UpdateUI()
        {
            if (_currentTrait?.Data is null || _currentHolder == null)
                return;
            
            await SetIconSafeAsync(iconImage, _currentTrait.Data.TraitIcon);

            nameText.SetText($"{_currentTrait.Data.TraitName}");

            int afterPoint = _currentHolder.TotalPoint - _currentTrait.Data.Point;
            string pointInfo = "삭제 후 특성 포인트\n " +
                               $"{_currentHolder.TotalPoint} / {_currentHolder.MaxPoints} ->  {afterPoint} / {_currentHolder.MaxPoints}";
            descriptionText.SetText(pointInfo);
        }

        private void ClearUI()
        {
            if (iconImage != null)
                iconImage.sprite = null;
            
            if (nameText != null)
                nameText.SetText("");
            
            if (descriptionText != null)
                descriptionText.SetText("");
        }

        public void OnConfirmClicked()
        {
            if (_currentTrait == null || _traitManager == null)
                return;

            RemoveTrait();
            Disable();
        }

        public void OnCancelClicked()
        {
            Disable();
        }

        private void RemoveTrait()
        {
            Bus<TraitRemoveRequested>.Raise(new TraitRemoveRequested(_traitManager.CurrentMember, _currentTrait.Data));
            Bus<TraitShowRequested>.Raise(new TraitShowRequested(_traitManager.CurrentMember));
        }

        public void Close()
        {
            OnCancelClicked();
        }

        public void TurnEnd()
        {
            IsCheck = false;
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            IsCheck = isOn;
        }
    }
}