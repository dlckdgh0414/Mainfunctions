using System;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 대사 텍스트 축소 연출 명령어
    /// </summary>
    [Serializable]
    public class ShrinkTextCommand : IDialogueCommand
    {
        public float startScale;
        public float endScale;

        public void Execute()
        {
        }
    }
}
