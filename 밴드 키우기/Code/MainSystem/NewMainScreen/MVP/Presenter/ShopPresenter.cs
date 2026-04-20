using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public class ShopPresenter : IShopPresenter
    {
        private readonly ShopView _view;
        private readonly ShopItemListSO _itemListData;
        private readonly ShopItemDetailPopup _detailPopup;
        private readonly MemberSelectPopup _memberSelectPopup;
        private readonly List<MemberDataSO> _memberDataList;
        private readonly List<ShopItemSlotView> _allSlots = new();
        private readonly HashSet<string> _purchasedItems = new();

        private int _selectedItemIndex = -1;
        private const int SLOTS_PER_BAR = 3;

        public ShopPresenter(
            IShopView view,
            ShopItemListSO itemListData,
            List<MemberDataSO> memberDataList,
            MemberSelectPopup memberSelectPopup)
        {
            _view              = view as ShopView;
            _itemListData      = itemListData;
            _memberDataList    = memberDataList;
            _memberSelectPopup = memberSelectPopup;
            _detailPopup       = _view.GetDetailPopup();

            _view.OnShopClosed += HandleShopClosed;

            if (_detailPopup != null)
            {
                _detailPopup.OnBuyClicked   += HandleBuyClicked;
                _detailPopup.OnCloseClicked += HandlePopupClosed;
                _detailPopup.Hide();
            }

            if (_memberSelectPopup != null)
                _memberSelectPopup.Hide();
        }

        public void OpenShop()
        {
            _view.ClearItems();
            _allSlots.Clear();
            LoadItems();
            _view.Show();
        }

        public void CloseShop()
        {
            _detailPopup?.Hide();
            _memberSelectPopup?.Hide();
            _view.Hide();
            Bus<TutorialShopClosedEvent>.Raise(new TutorialShopClosedEvent());
        }

        private void LoadItems()
        {
            if (_itemListData == null || _itemListData.itemList == null)
            {
                Debug.LogWarning("[ShopPresenter] ItemListData is null");
                return;
            }

            int itemCount  = _itemListData.itemList.Count;
            int barsNeeded = Mathf.CeilToInt((float)itemCount / SLOTS_PER_BAR);

            for (int barIndex = 0; barIndex < barsNeeded; barIndex++)
            {
                var listBar = _view.CreateListBar();
                if (listBar == null) continue;

                var slots = listBar.GetSlots();

                for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
                {
                    int itemIndex = barIndex * SLOTS_PER_BAR + slotIndex;

                    if (itemIndex >= itemCount)
                    {
                        slots[slotIndex].gameObject.SetActive(false);
                        continue;
                    }

                    var itemData = _itemListData.itemList[itemIndex];
                    if (itemData == null)
                    {
                        slots[slotIndex].gameObject.SetActive(false);
                        continue;
                    }

                    slots[slotIndex].Setup(itemIndex, itemData.icon, itemData.itemName, itemData.itemPrice);
                    slots[slotIndex].OnSlotClicked += HandleSlotClicked;
                    slots[slotIndex].gameObject.SetActive(true);
                    _allSlots.Add(slots[slotIndex]);
                }
            }
        }

        private void HandleSlotClicked(ShopItemSlotView slot)
        {
            int itemIndex = slot.GetItemIndex();
            if (itemIndex < 0 || itemIndex >= _itemListData.itemList.Count) return;

            var itemData = _itemListData.itemList[itemIndex];
            if (itemData == null) return;

            _selectedItemIndex = itemIndex;
            _detailPopup?.Show(itemData.icon, itemData.itemName, itemData.itemDesc, itemData.itemPrice);
        }

        private void HandleBuyClicked()
        {
            if (_selectedItemIndex < 0 || _selectedItemIndex >= _itemListData.itemList.Count) return;

            var itemData = _itemListData.itemList[_selectedItemIndex];
            if (itemData == null) return;
            
            if (itemData.effectType == ShopItemEffectType.ActivityEfficiencyBonus
                && _purchasedItems.Contains(itemData.itemName))
            {
                Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                    SystemMessageIconType.Warning,
                    $"{itemData.itemName}은 이미 구매했습니다!"));
                return;
            }

            if (BandSupplyManager.Instance == null) return;

            int currentFunds = BandSupplyManager.Instance.BandFunds;
            if (currentFunds < itemData.itemPrice)
            {
                Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                    SystemMessageIconType.Warning,
                    $"자금이 부족합니다! (보유: {currentFunds}원, 필요: {itemData.itemPrice}원)"));
                return;
            }

            if (!BandSupplyManager.Instance.SpendBandFunds(itemData.itemPrice)) return;
            
            if (itemData.effectType == ShopItemEffectType.ActivityEfficiencyBonus)
                _purchasedItems.Add(itemData.itemName);

            _detailPopup?.Hide();
            _selectedItemIndex = -1;

            ApplyItemEffect(itemData);
            Bus<TutorialShopPurchasedEvent>.Raise(new TutorialShopPurchasedEvent());
        }

        private void ApplyItemEffect(ShopItemDataSO itemData)
        {
            switch (itemData.effectType)
            {
                case ShopItemEffectType.MemberStatIncrease:
                    ApplyStatIncrease(itemData);
                    break;

                case ShopItemEffectType.ActivityEfficiencyBonus:
                    ApplyEfficiencyBonus(itemData);
                    Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                        SystemMessageIconType.Warning, $"{itemData.itemName} 구매 완료!"));
                    break;

                case ShopItemEffectType.ConditionRecovery:
                    ApplyConditionRecovery(itemData);
                    Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                        SystemMessageIconType.Warning, $"{itemData.itemName} 구매 완료!"));
                    break;
            }
        }

        private void ApplyStatIncrease(ShopItemDataSO itemData)
        {
            if (_memberDataList == null || _memberDataList.Count == 0) return;

            if (itemData.grade == ShopItemGrade.High)
            {
                _memberSelectPopup?.Show(
                    _memberDataList,
                    itemData.targetStatType,
                    itemData.effectValue,
                    itemData.itemName);
            }
            else
            {
                var randomMember = _memberDataList[Random.Range(0, _memberDataList.Count)];
                GameStatManager.Instance?.AddMemberStatDirect(
                    randomMember.memberType,
                    itemData.targetStatType,
                    itemData.effectValue);

                Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                    SystemMessageIconType.Warning,
                    $"{itemData.itemName} 구매 완료! ({randomMember.memberName})"));
            }
        }

        private void ApplyEfficiencyBonus(ShopItemDataSO itemData)
        {
            GameStatManager.Instance?.AddActivityEfficiencyBonus(itemData.effectValue);
            Debug.Log($"[ShopPresenter] 행동 효율 +{itemData.effectValue}%");
        }

        private void ApplyConditionRecovery(ShopItemDataSO itemData)
        {
            if (_memberDataList == null || _memberDataList.Count == 0) return;
            if (MemberConditionManager.Instance == null) return;

            var worstMember = _memberDataList
                .OrderByDescending(m => (int)MemberConditionManager.Instance.GetCondition(m.memberType))
                .First();

            MemberConditionManager.Instance.UpCondition(worstMember.memberType, itemData.effectValue);
            Debug.Log($"[ShopPresenter] {worstMember.memberName} 컨디션 +{itemData.effectValue}단계 회복");
        }

        private void HandlePopupClosed()
        {
            _detailPopup?.Hide();
            _selectedItemIndex = -1;
        }

        private void HandleShopClosed() => CloseShop();

        public void Dispose()
        {
            _view.OnShopClosed -= HandleShopClosed;

            if (_detailPopup != null)
            {
                _detailPopup.OnBuyClicked   -= HandleBuyClicked;
                _detailPopup.OnCloseClicked -= HandlePopupClosed;
            }

            foreach (var slot in _allSlots)
                if (slot != null)
                    slot.OnSlotClicked -= HandleSlotClicked;

            _allSlots.Clear();
        }
    }
}
