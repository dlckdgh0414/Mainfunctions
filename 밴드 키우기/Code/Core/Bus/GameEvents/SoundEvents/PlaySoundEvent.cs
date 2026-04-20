using Code.MainSystem.Sound;
using UnityEngine;

namespace Code.Core.Bus.GameEvents.SoundEvents
{
    public struct PlaySoundEvent : IEvent
    {
        public SoundSO Sound;

        public PlaySoundEvent(SoundSO sound)
        {
            Sound = sound;
        }
    }
}