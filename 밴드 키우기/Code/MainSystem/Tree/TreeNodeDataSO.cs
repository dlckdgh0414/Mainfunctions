using System;
using System.Collections.Generic;
using Code.Core.Addressable;
using Code.MainSystem.Tree.Upgrade;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.Tree
{
    [CreateAssetMenu(fileName = "TraitNodeData", menuName = "SO/Tree/TraitNodeData", order = 0)]
    public class TreeNodeDataSO : AddressableSO
    {
        [HideInInspector] public Vector2 graphPosition; 

        public string nodeID; 
        public string nodeName; 
        [TextArea] public string description; 
        public int cost;
        
        public AssetReferenceSprite iconRef; 
        public List<AssetReferenceT<TreeNodeDataSO>> childNodeRefs = new();
        public List<AssetReferenceT<BaseUpgradeSO>> upgradeRefs = new();
        
        [NonSerialized] public Sprite realIcon;
        [NonSerialized] public List<TreeNodeDataSO> realChildNodes = new();
        [NonSerialized] public List<BaseUpgradeSO> realUpgrades = new();
        
        public override async UniTask LoadAssets()
        {
            if(_isLoaded) return;
            realIcon = await RefLoad<Sprite>(iconRef);
            
            realChildNodes.Clear();
            foreach (var assetRef in childNodeRefs)
            {
                var data = await RefLoad<TreeNodeDataSO>(assetRef);
                realChildNodes.Add(data);
            }
            
            realUpgrades.Clear();
            foreach (var assetRef in upgradeRefs)
            {
                var data = await RefLoad<BaseUpgradeSO>(assetRef);
                Debug.Log(data);
                realUpgrades.Add(data);
            }
            _isLoaded = true;
        }
        
        public override void UnloadAssets()
        {
            if (!_isLoaded) return;
            if (iconRef != null && iconRef.IsValid()) iconRef.ReleaseAsset();
            foreach (var assetRef in childNodeRefs)
            {
                if (assetRef != null && assetRef.IsValid()) assetRef.ReleaseAsset();
            }
            foreach (var assetRef in upgradeRefs)
            {
                if (assetRef != null && assetRef.IsValid()) assetRef.ReleaseAsset();
            }
            _isLoaded = false;
        }
    }
}