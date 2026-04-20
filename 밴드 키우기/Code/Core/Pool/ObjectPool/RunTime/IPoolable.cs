using Chuh007Lib.ObjectPool.RunTime;
using UnityEngine;

namespace Work.CHUH.Chuh007Lib.ObjectPool.RunTime
{
    public interface IPoolable
    {
        public PoolItemSO PoolItem { get; }
        public GameObject gameObject { get; }
        public void ResetItem();
        public void SetUpPool(Pool pool);
    }
}

