using TMPro;
using UnityEngine;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitOverflowPanel : MonoBehaviour, IUIElement<int, int>
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI pointInfoText;
        [SerializeField] private GameObject panel;

        [Header("Settings")]
        [SerializeField] private string overflowMessageFormat = "특성 포인트가 한도를 초과했습니다.\n기존 특성을 제거해주세요.";
        [SerializeField] private string pointInfoFormat = "현재: {0}P / 최대: {1}P (초과: {2}P)";

        public void EnableFor(int currentPoint, int maxPoint)
        {
            UpdateMessage(currentPoint, maxPoint);
            Show();
        }

        public void Disable()
        {
            Hide();
        }

        private void UpdateMessage(int currentPoint, int maxPoint)
        {
            messageText?.SetText(overflowMessageFormat);

            if (pointInfoText is null) 
                return;
            
            int overflow = currentPoint - maxPoint;
            string info = string.Format(pointInfoFormat, currentPoint, maxPoint, overflow);
            pointInfoText.SetText(info);
            pointInfoText.color = Color.red;
        }

        private void Show()
        {
            if (panel is not null)
                panel.SetActive(true);
            else
                gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (panel is not null)
                panel.SetActive(false);
            else
                gameObject.SetActive(false);
        }

        public void Close()
        {
            Hide();
        }
    }
}