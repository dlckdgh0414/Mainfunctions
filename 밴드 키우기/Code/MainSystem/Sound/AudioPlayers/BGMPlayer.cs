using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SoundEvents;
using UnityEngine;

namespace Code.MainSystem.Sound.AudioPlayers
{
    public class BGMPlayer : MonoBehaviour
    {
        [SerializeField] private SoundSO sound;
        
        private void Start()
        {
            Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(sound));
        }
    }
}