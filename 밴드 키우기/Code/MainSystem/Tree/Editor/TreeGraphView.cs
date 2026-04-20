using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Code.MainSystem.Tree.Editor
{
    public class TreeGraphView : GraphView
    {
        public Action<TreeNodeDataSO> OnNodeSelected;
        public Action<string, TreeGraphNode> OnNodeCreated; 
        public Action<TreeNodeDataSO> OnNodeDeletedFromGraph; 
        private TreeSearchWindow _searchWindow;

        public TreeGraphView()
        {
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            this.graphViewChanged = (changes) => {
                if (changes.edgesToCreate != null || changes.elementsToRemove != null) {
                    foreach (var node in nodes.ToList().Cast<TreeGraphNode>()) {
                        if (node.Data != null) {
                            Undo.RecordObject(node.Data, "Graph Change");
                            EditorUtility.SetDirty(node.Data);
                        }
                    }
                }
                return changes;
            };

            _searchWindow = ScriptableObject.CreateInstance<TreeSearchWindow>();
            _searchWindow.Init(this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
            deleteSelection = (operationName, askUser) => {
                foreach (var selectable in selection) if (selectable is TreeGraphNode node) OnNodeDeletedFromGraph?.Invoke(node.Data);
                DeleteSelection(); 
            };
            style.flexGrow = 1;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        }

        public void CheckSelection()
        {
            var selectedNode = selection.FirstOrDefault(x => x is TreeGraphNode) as TreeGraphNode;
            OnNodeSelected?.Invoke(selectedNode?.Data);
        }

        public void CreateNewNodeOnMouse(Vector2 screenMousePosition)
        {
            string targetFolder = "Assets/_Modules/Tree/TreeNodes";
            CreateFolderRecursively(targetFolder);

            TreeNodeDataSO newData = ScriptableObject.CreateInstance<TreeNodeDataSO>();
            newData.nodeName = "New Node";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/NewTraitNode.asset");
            
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.ImportAsset(path); // 즉시 임포트하여 인식 유도
            AssetDatabase.SaveAssets();

            var localPos = contentViewContainer.WorldToLocal(screenMousePosition);
            newData.graphPosition = localPos;

            var node = CreateNode(newData);
            node.SetPosition(new Rect(localPos, new Vector2(200, 150)));

            string guid = AssetDatabase.AssetPathToGUID(path);
            OnNodeCreated?.Invoke(guid, node);
            
            node.RefreshTitle();
            ClearSelection(); AddToSelection(node); CheckSelection();
            
            Undo.RegisterCreatedObjectUndo(newData, "Create Node");
        }

        private void CreateFolderRecursively(string path)
        {
            string[] folders = path.Split('/'); string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++) {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath)) AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath = nextPath;
            }
        }

        public TreeGraphNode CreateNode(TreeNodeDataSO data)
        {
            var node = new TreeGraphNode(data);
            AddElement(node);
            return node;
        }

        public void ConnectPorts(Port output, Port input)
        {
            var edge = output.ConnectTo(input);
            AddElement(edge);
        }
    }
}