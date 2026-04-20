using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.Tree
{
    [CreateAssetMenu(fileName = "TreeNodeDatabase", menuName = "SO/Tree/TreeNodeDatabase", order = 0)]
    public class TreeNodeDatabaseSO : ScriptableObject
    {
        public List<AssetReferenceT<TreeNodeDataSO>> treeNodes;
    }
}