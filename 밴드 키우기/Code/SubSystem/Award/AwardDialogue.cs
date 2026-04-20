using UnityEngine;
using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.MainSystem.Dialogue;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Code.SubSystem.Award
{
    [Serializable]
    public class AwardDialogueData
    {
        public AwardType awardType;
        public AssetReference dialogue;
    }

    public class AwardDialogue : MonoBehaviour
    {
        [SerializeField] private List<AwardDialogueData> dialogueDataList;
        [SerializeField] private string sceneName;

        private readonly Dictionary<AwardType, DialogueInformationSO> _cachedDialogues = new();
        private AwardDialogueEvent? _pendingEvent = null;

        private void Awake()
        {
            LoadAllAsync().Forget();
            Bus<AwardDialogueEvent>.OnEvent += HandleDialoguePlay;
            Bus<DialogueEndEvent>.OnEvent += HandleDialogueEnd;

        }

        private void HandleDialogueEnd(DialogueEndEvent evt)
        {
             Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(sceneName));
        }

        private void HandleDialoguePlay(AwardDialogueEvent evt)
        {
            foreach (var data in dialogueDataList)
            {
                if (data.awardType == evt.AwardType && _cachedDialogues.ContainsKey(data.awardType))
                {
                    Bus<DialogueStartEvent>.Raise(new DialogueStartEvent(_cachedDialogues[data.awardType]));
                    return;
                }
            }
            _pendingEvent = evt;
        }

        private async UniTaskVoid LoadAllAsync()
        {
            foreach (var data in dialogueDataList)
            {
                if (data.dialogue == null || !data.dialogue.RuntimeKeyIsValid()) continue;

                try
                {
                    var so = await data.dialogue.LoadAssetAsync<DialogueInformationSO>().ToUniTask();
                    if (so != null)
                        _cachedDialogues[data.awardType] = so;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AwardDialogue] {data.awardType} 로드 실패: {e.Message}");
                }
            }

            if (_pendingEvent.HasValue)
            {
                HandleDialoguePlay(_pendingEvent.Value);
                _pendingEvent = null;
            }
        }

        private void OnDestroy()
        {
            foreach (var data in dialogueDataList)
            {
                if (data.dialogue != null && data.dialogue.IsValid())
                    data.dialogue.ReleaseAsset();
            }
            _cachedDialogues.Clear();
            Bus<AwardDialogueEvent>.OnEvent -= HandleDialoguePlay;
            Bus<DialogueEndEvent>.OnEvent -= HandleDialogueEnd;
        }
    }
}