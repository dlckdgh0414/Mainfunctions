using System;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public interface IShopView
    {
        event Action OnShopClosed;
        
        void Show();
        void Hide();
        void ClearItems();
    }
}