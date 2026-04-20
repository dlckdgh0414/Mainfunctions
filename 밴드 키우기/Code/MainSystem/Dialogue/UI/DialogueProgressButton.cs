using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Dialogue.UI
{
    [RequireComponent(typeof(Button))]
    public class DialogueProgressButton : MonoBehaviour
    {
        private Button _progressButton;

        private void Awake()
        {
            _progressButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _progressButton.onClick.AddListener(ProgressDialogue);
        }

        private void OnDisable()
        {
            _progressButton.onClick.RemoveListener(ProgressDialogue);
        }

        private void ProgressDialogue()
        {
            Bus<UIContinueButtonPressedEvent>.Raise(new UIContinueButtonPressedEvent());
        }
    }
}
