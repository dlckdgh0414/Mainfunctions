using System.Collections.Generic;
using System.Threading.Tasks;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.Song;
using Code.SubSystem.Minigame.Common.Management;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.UI
{
    public class MusicStatBar : MonoBehaviour
    {
        [Header("SO")]
        [SerializeField] private MiniGameResultSenderSO senderSO;
        
        [Header("UI")]
        [SerializeField] private Image icon1;
        [SerializeField] private TextMeshProUGUI statNameText1;
        [SerializeField] private Image bar1;
        [SerializeField] private Image lastBar1;
        [SerializeField] private GameObject stat1Objects;
        
        [SerializeField] private Image icon2;
        [SerializeField] private TextMeshProUGUI statNameText2;
        [SerializeField] private Image bar2;
        [SerializeField] private Image lastBar2;
        [SerializeField] private GameObject stat2Objects;

        public async Task PlayUIAnimation()
        {
            var data1 = senderSO.ChangeMusicStats[0];
            stat1Objects.SetActive(true);
            
            var data2 = senderSO.ChangeMusicStats[1];
            stat2Objects.SetActive(true);

            lastBar1.fillAmount = 0;
            lastBar2.fillAmount = 0;
            bar1.fillAmount = 0;
            bar2.fillAmount = 0;
            
            // float maxStat1 = SongManager.Instance.GetMaxMusicStat(data1.Item1);
            // float currentVal1 = StatManager.Instance.GetMusicStat(data1.Item1).CurrentValue;
            // float targetVal1 = currentVal1 + data1.Item2;
            //
            // float lastBarTarget1 = currentVal1 / maxStat1;
            // float barTarget1 = targetVal1 / maxStat1;
            //
            // Sequence statSequence1 = DOTween.Sequence();
            //
            // statSequence1.Append(lastBar1.DOFillAmount(lastBarTarget1, 0.5f).SetEase(Ease.OutQuad)) // 첫 번째 바 채우기
            //     .Append(bar1.DOFillAmount(barTarget1, 0.5f).SetEase(Ease.OutQuad));
            //
            // float maxStat2 = SongManager.Instance.GetMaxMusicStat(data2.Item1);
            // float currentVal2 = StatManager.Instance.GetMusicStat(data2.Item1).CurrentValue;
            // float targetVal2 = currentVal2 + data2.Item2;
            //
            // float lastBarTarget2 = currentVal2 / maxStat2;
            // float barTarget2 = targetVal2 / maxStat2;
            //
            // Sequence statSequence2 = DOTween.Sequence();
            //
            // statSequence2.Append(lastBar2.DOFillAmount(lastBarTarget2, 0.5f).SetEase(Ease.OutQuad)) // 첫 번째 바 채우기
            //     .Append(bar2.DOFillAmount(barTarget2, 0.5f).SetEase(Ease.OutQuad));
            //
            // await statSequence1.Play().OnComplete(() => statSequence2.Play()).AsyncWaitForCompletion();
        }
    }
}