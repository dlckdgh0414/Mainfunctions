using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.BandFunds
{
    public class BandFanUI : MonoBehaviour
    {
        [SerializeField] private Image bandFanImage;
        [SerializeField] private TextMeshProUGUI bandFanText;
        [SerializeField] private Sprite bandFanSprite;

        private void Awake()
        {
            if (bandFanImage != null && bandFanSprite != null)
                bandFanImage.sprite = bandFanSprite;
        }

        private void Start()
        {
            RefreshText();
            BandSupplyManager.Instance.OnFansChanged += RefreshText;
        }

        private void OnDestroy()
        {
            if (BandSupplyManager.Instance != null)
                BandSupplyManager.Instance.OnFansChanged -= RefreshText;
        }

        private void RefreshText()
        {
            if (BandSupplyManager.Instance == null) return;
            bandFanText.SetText($"{BandSupplyManager.Instance.BandFans}");
        }
    }
}