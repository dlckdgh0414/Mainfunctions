using System;
using Chuh007Lib.ObjectPool.RunTime;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;
using UnityEngine;
using UnityEngine.Audio;
using Work.CHUH.Chuh007Lib.ObjectPool.RunTime;
using Random = UnityEngine.Random;

namespace Code.MainSystem.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour, IPoolable
    {
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup bgmGroup;

        private SoundSO.AudioTypes _type;
        private AudioSource _audioSource;
        private Pool _myPool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Bus<BGMStopEvnet>.OnEvent += HandleBGMStop;
            Bus<PlayBGMEvent>.OnEvent += HandlePlayBGM;
        }

        private void OnDestroy()
        {
            Bus<BGMStopEvnet>.OnEvent -= HandleBGMStop;
            Bus<PlayBGMEvent>.OnEvent -= HandlePlayBGM;
        }

        public void SetUpPool(Pool pool) => _myPool = pool;
        
        public void ResetItem()
        { }
        
        public void PlaySound(SoundSO data)
        {
            _type = data.audioType;
            
            _audioSource.outputAudioMixerGroup = data.audioType switch
            {
                SoundSO.AudioTypes.SFX => sfxGroup,
                SoundSO.AudioTypes.MUSIC => bgmGroup,
                _ => sfxGroup
            };
            
            _audioSource.volume = data.volume;
            _audioSource.pitch = data.pitch;

            if (data.randomizePitch)
            {
                _audioSource.pitch += Random.Range(-data.randomPitchModifier, data.randomPitchModifier);
            }
            _audioSource.clip = data.clip;
            _audioSource.loop = data.loop;

            if (!data.loop)
            {
                float duration = _audioSource.clip.length + 0.2f;
                DisableSound(duration);
            }

            _audioSource.Play();
        }
        
        private async void DisableSound(float duration)
        {
            await Awaitable.WaitForSecondsAsync(duration);
            _myPool.Push(this);
        }
        
        public void StopAndGotoPool()
        {
            _audioSource.Stop();
            _myPool.Push(this);
        }
        
        private void HandleBGMStop(BGMStopEvnet evt)
        {
            if (_type == SoundSO.AudioTypes.SFX) return;
            _audioSource?.Pause();
        }

        private void HandlePlayBGM(PlayBGMEvent evt)
        {
            if (_type == SoundSO.AudioTypes.SFX) return;
            _audioSource?.UnPause();
        }
    }
}