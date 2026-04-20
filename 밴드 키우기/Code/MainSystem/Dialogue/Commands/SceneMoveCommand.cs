using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using UnityEngine.SceneManagement;

namespace Code.MainSystem.Dialogue.Commands
{
    [Serializable]
    public class SceneMoveCommand : IDialogueCommand
    {
        public void Execute()
        {
            //Bus<FadeSceneEvent>.Raise(new FadeSceneEvent("Tutor"));
        }
    }
}