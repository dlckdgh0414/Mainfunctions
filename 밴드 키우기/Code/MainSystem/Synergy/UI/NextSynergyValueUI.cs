using TMPro;
using UnityEngine;

namespace Code.MainSystem.Synergy.UI
{
    public class NextSynergyValueUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        public void EnableFor(int value, bool isActive)
        {
            valueText.text = value.ToString();
            valueText.color = isActive ? activeColor : inactiveColor;
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}