using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.BandFunds
{
    public class BandExpUI : MonoBehaviour
    {
        [SerializeField] private Image bandfundsImage;
        [SerializeField] private TextMeshProUGUI bandExpText;
        [SerializeField] private Sprite bandfundsSprite;

        private void Awake()
        {
            bandfundsImage.sprite = bandfundsSprite;
        }

        private void Start()
        {
            RefreshText();
            BandSupplyManager.Instance.OnExpChanged += RefreshText;
        }

        private void OnDestroy()
        {
            if (BandSupplyManager.Instance != null)
                BandSupplyManager.Instance.OnExpChanged -= RefreshText;
        }

        private void RefreshText()
        {
            if (BandSupplyManager.Instance == null) return;
            bandExpText.SetText($"{BandSupplyManager.Instance.BandExp}");
        }
    }
}