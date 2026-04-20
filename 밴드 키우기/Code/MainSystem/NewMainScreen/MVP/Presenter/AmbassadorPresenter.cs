using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public class AmbassadorPresenter : IAmbassadorPresenter
    {
        private readonly IAmbassadorView _view;
        private MemberAmbassadorDataSO   _data;

        private bool _hasShownFirst = false;

        private const float RandomTriggerChance = 0.4f;

        public AmbassadorPresenter(IAmbassadorView view)
        {
            _view = view;
            _view.OnButtonClicked += HandleButtonClicked;
        }

        public void SetData(MemberAmbassadorDataSO data)
        {
            _data = data;
        }

        public void OnPhonePutAway()
        {
            if (_data == null || _data.AmbassadorDataList == null || _data.AmbassadorDataList.Count == 0)
                return;

            if (_view.IsPanelActive) return;

            bool shouldSpeak = !_hasShownFirst || Random.value < RandomTriggerChance;
            if (!shouldSpeak) return;

            _hasShownFirst = true;
            ShowRandomLine();
        }

        private void HandleButtonClicked()
        {
            if (_data == null) return;
            var lines = _data.AmbassadorDataList;
            if (lines == null || lines.Count == 0) return;

            if (_view.IsTyping) return;

            bool isActive = _view.IsPanelActive;
            _view.SetPanelVisible(!isActive);

            if (!isActive)
                ShowRandomLine();
            else
            {
                _view.StopTyping();
                _view.SetText("");
            }
        }

        private void ShowRandomLine()
        {
            var lines = _data.AmbassadorDataList;
            string line = lines[Random.Range(0, lines.Count)];
            _view.SetPanelVisible(true);
            _view.StartTyping(line, null);
        }

        public void Dispose()
        {
            _view.OnButtonClicked -= HandleButtonClicked;
        }
    }
}