using System.Collections.Generic;
using Code.MainSystem.MusicProduction.Data;
using UnityEngine;
using System;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.MainSystem.Sound;
using UnityEngine.UI;

namespace Code.MainSystem.MusicProduction
{
    [Serializable]
    public class MusicDirectionListData
    {
        public Button musicDirectionButton;
        public string musicDirectionName;
        public MusicDirectionType musicDirectionType;
    }

    public class MusicDirectionUI : MonoBehaviour
    {
        [SerializeField] private List<MusicDirectionListData> musicDirectionListData;
        
        [Header("Sound")]
        [SerializeField] private SoundSO clickSound;
        
        public Action<MusicDirectionListData> OnChangeMusicDirection;
        
        public Action OnHide;

        private void Awake()
        {
            foreach (var data in musicDirectionListData)
            {
                if (data == null || data.musicDirectionButton == null) continue;
                var captured = data;
                data.musicDirectionButton.onClick.AddListener(() =>
                {
                    Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(clickSound));
                    OnChangeMusicDirection?.Invoke(captured);
                    HideUI();
                });
            }
        }

        public void ShowUI() => gameObject.SetActive(true);

        public void HideUI()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var data in musicDirectionListData)
                data?.musicDirectionButton?.onClick.RemoveAllListeners();
        }
    }
}