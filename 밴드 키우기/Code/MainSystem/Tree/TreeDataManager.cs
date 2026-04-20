using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.NewMainScreen;
using Code.SubSystem.BandFunds;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace Code.MainSystem.Tree
{
    public class TreeDataManager : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<TreeNodeDatabaseSO> treeDatabase;
        [SerializeField] private LoadingUI loadingUI;
        
        public static TreeDataManager Instance { get; private set; }
        
        // 모든 특성 노드 데이터를 식별 ID로 저장하는 딕셔너리
        public readonly Dictionary<string, TreeNodeDataSO> AllNodes = new();
        
        // 활성화된 특성 노드 ID 목록
        private HashSet<string> activatedNodes = new HashSet<string>();
        
        // 각 노드의 현재 진입차수 기록
        private Dictionary<string, int> nodeInDegrees = new Dictionary<string, int>();

        public bool IsLoading { get; private set; } = true;
        
        public event Action OnNodeActivated;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
            LoadAllTreeNodeData();
        }

        private async void LoadAllTreeNodeData()
        {
            if(treeDatabase.IsValid()) return;
            loadingUI.Show();
            var loadedNodes = await treeDatabase.LoadAssetAsync<TreeNodeDatabaseSO>();
            
            foreach (var node in loadedNodes.treeNodes)
            {
                var data = await node.LoadAssetAsync<TreeNodeDataSO>();
                await data.LoadAssets();
                AllNodes[data.nodeID] = data;
            }
            InitializeInDegrees();
            IsLoading = false;
            loadingUI.Hide();
            Bus<TutorialStartEvent>.Raise(new TutorialStartEvent());
        }

        // 진입차수 초기화
        private void InitializeInDegrees()
        {
            nodeInDegrees.Clear();

            foreach (var (nodeName, _) in AllNodes)
            {
                nodeInDegrees[nodeName] = 0; // 0 기본으로 깔고
            }
            foreach (var (_, nodeData) in AllNodes)
            {
                foreach (var child in nodeData.realChildNodes)
                {
                    nodeInDegrees[child.nodeID]++; // 진입차수 맞추기
                }
            }
        }

        // 2. 노드 활성화 시도 (UI 버튼 클릭 시 호출)
        public bool TryActivateNode(string nodeId)
        {
            if (!AllNodes.TryGetValue(nodeId, out TreeNodeDataSO nodeData)) 
            {
                Debug.LogError($"ID 없음 : {nodeId}");
                return false;
            }
            if (IsNodeAvailable(nodeId) || !CanActivateNode(nodeId) || !IsEnoughCost(nodeId)) return false;
            
            BandSupplyManager.Instance.SpendBandExp(nodeData.cost);
            activatedNodes.Add(nodeId);
            UpdateChildrenInDegrees(nodeData);
            foreach (var effect in nodeData.realUpgrades)
            {
                Bus<TreeUpgradeEvent>.Raise(new TreeUpgradeEvent(effect.type, effect));
            }
            OnNodeActivated?.Invoke();
            return true;
        }

        // 자식 노드들의 진입차수 갱신하는 메서드
        private void UpdateChildrenInDegrees(TreeNodeDataSO activatedNode)
        {
            foreach (var childID in activatedNode.realChildNodes)
            {
                if (nodeInDegrees.ContainsKey(childID.nodeID))
                {
                    nodeInDegrees[childID.nodeID]--;
                }
            }
        }
        
        /// <summary>
        /// 노드가 이미 찍힌 노드인지 확인하는 메서드
        /// </summary>
        /// <param name="nodeID">Node Data의 string 쓰기. 손으로 적지 마세요</param>
        public bool IsNodeAvailable(string nodeID)
        {
            return activatedNodes.Contains(nodeID);
        }
        
        /// <summary>
        /// 노드를 찍을 수 있는지 확인하는 메서드
        /// </summary>
        /// <param name="nodeID">Node Data의 string 쓰기. 손으로 적지 마세요</param>
        /// <returns></returns>
        public bool CanActivateNode(string nodeID)
        {
            return nodeInDegrees[nodeID] == 0;
        }

        public bool IsEnoughCost(string nodeId) => 
            BandSupplyManager.Instance.CheckBandExp(AllNodes[nodeId].cost); 
        
#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current.qKey.isPressed && Keyboard.current.ctrlKey.isPressed)
            {
                Bus<MusicUploadEvent>.Raise(new MusicUploadEvent());
            }

            if (Keyboard.current.wKey.isPressed && Keyboard.current.ctrlKey.isPressed)
            {
                BandSupplyManager.Instance.AddBandExp(100);
            }
        }
#endif
        
        // 사이클 감지용 코드, 사이클이 있으면 안된다.
#if UNITY_EDITOR
        [ContextMenu("Check Cycle")]
        public void HasCycle()
        {
            Dictionary<string, int> inDegreeMap = new Dictionary<string, int>();

            foreach (var nodeId in AllNodes.Keys)
            {
                inDegreeMap[nodeId] = 0;
            }
            foreach (var node in AllNodes.Values)
            {
                foreach (var child in node.realChildNodes)
                {
                    if (inDegreeMap.ContainsKey(child.nodeID))
                    {
                        inDegreeMap[child.nodeID]++;
                    }
                }
            }
            Queue<string> queue = new Queue<string>();
            foreach (var kvp in inDegreeMap)
            {
                if (kvp.Value == 0)
                {
                    queue.Enqueue(kvp.Key);
                }
            }
            
            int visitedCount = 0;
            while (queue.Count > 0)
            {
                string currentId = queue.Dequeue();
                visitedCount++;

                if (AllNodes.TryGetValue(currentId, out var nodeData))
                {
                    foreach (var child in nodeData.realChildNodes)
                    {
                        inDegreeMap[child.nodeID]--;
                        
                        if (inDegreeMap[child.nodeID] == 0) queue.Enqueue(child.nodeID);
                    }
                }
            }
            
            bool hasCycle = visitedCount != AllNodes.Count;
            
            if (hasCycle)
                Debug.LogError($"사이클 감지 : {visitedCount} / {AllNodes.Count}");
            else 
                Debug.Log("사이클 없음");
        }
#endif
        
    }
}
