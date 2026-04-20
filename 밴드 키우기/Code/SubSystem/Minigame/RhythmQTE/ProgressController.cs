using System;
using DG.Tweening;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    /// <summary>
    /// 미니게임에서 진행도 관리로 사용할 컨트롤러.
    /// QTE 기준으로 만들었고, 다른 곳에서 사용 가능하다면 사용 요망.
    /// </summary>
    public class ProgressController : MonoBehaviour
    {
        [SerializeField] private float maxProgress = 100;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressImageBar;
        
        public event Action ProgressMax;
        
        private float _currentProgress;
        
        // 지금 진행도를 특정 값으로 변경
        public void SetProgress(float progress)
        {
            _currentProgress = progress;
            ProgressChanged();
        }
        
        // 진행도를 증가시킴(- 넣으면 감소)
        public void AddProgress(float value)
        {
            _currentProgress += value;
            ProgressChanged();
        }

        public void ProgressChanged()
        {
            float targetRatio = _currentProgress / maxProgress > 1 ? 1 : _currentProgress / maxProgress;

            DOTween.To(() => progressImageBar.fillAmount, x =>
                {
                    progressImageBar.fillAmount = x;
                    progressText.SetText($"{(x * 100f):F0}%");
                }
                , targetRatio, 0.25f);
            
            if(_currentProgress >= maxProgress)
                ProgressMax?.Invoke();
        }
    }
}