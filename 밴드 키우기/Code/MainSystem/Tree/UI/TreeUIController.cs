using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.MainSystem.Tree.UI
{
    /// <summary>
    /// Tree가 보여지는것을 전담함
    /// 씬 로딩될 때 오브젝트들 생성하고, 이후로는 껐다켰다 하면서 데이터 변경
    /// </summary>
    public class TreeUIController : MonoBehaviour
    {
        [SerializeField] private Transform treeRoot;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TreeLevelContainer treeLevelContainerPrefab;
        [SerializeField] private UILineRenderer treeUiLinePrefab;
        [SerializeField] private TreeNodeDataSO defaultNodeObject;

        [Header("Line Settings")]
        [SerializeField] private float lineStartOffset = 100f;
        [SerializeField] private float lineEndOffset = 100f;
        
        // 레벨별로 쪼개서 각 레벨별 노드 아이디가 들어간다.
        private List<List<string>> _levelNodeIdList = new();
        private Dictionary<string, TreeNodeObject> _nodeObjects = new();

        private bool _isInitialized = false;
        
        private void Awake()
        {
            Bus<NodeSelectEvent>.OnEvent += HandleTreeNodeSelect;
        }

        private void OnDestroy()
        {
            Bus<NodeSelectEvent>.OnEvent -= HandleTreeNodeSelect;
        }

        private void OnEnable()
        {
            OpenUI();
        }

        // UI상에 배치될때 할 거
        // 레벨 쪼개기(맨 처음이 1이고, 깊이가 늘어날때마다 1씩 오름
        // queue에 동시에 있는 경우 그건 같은 레벨로 처리
        // 레벨이 낮은거부터 좌측에 배치하기
        // 그렇게 배치하고 선 연결을 어케해야하지
        // 그냥 이것도 레벨 1부터 시작해서, 자기랑 연결된거에다가 라인 박아서 하게 할까
        private async void Start()
        {
            // 1. 모든 노드가 어떻게 배치될지를 알아온다.
            // 2. 노드를 실제로 배치한다.
            // 3. 노드들 사이를 연결한다.
            // 4. 모든 노드에게 지금 이 노드가 꺼져있나 안꺼져있나 설정하게 한다.
            _isInitialized = false;
            await UniTask.WaitUntil(() => !TreeDataManager.Instance.IsLoading); // TreeData 설정 끝날때까지 대기
            TreeDataManager.Instance.OnNodeActivated += HandleActive;
            // 노드별 배치할 레벨을 구한다.
            NodeLevelSelect();
            
            // 배치한다.
            PlaceAllNodes();
            
            // 노드 사이를 연결한다.
            LinkAllNode();
            
            // 모든 노드에게 상태의 업데이트를 지시한다.
            UpdateAllNodeUI();
            
            scrollRect.horizontalNormalizedPosition = 0f; // 좌측에서 시작하게
            _isInitialized = true;
            gameObject.SetActive(false);
        }
        
        private void NodeLevelSelect()
        {
            // 실제 데이터 상태랑은 별개로 그래프 생성을 위한 진입차수
            Dictionary<string, int> inDegreeMap = new Dictionary<string, int>();
            var allNodes = TreeDataManager.Instance.AllNodes;
            
            
            foreach (var nodeId in allNodes.Keys)
            {
                inDegreeMap[nodeId] = 0; // 딕셔너리 초기화부터 
            }
            foreach (var node in allNodes.Values)
            {
                foreach (var child in node.realChildNodes)
                {
                    if (inDegreeMap.ContainsKey(child.nodeID))
                    {
                        inDegreeMap[child.nodeID]++; // 진입차수 올리기
                    }
                }
            }
            
            Queue<string> queue = new Queue<string>();
            foreach (var (nodeId, linkCnt) in inDegreeMap)
            {
                if (linkCnt == 0) // 맨 앞에 찍을수 있게 되는 것들 뽑아온다.
                {
                    queue.Enqueue(nodeId);
                }
            }
            
            while (queue.Count > 0)
            {
                List<string> currents = new List<string>();
                while (queue.Count > 0)
                {
                    currents.Add(queue.Dequeue()); // 위상 정렬인데, 여기서 찍을 수 있는 모든걸 가져오게 하는거임.
                }
                _levelNodeIdList.Add(currents); // 차례차례 추가해준다.
                foreach (var nodeId in currents) // 꺼낸 모든 노드에 대해서 연산한다.
                {
                    // 이하는 평범한 위상정렬
                    var nodeData = allNodes[nodeId];
                    foreach (var child in nodeData.realChildNodes)
                    {
                        inDegreeMap[child.nodeID]--;
                    
                        if (inDegreeMap[child.nodeID] == 0) queue.Enqueue(child.nodeID);
                    }
                }
            }
        }
        
        private void PlaceAllNodes()
        {
            foreach (var list in _levelNodeIdList)
            {
                TreeLevelContainer container = Instantiate(treeLevelContainerPrefab, treeRoot);
                container.SetupData(list);
                foreach (var node in container.HaveNodes)
                {
                    _nodeObjects.Add(node.NodeData.nodeID,node);
                }
            }
        }
        
        private void LinkAllNode()
        {
            Canvas.ForceUpdateCanvases(); // UI 위치를 강제로 즉시 반영한다.
            
            foreach (TreeNodeObject nodeObject in _nodeObjects.Values)
            {
                foreach (var nodeData in nodeObject.NodeData.realChildNodes)
                {
                    UILineRenderer link = Instantiate(treeUiLinePrefab, nodeObject.lineTrm);
                    link.lineColor = TreeDataManager.Instance.IsNodeAvailable(nodeData.nodeID) ? Color.white : Color.gray;
                    
                    Vector2 rawEnd = link.transform.InverseTransformPoint(_nodeObjects[nodeData.nodeID].transform.position);
                    
                    Vector2 start = rawEnd.normalized * lineStartOffset;
                    Vector2 end = rawEnd - (rawEnd.normalized * lineEndOffset);

                    link.SetLinePositions(start, end);
                }
            }
        }
        
        private void HandleActive()
        {
            UpdateAllNodeUI();
        }
        
        private void HandleTreeNodeSelect(NodeSelectEvent evt)
        {
            foreach (var nodeObject in _nodeObjects.Values)
            {
                if (nodeObject.NodeData == evt.NodeData)
                {
                    nodeObject.Select();
                }
                else nodeObject.UnSelect();
            }
        }
        
        private void UpdateAllNodeUI()
        {
            Bus<UpdateNodeEvent>.Raise(new UpdateNodeEvent());
        }
        
        public void OpenUI()
        {
            if(!_isInitialized) return;
            UpdateAllNodeUI();
            Bus<NodeSelectEvent>.Raise(new NodeSelectEvent(defaultNodeObject));
        }
    }
}