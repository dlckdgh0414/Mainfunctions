using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Test
{
    public class DialogueTester : MonoBehaviour
    {
        [SerializeField] private DialogueInformationSO dialogueInformationSO;


        private void Start()
        {
            Bus<DialogueStartEvent>.Raise(new DialogueStartEvent(dialogueInformationSO));
        }

        private void OnEnable()
        {
            Bus<DialogueEndEvent>.OnEvent += HandleProgressScene;
        }

        private void HandleProgressScene(DialogueEndEvent evt)
        {
            Bus<FadeSceneEvent>.Raise(new FadeSceneEvent("Tutor"));
        }
    }
}