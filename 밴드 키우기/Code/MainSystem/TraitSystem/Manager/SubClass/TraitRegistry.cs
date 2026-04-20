using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Core;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Interface;
using UnityEngine;

namespace Code.MainSystem.TraitSystem.Manager.SubClass
{

    public class TraitRegistry : MonoBehaviour, ITraitRegistry
    {
        [SerializeField] private string label;
        
        private readonly TaskCompletionSource<bool> _initializeTcs = new();
        public Task InitializationTask => _initializeTcs.Task;
        
        private readonly Dictionary<TraitTag, List<TraitDataSO>> _tagMap = new();
        private readonly Dictionary<int, TraitDataSO> _map = new();

        public async Task Initialize()
        {
            List<TraitDataSO> assets = await GameManager.Instance.LoadAllAddressablesAsync<TraitDataSO>(label);

            _map.Clear();
            _tagMap.Clear();
            
            var traitTags = System.Enum.GetValues(typeof(TraitTag))
                .Cast<TraitTag>()
                .Where(t => t != TraitTag.None)
                .ToList();

            foreach (var asset in assets.Where(a => a != null))
            {
                _map[asset.IDHash] = asset;

                foreach (var traitTag in traitTags.Where(traitTag => asset.TraitTag.HasFlag(traitTag)))
                {
                    if (!_tagMap.ContainsKey(traitTag))
                        _tagMap[traitTag] = new List<TraitDataSO>();
                
                    _tagMap[traitTag].Add(asset);
                }
            }
            
            _initializeTcs.TrySetResult(true);
        }
        
        // 특정 태그를 가진 모든 특성 가져오기
        public List<TraitDataSO> GetByTag(TraitTag traitTag)
        {
            return _tagMap.TryGetValue(traitTag, out var traits) ? traits : new List<TraitDataSO>();
        }

        /// <summary>
        /// Hash로 TraitDataSO를 가져옴
        /// </summary>
        /// <param name="hash">가져올 특성의 hash</param>
        /// <returns>특성 반환</returns>
        public TraitDataSO Get(int hash)
            => _map.GetValueOrDefault(hash);

        /// <summary>
        /// 찾고자 하는 특성이 있는지 확인
        /// </summary>
        /// <param name="traitHash">찾고자 하는 특성의 hash</param>
        /// <returns>있으면 true 없으면 false</returns>
        public bool Contains(int traitHash)
            => _map.ContainsKey(traitHash);
    }
}