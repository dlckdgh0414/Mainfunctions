using Chuh007Lib.ObjectPool.RunTime;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Work.CHUH.Chuh007Lib.ObjectPool.RunTime;

namespace Chuh007Lib.ObjectPool.RunTime
{
    [CreateAssetMenu(fileName = "PoolManager", menuName = "SO/Pool/Manager")]
    public class PoolManagerSO : ScriptableObject
    {
        public List<PoolItemSO> itemList = new List<PoolItemSO>();

        Dictionary<PoolItemSO, Pool> _pools;
        Transform _rootTrm;

        public void Initialize(Transform rootTrm)
        {
            _rootTrm = rootTrm;
            _pools = new Dictionary<PoolItemSO, Pool>();

            foreach (var item in itemList)
            {
                IPoolable poolable = item.prefab.GetComponent<IPoolable>();
                Debug.Assert(poolable != null, $"Pooling item does not have IPoolable component {item.prefab.name}");

                Transform poolParent = new GameObject(item.poolingName).transform;
                poolParent.SetParent( _rootTrm );

                Pool pool = new Pool(poolable, _rootTrm, item.initCount);
                _pools.Add(item, pool);
            }
        }

        public IPoolable Pop(PoolItemSO item)
        {
            if (_pools.TryGetValue(item, out Pool pool))
            {
                return pool.Pop();
            }

            return null;
        }

        public void Push(IPoolable item)
        {
            if (_pools.TryGetValue(item.PoolItem, out Pool pool))
            {
                pool.Push(item);
            }
        }
    }
}
