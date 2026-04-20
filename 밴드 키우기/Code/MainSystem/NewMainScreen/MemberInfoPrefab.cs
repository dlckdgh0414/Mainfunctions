using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class MemberInfoPrefab : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;

        public void SetUP(Sprite icon, string name)
        {
            this.icon.sprite = icon;
            this.nameText.text = name;
        }
    }
}