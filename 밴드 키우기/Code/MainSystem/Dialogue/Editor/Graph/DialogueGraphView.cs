using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Code.MainSystem.Dialogue;

namespace Code.MainSystem.Dialogue.Editor.Graph
{
    /// <summary>
    /// 다이얼로그 노드와 연결선을 렌더링하고 편집하는 그래프 뷰
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        public Action<string> onNodeSelected;
        public Action onNodeUnselected;
        public Action onGraphChanged;

        private DialogueInformationSO _currentSO;
        private bool _isPopulating = false;

        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // 배경 우클릭 메뉴 등록
            ConstructContextualMenu();

            graphViewChanged = OnGraphViewChanged;
        }

        private void ConstructContextualMenu()
        {
            // 배경 우클릭 시 노드 생성 메뉴 추가
            this.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                if (evt.target is DialogueGraphView)
                {
                    Vector2 mousePos = evt.localMousePosition;
                    Vector2 graphMousePos = viewTransform.matrix.inverse.MultiplyPoint(mousePos);

                    evt.menu.AppendAction("Create Node", action => 
                    {
                        CreateAndAddNode($"Node_{DateTime.Now.Ticks % 10000}", graphMousePos);
                    });
                }
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (_isPopulating) return change;

            if (change.elementsToRemove != null && _currentSO != null)
            {
                Undo.RecordObject(_currentSO, "Delete Graph Elements");
                List<DialogueNode> nodes = _currentSO.DialogueNodes;
                bool nodeDeleted = false;

                foreach (GraphElement element in change.elementsToRemove)
                {
                    if (element is Node node)
                    {
                        nodes.RemoveAll(n => n.NodeID == node.viewDataKey);
                        nodeDeleted = true;
                        
                        if (selection.Contains(node)) onNodeUnselected?.Invoke();
                    }
                }

                if (nodeDeleted)
                {
                    _currentSO.SetNodes(nodes);
                    EditorUtility.SetDirty(_currentSO);
                    onGraphChanged?.Invoke();
                }
            }
            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void PopulateFromSO(DialogueInformationSO dialogueSO)
        {
            _isPopulating = true;
            _currentSO = dialogueSO;
            DeleteElements(graphElements);
            _isPopulating = false;

            if (dialogueSO == null || dialogueSO.DialogueNodes == null) return;

            Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();

            foreach (DialogueNode dialogueNode in dialogueSO.DialogueNodes)
            {
                Node graphNode = CreateReadOnlyGraphNode(dialogueNode);
                AddElement(graphNode);
                nodeMap[dialogueNode.NodeID] = graphNode;
            }

            foreach (DialogueNode dialogueNode in dialogueSO.DialogueNodes)
            {
                if (!nodeMap.TryGetValue(dialogueNode.NodeID, out Node fromNode)) continue;

                if (dialogueNode.Choices != null && dialogueNode.Choices.Count > 0)
                {
                    for (int i = 0; i < dialogueNode.Choices.Count; i++)
                    {
                        DialogueChoice choice = dialogueNode.Choices[i];
                        if (!string.IsNullOrEmpty(choice.NextNodeID) && nodeMap.TryGetValue(choice.NextNodeID, out Node toNode))
                        {
                            if (i < fromNode.outputContainer.childCount)
                            {
                                Port outputPort = fromNode.outputContainer[i] as Port;
                                LinkPorts(outputPort, toNode);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(dialogueNode.NextNodeID) && nodeMap.TryGetValue(dialogueNode.NextNodeID, out Node toNode))
                {
                    if (fromNode.outputContainer.childCount > 0)
                    {
                        Port outputPort = fromNode.outputContainer[0] as Port;
                        LinkPorts(outputPort, toNode);
                    }
                }
            }
            
            UpdateStartNodeVisuals();
        }

        public void ClearGraphSafely()
        {
            _isPopulating = true;
            DeleteElements(graphElements);
            _isPopulating = false;
        }

        private void LinkPorts(Port outPort, Node toNode)
        {
            if (outPort == null || toNode == null) return;
            Port inputPort = toNode.inputContainer.Query<Port>().First();
            if (inputPort != null)
            {
                Edge edge = outPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }

        private Node CreateReadOnlyGraphNode(DialogueNode data)
        {
            Node node = new Node
            {
                title = data.NodeID,
                viewDataKey = data.NodeID
            };

            node.RegisterCallback<MouseDownEvent>(evt =>
            {
                onNodeSelected?.Invoke(node.viewDataKey);
            });

            // 노드 우클릭 메뉴 (시작 노드 설정)
            node.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Set as Start Node", action => 
                {
                    SetAsStartNode(node.viewDataKey);
                });
            });

            Port inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "In";
            node.inputContainer.Add(inputPort);

            Label dialogueLabel = new Label(data.DialogueDetail);
            dialogueLabel.style.maxWidth = 180;
            dialogueLabel.style.whiteSpace = WhiteSpace.Normal;
            dialogueLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            dialogueLabel.style.paddingTop = 5;
            node.extensionContainer.Add(dialogueLabel);

            if (data.Choices != null && data.Choices.Count > 0)
            {
                foreach (DialogueChoice choice in data.Choices)
                {
                    Port outPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                    outPort.portName = string.IsNullOrEmpty(choice.ChoiceText) ? "Next (Auto)" : choice.ChoiceText;
                    node.outputContainer.Add(outPort);
                }
            }
            else
            {
                Port outPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                outPort.portName = "Next";
                node.outputContainer.Add(outPort);
            }

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(data.NodePosition, new Vector2(200, 150)));

            return node;
        }

        private void CreateAndAddNode(string nodeID, Vector2 position)
        {
            if (_currentSO == null) return;

            Undo.RecordObject(_currentSO, "Create Node");
            List<DialogueNode> nodes = _currentSO.DialogueNodes ?? new List<DialogueNode>();
            DialogueNode newNodeData = new DialogueNode 
            { 
                NodeID = nodeID, 
                NodePosition = position, 
                Choices = new List<DialogueChoice>() 
            };
            nodes.Add(newNodeData);
            _currentSO.SetNodes(nodes);
            EditorUtility.SetDirty(_currentSO);
            AssetDatabase.SaveAssets();

            Node graphNode = CreateReadOnlyGraphNode(newNodeData);
            AddElement(graphNode);
            
            onGraphChanged?.Invoke();
        }

        private void SetAsStartNode(string nodeID)
        {
            if (_currentSO == null) return;
            Undo.RecordObject(_currentSO, "Set Start Node");
            _currentSO.SetStartNode(nodeID);
            EditorUtility.SetDirty(_currentSO);
            UpdateStartNodeVisuals();
            onGraphChanged?.Invoke();
        }

        public void UpdateStartNodeVisuals()
        {
            if (_currentSO == null) return;

            foreach (Node node in graphElements.ToList().OfType<Node>())
            {
                bool isStart = node.viewDataKey == _currentSO.StartNodeID;
                node.title = isStart ? $"[START] {node.viewDataKey}" : node.viewDataKey;
                
                VisualElement titleContainer = node.Query<VisualElement>("title");
                if (titleContainer != null)
                {
                    titleContainer.style.backgroundColor = isStart ? new Color(0.1f, 0.4f, 0.1f, 0.8f) : new Color(0.24f, 0.24f, 0.24f, 0.8f);
                }
            }
        }

        public void SaveToSO()
        {
            if (_currentSO == null) return;

            Undo.RecordObject(_currentSO, "Save Dialogue Graph Changes");

            List<DialogueNode> nodes = _currentSO.DialogueNodes;

            foreach (Node gNode in graphElements.ToList().OfType<Node>())
            {
                int nodeIndex = nodes.FindIndex(n => n.NodeID == gNode.viewDataKey);
                if (nodeIndex == -1) continue;

                DialogueNode nodeData = nodes[nodeIndex];
                nodeData.NodePosition = gNode.GetPosition().position;

                if (nodeData.Choices != null && nodeData.Choices.Count > 0)
                {
                    for (int i = 0; i < nodeData.Choices.Count; i++)
                    {
                        Port outPort = gNode.outputContainer[i] as Port;
                        string nextID = (outPort != null && outPort.connected) ? (outPort.connections.First().input.node as Node).viewDataKey : string.Empty;
                        DialogueChoice updatedChoice = nodeData.Choices[i];
                        updatedChoice.NextNodeID = nextID;
                        nodeData.Choices[i] = updatedChoice;
                    }
                    nodeData.NextNodeID = string.Empty; // Choices가 있으면 NextNodeID는 사용 안 함
                }
                else
                {
                    Port nextPort = gNode.outputContainer.Query<Port>().First();
                    if (nextPort != null && nextPort.connected)
                    {
                        string nextID = (nextPort.connections.First().input.node as Node).viewDataKey;
                        nodeData.NextNodeID = nextID;
                    }
                    else
                    {
                        nodeData.NextNodeID = string.Empty;
                    }
                    // 빈 더미 Choice 리스트 생성을 방지
                    nodeData.Choices = null; 
                }
                nodes[nodeIndex] = nodeData;
            }

            _currentSO.SetNodes(nodes);
            EditorUtility.SetDirty(_currentSO);
            AssetDatabase.SaveAssets();
            Debug.Log($"[DialogueGraph] Saved for {_currentSO.name}");
        }

        public void HighlightNode(string nodeID, Color color)
        {
            Node node = graphElements.ToList().OfType<Node>().FirstOrDefault(n => n.viewDataKey == nodeID);
            if (node != null)
            {
                node.style.borderTopColor = color; node.style.borderBottomColor = color;
                node.style.borderLeftColor = color; node.style.borderRightColor = color;
                node.style.borderTopWidth = 2; node.style.borderBottomWidth = 2;
                node.style.borderLeftWidth = 2; node.style.borderRightWidth = 2;
            }
        }
        
        public void ClearHighlights()
        {
            foreach (Node node in graphElements.ToList().OfType<Node>())
            {
                node.style.borderTopColor = StyleKeyword.Null; node.style.borderBottomColor = StyleKeyword.Null;
                node.style.borderLeftColor = StyleKeyword.Null; node.style.borderRightColor = StyleKeyword.Null;
                node.style.borderTopWidth = StyleKeyword.Null; node.style.borderBottomWidth = StyleKeyword.Null;
                node.style.borderLeftWidth = StyleKeyword.Null; node.style.borderRightWidth = StyleKeyword.Null;
            }
        }

        public void UpdateNodeContent(string nodeID, string newText)
        {
            UnityEditor.Experimental.GraphView.Node node = graphElements.ToList()
                .OfType<UnityEditor.Experimental.GraphView.Node>()
                .FirstOrDefault(n => n.viewDataKey == nodeID);
            
            if (node != null)
            {
                UnityEngine.UIElements.Label dialogueLabel = node.extensionContainer.Query<UnityEngine.UIElements.Label>().First();
                if (dialogueLabel != null)
                {
                    dialogueLabel.text = newText;
                }
            }
        }
    }
}
