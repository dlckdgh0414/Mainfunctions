using Code.MainSystem.Dialogue;

namespace Code.Core.Bus.GameEvents.CutsceneEvents
{
    /// <summary>
    /// 다이얼로그를 넣어 보내주면 그 다이얼로그를 출력해줌(즉시)
    /// </summary>
    public struct DialogCutscenePlayEvent : IEvent
    {
        public DialogueInformationSO Dialogue;

        public DialogCutscenePlayEvent(DialogueInformationSO dialogue)
        {
            Dialogue = dialogue;
        }
    }
}