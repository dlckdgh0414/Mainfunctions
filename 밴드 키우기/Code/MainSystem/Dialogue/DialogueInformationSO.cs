using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 하나의 다이알로그 시퀀스 데이터를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueInformation", menuName = "SO/Dialogue/Information", order = 20)]
    public class DialogueInformationSO : ScriptableObject
    {
        [SerializeField] private string startNodeID;
        [SerializeField] private List<DialogueNode> dialogueNodes;

        public string StartNodeID => startNodeID;
        public List<DialogueNode> DialogueNodes => dialogueNodes;

        public void SetNodes(List<DialogueNode> nodes)
        {
            dialogueNodes = nodes;
        }

        public void SetStartNode(string nodeID)
        {
            startNodeID = nodeID;
        }
    }
}
