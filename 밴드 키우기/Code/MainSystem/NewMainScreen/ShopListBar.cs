using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.MVP.View;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class ShopListBar : MonoBehaviour
    {
        [SerializeField] private List<ShopItemSlotView> itemSlots = new List<ShopItemSlotView>();

        private void Awake()
        {
            if (itemSlots.Count == 0)
            {
                itemSlots.AddRange(GetComponentsInChildren<ShopItemSlotView>(true));
            }
        }

        public List<ShopItemSlotView> GetSlots() => itemSlots;

        public int GetSlotCount() => itemSlots.Count;
    }
}