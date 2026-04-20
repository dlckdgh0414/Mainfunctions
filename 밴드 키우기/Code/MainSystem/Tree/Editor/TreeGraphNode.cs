using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.MainSystem.Tree.Editor
{
    public class TreeGraphNode : Node
    {
        public TreeNodeDataSO Data { get; private set; }
        public Port InputPort;
        public Port OutputPort;

        public TreeGraphNode(TreeNodeDataSO data)
        {
            Data = data;
            if(string.IsNullOrEmpty(Data.nodeID)) Data.nodeID = System.Guid.NewGuid().ToString();
            title = string.IsNullOrEmpty(data.nodeName) ? data.name : data.nodeName;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "Parent";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Children";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        // 그래프 뷰에서 노드를 움직일 때 호출됨
        public override void SetPosition(Rect newPos)
        {
            if (Data != null && !Mathf.Approximately(Data.graphPosition.x, newPos.x))
            {
                // 위치가 바뀌기 직전에 Undo 기록
                Undo.RecordObject(Data, "Move Tree Node");
                Data.graphPosition = newPos.position;
                EditorUtility.SetDirty(Data);
            }
            base.SetPosition(newPos);
        }
        
        public void RefreshTitle() => title = string.IsNullOrEmpty(Data.nodeName) ? Data.name : Data.nodeName;
    }
}