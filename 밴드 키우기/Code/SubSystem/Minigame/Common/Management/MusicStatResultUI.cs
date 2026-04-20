using Code.MainSystem.Song;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.Common.Management
{
    public class MusicStatResultUI : MonoBehaviour
    {
        [SerializeField] private Image statIconImage;
        [SerializeField] private Image fillAmount;

        public void SetData(Sprite statIcon, int plusValue, int statMax)
        {
            statIconImage.sprite = statIcon;
            fillAmount.DOFillAmount((float)plusValue / statMax, 0.25f);
        }
    }
}