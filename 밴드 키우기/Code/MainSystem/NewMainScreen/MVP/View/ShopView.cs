using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class ShopView : MonoBehaviour, IShopView
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform listBarParent;
        [SerializeField] private ShopListBar listBarPrefab;
        [SerializeField] private ShopItemDetailPopup itemDetailPopup;

        private List<ShopListBar> _instantiatedBars = new List<ShopListBar>();

        public event Action OnShopClosed;

        private void Awake()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
            
            if (itemDetailPopup != null)
                itemDetailPopup.Hide();
        }

        public void Show()
        {
            if (shopPanel != null)
                shopPanel.SetActive(true);
        }

        public void Hide()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
        }

        public void ClearItems()
        {
            foreach (var bar in _instantiatedBars)
            {
                if (bar != null)
                    Destroy(bar.gameObject);
            }
            _instantiatedBars.Clear();
        }

        public void OnCloseButtonClicked()
        {
            OnShopClosed?.Invoke();
        }

        public ShopListBar CreateListBar()
        {
            if (listBarPrefab == null || listBarParent == null)
            {
                Debug.LogError("[ShopView] ListBarPrefab or Parent is null");
                return null;
            }

            var bar = Instantiate(listBarPrefab, listBarParent);
            _instantiatedBars.Add(bar);
            return bar;
        }

        public ShopItemDetailPopup GetDetailPopup() => itemDetailPopup;
    }
}