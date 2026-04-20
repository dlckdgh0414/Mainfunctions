using System;
using System.Collections.Generic;
using System.Text;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.MainSystem.EventManager.RandomEvent;
using Code.MainSystem.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager
{
    public class TextEventUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeBtn;

        [Header("Sound")]
        [SerializeField] private SoundSO popUpSound;
        
        private Action _onClosed;

        private const string PositiveColor = "#3AAB57";
        private const string NegativeColor = "#D13B3B";

        private void Awake()
        {
            closeBtn.onClick.AddListener(HandleClosePanel);
            panel.SetActive(false);
        }

        public void ShowEvent(string eventText, int eventAmount, Action onClosed = null)
        {
            panel.SetActive(true);
            mainText.text = eventText;
            if (effectText != null) effectText.text = string.Empty;
            _onClosed = onClosed;
        }

        public void ShowEvent(string eventText, List<RandomEventEffect> effects, Action onClosed = null)
        {
            panel.SetActive(true);
            Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(popUpSound));
            mainText.text = eventText;
            if (effectText != null) effectText.text = BuildEffectText(effects);
            _onClosed = onClosed;
        }

        private string BuildEffectText(List<RandomEventEffect> effects)
        {
            if (effects == null || effects.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var e in effects)
            {
                if (e.amount == 0) continue;

                string line = FormatEffect(e);
                if (string.IsNullOrEmpty(line)) continue;

                sb.Append(line);
                sb.Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        private string FormatEffect(RandomEventEffect e)
        {
            bool positive = e.amount > 0;
            string color  = positive ? PositiveColor : NegativeColor;
            string sign   = positive ? "+" : "";

            switch (e.type)
            {
                case RandomEventEffectType.AddFunds:
                    return $"<color={color}>자금 {sign}{e.amount} G</color>";

                case RandomEventEffectType.AddFans:
                    return $"<color={color}>팬 {sign}{e.amount}명</color>";

                case RandomEventEffectType.AddExp:
                    return $"<color={color}>경험치 {sign}{e.amount}</color>";

                case RandomEventEffectType.AddConditionAll:
                    return $"<color={color}>전원 컨디션 {sign}{e.amount}</color>";

                default:
                    return string.Empty;
            }
        }

        private void HandleClosePanel()
        {
            panel.SetActive(false);

            var callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }

        private void OnDestroy()
        {
            closeBtn.onClick.RemoveAllListeners();
        }
    }
}