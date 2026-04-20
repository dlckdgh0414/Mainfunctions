using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.Common.Management
{
    public class MemberResultUI : MonoBehaviour
    {
        [SerializeField] private Image memberIconImage;
        [SerializeField] private Image statIconImage;
        [SerializeField] private TextMeshProUGUI plusText;

        public void SetData(Sprite memberIcon, Sprite statIcon, int plusValue)
        {
            memberIconImage.sprite = memberIcon;
            statIconImage.sprite = statIcon;
            plusText.SetText(plusValue.ToString());
        }
    }
}