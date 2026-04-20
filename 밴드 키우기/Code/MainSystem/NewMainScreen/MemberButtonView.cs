using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class MemberButtonView : MonoBehaviour
    {
        [SerializeField] private Image             backgroundImage;
        [SerializeField] private Image             iconImage;

        public void Setup(string memberName, Sprite icon, Color personalColor)
        {
            if (backgroundImage != null) backgroundImage.color = personalColor;

            if (iconImage != null)
            {
                iconImage.sprite  = icon;
                iconImage.enabled = icon != null;
            }
        }
    }
}