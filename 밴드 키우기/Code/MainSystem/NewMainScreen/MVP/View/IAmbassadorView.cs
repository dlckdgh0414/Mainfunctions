using System;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public interface IAmbassadorView
    {
        event Action OnButtonClicked;

        bool IsPanelActive { get; }
        bool IsTyping { get; } 
        void SetPanelVisible(bool visible);
        void SetText(string text);
        void StartTyping(string text, Action onComplete);
        void StopTyping();
    }
}
