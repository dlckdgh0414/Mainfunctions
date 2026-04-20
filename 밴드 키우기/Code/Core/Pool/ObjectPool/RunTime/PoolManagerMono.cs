using System;
using UnityEngine;
using Work.CHUH.Chuh007Lib.ObjectPool.RunTime;

namespace Chuh007Lib.ObjectPool.RunTime
{
    public class PoolManagerMono : MonoBehaviour
    {
        [SerializeField] private PoolManagerSO poolManager;

        private void Awake()
        {
            poolManager.Initialize(transform);
        }

        public T Pop<T>(PoolItemSO poolItem) where T : IPoolable
        {
            return (T)poolManager.Pop(poolItem);
        }

        public void Push(IPoolable item)
        {
            poolManager.Push(item);
        }
    }
}