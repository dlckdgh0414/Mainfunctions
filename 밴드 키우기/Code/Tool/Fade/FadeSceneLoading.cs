using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.SubSystem.Save;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Code.Tool.Fade
{
    public enum FadeImageType
    {
        Random, // 무작위
        Guitar,
        Drums,
        Bass,
        Vocal,
        Piano
    }

    [Serializable]
    public struct FadeImageData
    {
        public FadeImageType type;
        public Sprite image;
    }
    
    /// <summary>
    /// 씬 로딩시 페이드 인아웃 적용해주는 컴포넌트.
    /// 이게 씬에 있으면 진입시 페이드 아웃 됨.
    /// </summary>
    public class FadeSceneLoading : MonoBehaviour
    {
        [SerializeField] private Image fadeImage; // 여기 이미지에 넣을 스프라이트 MeshType를 Full Rect로 설정해야함
        [SerializeField] private float fadeOutDuration = 1.0f; // 씬 로딩시 페이드아웃까지 걸리는 시간
        [SerializeField] private List<FadeImageData> dataList; // 이 리스트에 삽입할때 Random을 키로 넣지 마셈
        [SerializeField] private bool isFadeOut = true;
        
        private static Sprite LastFadeImage = null;
        
        private readonly int _sizeHash = Shader.PropertyToID("_ImageSize");
        
        private const int fadeInEndValue = 250;
        private const int fadeOutEndValue = 0;
        
        private Dictionary<FadeImageType, Sprite> _imageDict = new Dictionary<FadeImageType, Sprite>();
        
        private void Awake()
        {
            fadeImage.material = new Material(fadeImage.material);

            foreach (var data in dataList)
            {
                _imageDict.Add(data.type, data.image);
            }
            
            Bus<FadeSceneEvent>.OnEvent += HandleLoadSceneToFadeIn;
        }
        
        private void Start()
        {
            if (!isFadeOut)
            {
                fadeImage.raycastTarget = false;
                return;
            };
            FadeOut();
        }
        
        private void OnDestroy()
        {
            Bus<FadeSceneEvent>.OnEvent -= HandleLoadSceneToFadeIn;
        }

        private void FadeOut()
        {
            if (LastFadeImage)
            {
                fadeImage.sprite = LastFadeImage;
                LastFadeImage = null;
            }
            fadeImage.material.SetFloat(_sizeHash, fadeInEndValue);
            fadeImage.raycastTarget = true;

            fadeImage.material.DOFloat(fadeOutEndValue, _sizeHash, fadeOutDuration)
                .SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    fadeImage.raycastTarget = false;
                });
        }
        
        private void HandleLoadSceneToFadeIn(FadeSceneEvent evt)
        {
            Sprite fadeSprite;
            if (evt.Type == FadeImageType.Random)
            {
                fadeSprite = GetRandomImage();
            }
            else fadeSprite = _imageDict[evt.Type];
            
            LastFadeImage = fadeSprite;
            
            fadeImage.sprite = fadeSprite;
            fadeImage.material.SetFloat(_sizeHash, fadeOutEndValue);
            fadeImage.raycastTarget = true;
            
            SaveManager.Instance?.SaveNow();
            
            fadeImage.material.DOFloat(fadeInEndValue, _sizeHash, evt.Duration)
                .SetEase(Ease.InExpo).OnComplete(() =>
            {
                if(evt.SceneName != string.Empty) 
                    SceneManager.LoadScene(evt.SceneName, evt.IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            });
        }

        private Sprite GetRandomImage()
        {
            int idx = Random.Range(0, dataList.Count);
            return dataList[idx].image;
        }
    }
}
