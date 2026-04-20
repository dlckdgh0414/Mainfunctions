using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    [RequireComponent(typeof(Button))]
    public class DialogueSkipButton : MonoBehaviour
    {
        private Button _skipButton;

        private void Awake()
        {
            _skipButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(SkipDialogue);
        }

        private void OnDisable()
        {
            _skipButton.onClick.RemoveListener(SkipDialogue);
        }

        private void SkipDialogue()
        {
            Bus<DialogueSkipEvent>.Raise(new DialogueSkipEvent());
        }
    }
}
