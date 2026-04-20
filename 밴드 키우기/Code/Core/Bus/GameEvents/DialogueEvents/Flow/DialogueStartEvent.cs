using Code.MainSystem.Dialogue;
using Code.MainSystem.Dialogue.Parser;

namespace Code.Core.Bus.GameEvents.DialogueEvents.Flow
{
    public struct DialogueStartEvent : IEvent
    {
        public readonly DialogueInformationSO DialogueSO;
        public readonly DialogueVariableContext VariableContext;

        public DialogueStartEvent(DialogueInformationSO dialogueSO, DialogueVariableContext variableContext = null)
        {
            DialogueSO = dialogueSO;
            VariableContext = variableContext;
        }
    }
}
