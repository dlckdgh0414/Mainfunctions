using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace Code.Core.Addressable
{
    public class GameResourceManager : IDisposable
    {
        private static GameResourceManager _instance;
        public static GameResourceManager Instance => _instance ??= new GameResourceManager();

        private Dictionary<string, Object> _resourceDict = new();
        private Dictionary<string,AsyncOperationHandle> _loadHandleDict = new();

        public event Action OnLoadCountChange;
        private int _needLoadCount = 0;
        private int _loadCount = 0;

        public int NeedLoadCount
        {
            get => _needLoadCount;
            set
            {
                _needLoadCount = value;
                OnLoadCountChange?.Invoke();
            }
        }

        public int LoadCount
        {
            get => _loadCount;
            set
            {
                _loadCount = value;
                OnLoadCountChange?.Invoke();
            }
        }

        #region 로드된 리소스들을 언제든 가져올 수 있께 해주는 영역

        public T Load<T>(string key) where T : Object
        {
            return _resourceDict.GetValueOrDefault(key) as T;
        }
        #endregion

        #region  어드레서블 에셋 로더


        public async Task<T> LoadAsync<T>(string key) where T : Object
        {
            if (_resourceDict.TryGetValue(key, out var resource))
                return resource as T;
            
            if (_loadHandleDict.TryGetValue(key, out var existingHandle))
                return await existingHandle.Convert<T>().Task;
            
            AsyncOperationHandle<T> opHandle = Addressables.LoadAssetAsync<T>(key);
            
            _loadHandleDict.Add(key, opHandle);

            T result = await opHandle.Task;
            _resourceDict.Add(key, result);
    
            return result;
        }

        public async Task<T> LoadAssetAsync<T>(string key) where T : Object => await LoadAsync<T>(key);
        
        //멀티 에셋 로더
        public async Task LoadAllAsync<T>(string lable) where T : Object
        {
            IList<IResourceLocation> results = await Addressables.LoadResourceLocationsAsync(lable,typeof(T)).Task;
            NeedLoadCount += results.Count;

            foreach (IResourceLocation location in results)
            {
                await LoadAsync<T>(location.PrimaryKey);
                LoadCount++;
            }
        }
        #endregion
        
        
        public void Release(Object obj)
        {
            if (obj == null) return;

            string keyToRemove = null;
            foreach (var pair in _resourceDict)
            {
                if (pair.Value == obj)
                {
                    keyToRemove = pair.Key;
                    break;
                }
            }

            if (keyToRemove != null)
            {
                if (_loadHandleDict.TryGetValue(keyToRemove, out var handle))
                {
                    Addressables.Release(handle);
                    _loadHandleDict.Remove(keyToRemove);
                }
                _resourceDict.Remove(keyToRemove);
                LoadCount--;
            }
        }
        
        public void Dispose()
        {
            foreach (var handle in _loadHandleDict.Values)
            {
                Addressables.Release(handle);
            }
        }
    }
}