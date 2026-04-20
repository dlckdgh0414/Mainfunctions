using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.Song.Director
{
    /// <summary>
    /// 연출부에서 총 조회수 얼마인지 잠깐 띄워주는 UI
    /// </summary>
    public class PlayCountShowUI : MonoBehaviour
    {
        [SerializeField] private GameObject playCountObj;
        [SerializeField] private GameObject textObj;
        [SerializeField] private TextMeshProUGUI playCountText;
        
        private void Awake()
        {
            Hide();
        }

        public void Hide()
        {
            playCountObj.SetActive(false);
            textObj.SetActive(false);
            playCountText.gameObject.SetActive(false);
        }
        
        public async UniTask ShowPlayCountText(int playCount)
        {
            playCountText.SetText("0회");
            playCountObj.SetActive(true);
            textObj.SetActive(true);
            await UniTask.WaitForSeconds(0.5f);
            playCountText.gameObject.SetActive(true);
            DOTween.To(() => playCount, x => playCount = x, playCount, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    playCountText.SetText(playCount + "회");
                })
                .SetLink(gameObject);
        }
    }
}