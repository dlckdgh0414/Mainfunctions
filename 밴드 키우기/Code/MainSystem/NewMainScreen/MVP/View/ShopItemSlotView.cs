using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class ShopItemSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button slotButton;

        public event Action<ShopItemSlotView> OnSlotClicked;

        private int _itemIndex;

        private void Awake()
        {
            if (slotButton != null)
                slotButton.onClick.AddListener(HandleSlotClicked);
        }

        private void OnDestroy()
        {
            if (slotButton != null)
                slotButton.onClick.RemoveListener(HandleSlotClicked);
        }

        public void Setup(int itemIndex, Sprite icon, string itemName, int price)
        {
            _itemIndex = itemIndex;

            if (iconImage != null) iconImage.sprite = icon;
            if (nameText != null) nameText.text = itemName;
            if (priceText != null) priceText.text = $"{price:N0}원";
        }

        private void HandleSlotClicked()
        {
            OnSlotClicked?.Invoke(this);
        }

        public int GetItemIndex() => _itemIndex;
    }
}