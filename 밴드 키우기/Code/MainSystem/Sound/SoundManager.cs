using System;
using Chuh007Lib.ObjectPool.RunTime;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;
using Code.Core.Bus.GameEvents.SoundEvents;
using UnityEngine;

namespace Code.MainSystem.Sound
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO soundItem;
        
        private void Awake()
        {
            Bus<PlaySoundEvent>.OnEvent += HandlePlaySound;
        }
        private void OnDestroy()
        {
            Bus<PlaySoundEvent>.OnEvent -= HandlePlaySound;
        }
        
        private void HandlePlaySound(PlaySoundEvent evt)
        {
            SoundPlayer sound = poolManager.Pop(soundItem) as SoundPlayer;
            Debug.Assert(sound != null, $"soundItem is Not SoundPlayer");
            sound.PlaySound(evt.Sound);
        }
        
    }
}