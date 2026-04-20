using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.UI
{
    public class PersonalStatBar : MonoBehaviour
    {
        [SerializeField] private Image memberIcon;
        [SerializeField] private Image statIcon;
        [SerializeField] private Image barFill;
        [SerializeField] private Image lastBarFill;
        [SerializeField] private TextMeshProUGUI percentText;

        public async Task SetDataAndAnim(Sprite member, Sprite stat, float bar, float last)
        {
            memberIcon.sprite = member;
            statIcon.sprite = stat;
            barFill.fillAmount = 0;
            lastBarFill.fillAmount = 0;
            Sequence sequence = DOTween.Sequence();
            
            await sequence.Append(lastBarFill.DOFillAmount(last, 0.5f).SetEase(Ease.OutQuad)) // 첫 번째 바 채우기
                .Append(barFill.DOFillAmount(bar, 0.5f).SetEase(Ease.OutQuad)).AsyncWaitForCompletion();
            percentText.SetText(bar + "%");
        }
        
    }
}