using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using UnityEngine;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 다이알로그 UI의 최상위 활성화 상태를 제어하는 매니저 클래스
    /// 실제 연출은 하위 Presenter들이 담당함
    /// </summary>
    public class DialogueUIManager : MonoBehaviour
    {
        [Header("UI Root")]
        [SerializeField] private GameObject dialogueUiParent;

        private void Awake()
        {
            if (dialogueUiParent != null)
            {
                dialogueUiParent.SetActive(false);
            }
        }

        private void OnEnable()
        {
            Bus<DialogueStartEvent>.OnEvent += OnDialogueStart;
            Bus<DialogueEndEvent>.OnEvent += OnDialogueEnd;
        }

        private void OnDisable()
        {
            Bus<DialogueStartEvent>.OnEvent -= OnDialogueStart;
            Bus<DialogueEndEvent>.OnEvent -= OnDialogueEnd;
        }

        private void OnDialogueStart(DialogueStartEvent obj)
        {
            if (dialogueUiParent != null)
            {
                dialogueUiParent.SetActive(true);
            }
        }

        private void OnDialogueEnd(DialogueEndEvent e)
        {
            if (dialogueUiParent != null)
            {
                dialogueUiParent.SetActive(false);
            }
        }
    }
}
