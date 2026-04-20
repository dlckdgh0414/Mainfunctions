using Code.SubSystem.UI;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI tmiText;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private LoadingTmiSO loadingTmiSO;

        private const string LoadingStr = "Loading...";

        public void Show()
        {
            loadingPanel.SetActive(true);

            if (tmiText != null && loadingTmiSO != null)
                tmiText.SetText(loadingTmiSO.GetRandom());

            if (loadingText != null)
                loadingText.SetText(LoadingStr);
        }

        public void Hide() => loadingPanel.SetActive(false);
    }
}