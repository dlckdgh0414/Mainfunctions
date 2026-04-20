using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser.Editor
{
    /// <summary>
    /// 다이얼로그 노드 자동 배치 계산을 담당하는 유틸리티
    /// </summary>
    public static class DialogueNodeLayoutUtility
    {
        private const float X_SPACING = 350f;
        private const float Y_SPACING = 200f;
        private const float DISCONNECTED_PADDING = 100f;

        public static void ApplyAutoLayout(List<DialogueNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            Dictionary<string, DialogueNode> nodeDict = nodes.ToDictionary(node => node.NodeID);
            HashSet<string> visited = new HashSet<string>();
            Dictionary<int, float> depthToMaxY = new Dictionary<int, float>();

            ArrangeNode(nodes[0].NodeID, 0, nodeDict, visited, depthToMaxY);
            PlaceDisconnectedNodes(nodes, nodeDict, visited, depthToMaxY);

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] = nodeDict[nodes[i].NodeID];
            }
        }

        private static void ArrangeNode(
            string currentId,
            int depth,
            Dictionary<string, DialogueNode> nodeDict,
            HashSet<string> visited,
            Dictionary<int, float> depthToMaxY)
        {
            if (string.IsNullOrEmpty(currentId) || visited.Contains(currentId) || !nodeDict.ContainsKey(currentId))
            {
                return;
            }

            visited.Add(currentId);
            DialogueNode currentNode = nodeDict[currentId];

            if (!depthToMaxY.ContainsKey(depth))
            {
                depthToMaxY[depth] = 0f;
            }

            float currentY = depthToMaxY[depth];
            currentNode.NodePosition = new Vector2(depth * X_SPACING, currentY);
            depthToMaxY[depth] = currentY + Y_SPACING;
            nodeDict[currentId] = currentNode;

            if (currentNode.Choices != null && currentNode.Choices.Count > 0)
            {
                foreach (DialogueChoice choice in currentNode.Choices)
                {
                    ArrangeNode(choice.NextNodeID, depth + 1, nodeDict, visited, depthToMaxY);
                }
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.NextNodeID))
            {
                ArrangeNode(currentNode.NextNodeID, depth + 1, nodeDict, visited, depthToMaxY);
            }
        }

        private static void PlaceDisconnectedNodes(
            List<DialogueNode> nodes,
            Dictionary<string, DialogueNode> nodeDict,
            HashSet<string> visited,
            Dictionary<int, float> depthToMaxY)
        {
            int disconnectedCount = 0;
            float baseY = depthToMaxY.GetValueOrDefault(0, 0f);

            foreach (DialogueNode node in nodes)
            {
                if (visited.Contains(node.NodeID))
                {
                    continue;
                }

                DialogueNode disconnectedNode = nodeDict[node.NodeID];
                disconnectedNode.NodePosition = new Vector2(0f, baseY + (disconnectedCount * Y_SPACING) + DISCONNECTED_PADDING);
                nodeDict[node.NodeID] = disconnectedNode;
                disconnectedCount++;
            }
        }
    }
}
