using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Code.MainSystem.Tree.Upgrade;

namespace Code.MainSystem.Tree.Editor
{
    public class TreeEditorWindow : EditorWindow
    {
        private TreeGraphView _graphView;
        private VisualElement _inspectorPanel;
        private Dictionary<string, TreeGraphNode> _nodeCache = new();
        private HashSet<TreeNodeDataSO> _assetsToDelete = new(); 
        private bool _isDirty = false;

        // 데이터베이스 참조
        private TreeNodeDatabaseSO _database;

        [MenuItem("Tools/Tree System/Tree Node Editor")]
        public static void OpenWindow() => GetWindow<TreeEditorWindow>("Tree Node Editor");

        private void OnEnable()
        {
            _graphView = new TreeGraphView();
            _graphView.StretchToParentSize();
            _graphView.graphViewChanged = (changes) => { SetDirty(true); return changes; };
            _graphView.OnNodeSelected = ShowNodeInspector;
            _graphView.OnNodeCreated = (guid, nodeView) => { _nodeCache[guid] = nodeView; SetDirty(true); };
            _graphView.OnNodeDeletedFromGraph = (data) => { _assetsToDelete.Add(data); SetDirty(true); };

            _graphView.RegisterCallback<MouseUpEvent>(evt => _graphView.CheckSelection());
            rootVisualElement.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.modifiers == EventModifiers.Control && evt.keyCode == KeyCode.S) { SaveData(); evt.StopPropagation(); }
            });

            rootVisualElement.Add(_graphView);
            GenerateToolbar();
            GenerateInspectorPanel();
            
            Undo.undoRedoPerformed += OnUndoRedo;
            rootVisualElement.schedule.Execute(() => LoadAndAlignTree(false)).ExecuteLater(100);
        }

        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;
        private void OnUndoRedo() { LoadAndAlignTree(false); SetDirty(true); }

        private void SetDirty(bool dirty)
        {
            _isDirty = dirty;
            titleContent.text = _isDirty ? "Tree Node Editor*" : "Tree Node Editor";
        }

        // 데이터베이스 에셋을 찾아오거나 없으면 경고를 띄웁니다.
        private bool LoadDatabase()
        {
            if (_database != null) return true;
            string[] guids = AssetDatabase.FindAssets("t:TreeNodeDatabaseSO");
            if (guids.Length > 0) {
                _database = AssetDatabase.LoadAssetAtPath<TreeNodeDatabaseSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
                return true;
            }
            Debug.LogError("TreeNodeDatabaseSO를 찾을 수 없습니다! 에셋을 먼저 생성해주세요.");
            return false;
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.Add(new Button(SaveData) { text = "Save All Files (Ctrl+S)" });
            toolbar.Add(new Button(() => LoadAndAlignTree(true)) { text = "Reload & Force Align" });
            rootVisualElement.Add(toolbar);
        }

        private void GenerateInspectorPanel()
        {
            _inspectorPanel = new VisualElement {
                style = { width = 300, backgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.95f), borderLeftWidth = 1, borderLeftColor = Color.black, position = Position.Absolute, right = 0, top = 21, bottom = 0, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, paddingTop = 10 }
            };
            rootVisualElement.Add(_inspectorPanel);
        }

        private void ShowNodeInspector(TreeNodeDataSO data)
        {
            _inspectorPanel.Clear();
            _inspectorPanel.Add(new Label("NODE PROPERTIES") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            if (data == null) return;

            SerializedObject so = new SerializedObject(data);
            string[] props = { "nodeID", "nodeName", "description", "cost", "iconRef", "childNodeRefs", "upgradeRefs" };
            foreach (var propName in props) {
                var field = new PropertyField(so.FindProperty(propName));
                field.Bind(so);
                field.RegisterValueChangeCallback(evt => {
                    SetDirty(true);
                    if (propName == "nodeName") {
                        var node = _graphView.nodes.ToList().Cast<TreeGraphNode>().FirstOrDefault(n => n.Data == data);
                        node?.RefreshTitle();
                    }
                });
                _inspectorPanel.Add(field);
            }

            _inspectorPanel.Add(new Label("UPGRADE EFFECTS") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 5 } });
            string combinedDescriptions = "";
            foreach (var upgradeRef in data.upgradeRefs) {
                if (upgradeRef == null || string.IsNullOrEmpty(upgradeRef.AssetGUID)) continue;
                var upgrade = AssetDatabase.LoadAssetAtPath<BaseUpgradeSO>(AssetDatabase.GUIDToAssetPath(upgradeRef.AssetGUID));
                if (upgrade != null && !string.IsNullOrEmpty(upgrade.effectDescription)) combinedDescriptions += $"- {upgrade.effectDescription}\n";
            }
            _inspectorPanel.Add(new Label(string.IsNullOrEmpty(combinedDescriptions) ? "No effects." : combinedDescriptions) { style = { whiteSpace = WhiteSpace.Normal, marginTop = 5 } });
        }

        private void SaveData()
        {
            if (!LoadDatabase()) return;

            var currentNodesData = _graphView.nodes.ToList().Cast<TreeGraphNode>().Select(n => n.Data).Where(d => d != null).ToHashSet();
            _assetsToDelete.RemoveWhere(data => currentNodesData.Contains(data) || data == null);

            if (_assetsToDelete.Count > 0) {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"{_assetsToDelete.Count} assets will be deleted.", "Yes", "Cancel")) {
                    foreach (var data in _assetsToDelete) {
                        string path = AssetDatabase.GetAssetPath(data);
                        if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
                    }
                }
                _assetsToDelete.Clear();
            }

            // 데이터베이스 리스트 동기화 시작
            Undo.RecordObject(_database, "Update Tree Database");
            _database.treeNodes.Clear();

            foreach (var node in _graphView.nodes.ToList().Cast<TreeGraphNode>()) {
                var data = node.Data;
                if (data == null) continue;

                Undo.RecordObject(data, "Save Tree Data");
                data.graphPosition = node.GetPosition().position;
                
                // 데이터베이스에 추가
                string nodePath = AssetDatabase.GetAssetPath(data);
                string nodeGuid = AssetDatabase.AssetPathToGUID(nodePath);
                if (!string.IsNullOrEmpty(nodeGuid)) {
                    _database.treeNodes.Add(new AssetReferenceT<TreeNodeDataSO>(nodeGuid));
                }

                // 자식 노드 리스트 갱신
                data.childNodeRefs.Clear();
                foreach (var edge in node.OutputPort.connections) {
                    if (edge.input.node is TreeGraphNode childNode) {
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(childNode.Data));
                        if (!string.IsNullOrEmpty(guid)) data.childNodeRefs.Add(new AssetReferenceT<TreeNodeDataSO>(guid));
                    }
                }
                EditorUtility.SetDirty(data);
            }

            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetDirty(false);
            Debug.Log($"Tree Saved. Database has {_database.treeNodes.Count} nodes.");
        }

        private void LoadAndAlignTree(bool forceAutoLayout)
        {
            if (!LoadDatabase()) return;

            _graphView.DeleteElements(_graphView.graphElements.ToList());
            _nodeCache.Clear();
            bool hasSavedPositions = false;

            // 데이터베이스에 등록된 노드들만 로드합니다.
            foreach (var nodeRef in _database.treeNodes)
            {
                if (nodeRef == null || string.IsNullOrEmpty(nodeRef.AssetGUID)) continue;
                
                string path = AssetDatabase.GUIDToAssetPath(nodeRef.AssetGUID);
                var data = AssetDatabase.LoadAssetAtPath<TreeNodeDataSO>(path);
                if (data == null) continue;

                var node = _graphView.CreateNode(data);
                if (data.graphPosition != Vector2.zero) {
                    node.SetPosition(new Rect(data.graphPosition, new Vector2(200, 150)));
                    hasSavedPositions = true;
                }
                _nodeCache[nodeRef.AssetGUID] = node;
            }

            // 연결 복구
            foreach (var nodeView in _nodeCache.Values) {
                foreach (var childRef in nodeView.Data.childNodeRefs) {
                    if (childRef != null && !string.IsNullOrEmpty(childRef.AssetGUID) && _nodeCache.TryGetValue(childRef.AssetGUID, out var childView))
                        _graphView.ConnectPorts(nodeView.OutputPort, childView.InputPort);
                }
            }

            if (forceAutoLayout || !hasSavedPositions) ExecuteAutoLayout();
            SetDirty(false);
        }

        private void ExecuteAutoLayout()
        {
            var nodes = _nodeCache.Values.ToList();
            if (nodes.Count == 0) return;
            var roots = nodes.Where(n => !n.InputPort.connections.Any()).ToList();
            if (roots.Count == 0) roots.Add(nodes[0]);
            float currentY = 50f;
            HashSet<TreeGraphNode> visited = new();
            foreach (var root in roots) { RecursiveLayout(root, 100f, currentY, visited, out float treeHeight); currentY += treeHeight + 50f; }
            _graphView.FrameAll();
        }

        private void RecursiveLayout(TreeGraphNode node, float x, float y, HashSet<TreeGraphNode> visited, out float height)
        {
            visited.Add(node);
            float nodeHeight = 150f; float currentChildY = y; float totalChildrenHeight = 0;
            var children = node.OutputPort.connections.Select(e => e.input.node as TreeGraphNode).Where(n => n != null && !visited.Contains(n)).ToList();
            if (children.Count > 0) {
                foreach (var child in children) { RecursiveLayout(child, x + 350f, currentChildY, visited, out float childHeight); totalChildrenHeight += childHeight; currentChildY = y + totalChildrenHeight; }
                node.SetPosition(new Rect(x, y + (totalChildrenHeight / 2f) - (nodeHeight / 2f), 200, 150));
                height = totalChildrenHeight;
            } else { node.SetPosition(new Rect(x, y, 200, 150)); height = nodeHeight; }
        }
    }
}