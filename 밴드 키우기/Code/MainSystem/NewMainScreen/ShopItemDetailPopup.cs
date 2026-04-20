using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class ShopItemDetailPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        public event Action OnBuyClicked;
        public event Action OnCloseClicked;

        private void Awake()
        {
            if (buyButton != null)
                buyButton.onClick.AddListener(HandleBuyClicked);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDestroy()
        {
            if (buyButton != null)
                buyButton.onClick.RemoveListener(HandleBuyClicked);
            
            if (closeButton != null)
                closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void Show(Sprite icon, string itemName, string desc, int price)
        {
            Debug.Log($"[ShopItemDetailPopup] Show - Name: {itemName}, Price: {price}");
            
            if (iconImage != null) iconImage.sprite = icon;
            if (nameText != null) nameText.text = itemName;
            if (descText != null) descText.text = desc;
            if (priceText != null)
            {
                priceText.text = $"{price:N0}원";
                Debug.Log($"[ShopItemDetailPopup] PriceText set: {priceText.text}");
            }
            else
            {
                Debug.LogError("[ShopItemDetailPopup] priceText is null!");
            }

            if (popupPanel != null)
                popupPanel.SetActive(true);
        }

        public void Hide()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        private void HandleBuyClicked()
        {
            OnBuyClicked?.Invoke();
        }

        private void HandleCloseClicked()
        {
            OnCloseClicked?.Invoke();
        }
    }
}