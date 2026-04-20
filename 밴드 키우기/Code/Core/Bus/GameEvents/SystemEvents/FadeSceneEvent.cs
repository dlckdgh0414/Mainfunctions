using Code.Tool;
using Code.Tool.Fade;
using UnityEngine;

namespace Code.Core.Bus.GameEvents.SystemEvents
{
    public struct FadeSceneEvent : IEvent
    {
        public string SceneName;
        public float Duration;
        public FadeImageType Type;
        public bool IsAdditive;
        
        /// <param name="sceneName">로딩할 씬 이름(Scene List에 있어야함)</param>
        /// <param name="duration">페이드 인 지속시간</param>
        /// <param name="type">페이드인에 쓰일 이미지</param>
        /// <param name="isAdditive">Additive로 씬 로드할지(앵간하면 ㄴㄴ)</param>
        public FadeSceneEvent(string sceneName, float duration = 0.5f, FadeImageType type = FadeImageType.Random, bool isAdditive = false)
        {
            SceneName = sceneName;
            Duration = duration;
            Type = type;
            IsAdditive = isAdditive;
        }
    }
}