using Code.MainSystem.Tree;

namespace Code.Core.Bus.GameEvents.TreeEvents
{
    public struct NodeSelectEvent : IEvent
    {
        public TreeNodeDataSO NodeData;

        public NodeSelectEvent(TreeNodeDataSO nodeData)
        {
            NodeData = nodeData;
        }
    }
}