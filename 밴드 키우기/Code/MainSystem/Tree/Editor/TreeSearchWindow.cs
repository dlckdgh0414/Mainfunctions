using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.MainSystem.Tree.Editor
{
    public class TreeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private TreeGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Init(TreeGraphView graphView)
        {
            _graphView = graphView;
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeEntry(new GUIContent("New Trait Node", _indentationIcon))
                {
                    userData = "CreateNode",
                    level = 1
                }
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData.ToString() == "CreateNode")
            {
                _graphView.CreateNewNodeOnMouse(context.screenMousePosition);
                return true;
            }
            return false;
        }
    }
}