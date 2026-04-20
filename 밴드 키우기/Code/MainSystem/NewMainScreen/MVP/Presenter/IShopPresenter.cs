using System;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public interface IShopPresenter : IDisposable
    {
        void OpenShop();
        void CloseShop();
    }
}