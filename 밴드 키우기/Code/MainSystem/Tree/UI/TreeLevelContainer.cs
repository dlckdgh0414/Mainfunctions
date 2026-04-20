using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Tree.UI
{
    /// <summary>
    /// 세로배치용 컨테이너. 자식으로 TreeNodeObject가 있다.
    /// </summary>
    public class TreeLevelContainer : MonoBehaviour
    {
        [SerializeField] private TreeNodeObject nodePrefab;
        // 이 레벨에 넣을 노드들
        private List<TreeNodeDataSO> _levelNodes;

        public List<TreeNodeObject> HaveNodes { get; } = new();
        
        public void SetupData(IList<string> levelNodes)
        {
            
            foreach (var nodeId in levelNodes)
            {
                TreeNodeObject node = Instantiate(nodePrefab, transform);
                node.SetupData(TreeDataManager.Instance.AllNodes[nodeId]);
                HaveNodes.Add(node);
            }
        }
    }
}