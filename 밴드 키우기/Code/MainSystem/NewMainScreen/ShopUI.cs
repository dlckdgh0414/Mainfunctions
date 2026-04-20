using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.Presenter;
using Code.MainSystem.NewMainScreen.MVP.View;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class ShopUI : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private ShopView shopView;

        [Header("Data")]
        [SerializeField] private ShopItemListSO shopItemListData;

        [Header("멤버 선택 팝업 (상 등급 아이템용)")]
        [SerializeField] private MemberSelectPopup memberSelectPopup;

        private ShopPresenter _shopPresenter;

        private void Start()
        {
            BuildPresenter();
        }

        private void BuildPresenter()
        {
            _shopPresenter = new ShopPresenter(
                shopView,
                shopItemListData,
                null,
                memberSelectPopup
            );
        }

        public void OpenShop()  => _shopPresenter?.OpenShop();
        public void CloseShop() => _shopPresenter?.CloseShop();

        private void OnDestroy() => _shopPresenter?.Dispose();
    }
}