using System.Threading.Tasks;
using Chuh007Lib.ObjectPool.RunTime;
using Code.Core.Addressable;
using Code.Core.Bus.GameEvents.MiniGameEvent;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.RhythmQTE;
using Code.MainSystem.Song;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.Minigame.Common.Contexts;
using Code.SubSystem.Minigame.Common.Management;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    /// <summary>
    /// QTE 관리해줄 컴포넌트
    /// QTE 씬에 있음
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class QTEController : BaseMiniGame
    {
        [Header("Data Setting")]
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO qteItem;
        [SerializeField] private Transform qteParent;
        [SerializeField] private float qteObjectLifeTime;
        [SerializeField] private ProgressController progressController;
        
        [Header("Song Data")]
        // [SerializeField] private CompletedSongDataSO dataSO;
        
        private AudioSource _musicSource;
        
        protected async void Start()
        {
            progressController.SetProgress(0);
            progressController.ProgressMax += GameEnd;
            _musicSource = GetComponent<AudioSource>();
            // if (!dataSO.songKey.IsValid())
            // {
            //     var audio = await Addressables.LoadAssetAsync<AudioClip>(dataSO.songKey.RuntimeKey).Task;
            //     _musicSource.clip = audio;
            // }
            // else
            // {
            //     _musicSource.clip = dataSO.songKey.Asset as AudioClip;
            // }
            _isPlaying = true;
            
            Init();
        }

        protected override void HandleStart(StartCountingEndEvent evt)
        {
            base.HandleStart(evt);
            StartMusic();
        }
        
        public void StartMusic()
        {
            _musicSource.Play();
            QTELoop();
        }

        private async void QTELoop()
        {
            while (_isPlaying)
            {
                Vector2 spawnPos = GetRandomSafeRectPosition();
                QTEObject qteObj = poolManagerSO.Pop(qteItem) as QTEObject;
                Debug.Assert(qteObj != null, $"casting error");
                qteObj.transform.SetParent(qteParent);
                qteObj.Initailize(this, qteObjectLifeTime);
                qteObj.gameObject.transform.position = spawnPos;
                await Awaitable.WaitForSecondsAsync(1.0f);
            }
        }

        private Vector2 GetRandomSafeRectPosition()
        {
            Rect rect = Screen.safeArea;
            float x = Random.Range(rect.x + 50, rect.x + rect.width - 50);
            float y = Random.Range(rect.y + 500, rect.y + rect.height - 250);
            
            return new Vector2(x, y);
        }
        
        public void OnQTEPressSucceed()
        {
            if(!_isPlaying) return;
            progressController.AddProgress(5.0f);
        }
    }
}