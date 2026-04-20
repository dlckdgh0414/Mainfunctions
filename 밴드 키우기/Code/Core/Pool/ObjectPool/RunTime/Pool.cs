using System.Collections.Generic;
using Chuh007Lib.ObjectPool.RunTime;
using UnityEngine;

namespace Work.CHUH.Chuh007Lib.ObjectPool.RunTime
{
    public class Pool
    {
        private Stack<IPoolable> _pool;
        private Transform _parent;    
        private GameObject _prefab;

        public Pool(IPoolable poolable, Transform parent, int count)
        {
            _pool = new Stack<IPoolable>(count);
            _parent = parent;
            _prefab = poolable.gameObject;
            for (int i = 0; i < count; i++)
            {
                GameObject gameObj = GameObject.Instantiate(_prefab, _parent);
                gameObj.SetActive(false);
                IPoolable item = gameObj.GetComponent<IPoolable>();
                item.SetUpPool(this);
                _pool.Push(item);
            }
        }

        public IPoolable Pop()
        {
            IPoolable item;
            if (_pool.Count == 0)
            {
                GameObject gameObj = GameObject.Instantiate(_prefab, _parent);
                item = gameObj.GetComponent<IPoolable>();
                item.SetUpPool(this);
            }
            else
            {
                item = _pool.Pop();
                item.gameObject.SetActive(true);
            }
            item.ResetItem();
            return item;
        }

        public void Push(IPoolable item)
        {
            item.gameObject.SetActive(false);
            _pool.Push(item);
        }
    }
}
