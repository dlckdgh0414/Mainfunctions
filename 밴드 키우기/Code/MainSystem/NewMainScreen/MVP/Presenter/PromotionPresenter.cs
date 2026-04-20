using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class PromotionPresenter
    {
        private readonly PromotionView _view;
        private readonly List<PromotionListSO> _pages;
        private int _currentPage = 0;

        public PromotionPresenter(PromotionView view, List<PromotionListSO> pages)
        {
            _view = view;
            _pages = pages;

            _view.OnBarClicked += HandleBarClicked;
            _view.OnPrevClicked += HandlePrevClicked;
            _view.OnNextClicked += HandleNextClicked;
        }

        public void OpenPromotion()
        {
            _currentPage = 0;
            LoadPage();
            _view.Show();
        }

        public void ClosePromotion()
        {
            _view.Hide();
            Bus<TutorialPromotionClosedEvent>.Raise(new TutorialPromotionClosedEvent());
        }

        private void LoadPage()
        {
            _view.ClearBars();

            if (_pages == null || _pages.Count == 0) return;

            var listData = _pages[_currentPage];
            if (listData == null || listData.promotionList == null) return;

            foreach (var data in listData.promotionList)
            {
                if (data == null) continue;
                var bar = _view.CreateBar();
                if (bar == null) continue;
                bar.Setup(data.promotionName, data.promotionPrice, data.addFans);
            }

            _view.SetPrevBtnInteractable(_currentPage > 0);
            _view.SetNextBtnInteractable(_currentPage < _pages.Count - 1);
        }

        private void HandlePrevClicked()
        {
            if (_currentPage <= 0) return;
            _currentPage--;
            LoadPage();
        }

        private void HandleNextClicked()
        {
            if (_pages == null || _currentPage >= _pages.Count - 1) return;
            _currentPage++;
            LoadPage();
        }

        private void HandleBarClicked(PromotionListBar bar)
        {
            if (BandSupplyManager.Instance == null) return;

            int currentFunds = BandSupplyManager.Instance.BandFunds;

            if (currentFunds < bar.PromotionPrice)
            {
                Bus<SystemMessageEvent>.Raise(
                    new SystemMessageEvent(SystemMessageIconType.Warning,
                        $"자금이 부족합니다! (보유: {currentFunds}원, 필요: {bar.PromotionPrice}원)")
                );
                return;
            }

            BandSupplyManager.Instance.AddBandFunds(-bar.PromotionPrice);
            BandSupplyManager.Instance.AddBandFans(bar.AddFans);
            Debug.Log($"[PromotionPresenter] {bar.PromotionName} 완료, 팬 +{bar.AddFans}");
            Bus<SystemMessageEvent>.Raise(
                new SystemMessageEvent(SystemMessageIconType.Warning,
                    $"홍보 완료 (팬 +{bar.AddFans})")
            );
        }

        public void Dispose()
        {
            _view.OnBarClicked -= HandleBarClicked;
            _view.OnPrevClicked -= HandlePrevClicked;
            _view.OnNextClicked -= HandleNextClicked;
        }
    }
}
