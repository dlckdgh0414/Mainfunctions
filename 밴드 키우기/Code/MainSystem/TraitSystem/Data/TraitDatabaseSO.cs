using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.Data
{
    [CreateAssetMenu(fileName = "TraitDatabase", menuName = "SO/Database/TraitDatabase")]
    public class TraitDatabaseSO : ScriptableObject
    {
        [Header("특성 목록")]
        public List<TraitDataSO> traits = new List<TraitDataSO>();
        
        private Dictionary<string, TraitDataSO> _traitDict;
        
        public void Initialize()
        {
            _traitDict = new Dictionary<string, TraitDataSO>();
            foreach (var trait in traits)
            {
                if (trait != null && !string.IsNullOrEmpty(trait.TraitName))
                {
                    _traitDict[trait.TraitName] = trait;
                }
            }
        }
        
        public TraitDataSO GetTrait(string id)
        {
            if (_traitDict == null)
            {
                Initialize();
            }
            return _traitDict.ContainsKey(id) ? _traitDict[id] : null;
        }
    }
}
