using System.Collections.Generic;
using System.Linq;

namespace Code.MainSystem.Dialogue.Editor.Graph
{
    /// <summary>
    /// 다이얼로그 노드 데이터 무결성을 검증하는 유틸리티
    /// </summary>
    public static class DialogueDataValidator
    {
        public sealed class ValidationResult
        {
            public List<string> errors = new List<string>();
            public HashSet<string> invalidNodeIds = new HashSet<string>();

            public bool HasErrors => errors.Count > 0;
        }

        public static ValidationResult Validate(
            List<DialogueNode> nodes,
            string[] characterIds,
            string[] backgroundIds)
        {
            ValidationResult result = new ValidationResult();
            if (nodes == null || nodes.Count == 0)
            {
                return result;
            }

            HashSet<string> nodeIds = new HashSet<string>(nodes.Select(node => node.NodeID));

            for (int i = 0; i < nodes.Count; i++)
            {
                DialogueNode node = nodes[i];
                bool hasError = false;

                if (characterIds != null && !characterIds.Contains(node.CharacterID) && !string.IsNullOrEmpty(node.CharacterID))
                {
                    result.errors.Add($"[{node.NodeID}] Character ID '{node.CharacterID}' not found.");
                    hasError = true;
                }

                if (backgroundIds != null && !backgroundIds.Contains(node.BackgroundID) && !string.IsNullOrEmpty(node.BackgroundID))
                {
                    result.errors.Add($"[{node.NodeID}] Background ID '{node.BackgroundID}' not found.");
                    hasError = true;
                }

                if (node.Choices != null)
                {
                    foreach (DialogueChoice choice in node.Choices)
                    {
                        if (!string.IsNullOrEmpty(choice.NextNodeID) && !nodeIds.Contains(choice.NextNodeID))
                        {
                            result.errors.Add($"[{node.NodeID}] Choice '{choice.ChoiceText}' leads to non-existent ID '{choice.NextNodeID}'.");
                            hasError = true;
                        }
                    }
                }

                if (hasError)
                {
                    result.invalidNodeIds.Add(node.NodeID);
                }
            }

            return result;
        }
    }
}
