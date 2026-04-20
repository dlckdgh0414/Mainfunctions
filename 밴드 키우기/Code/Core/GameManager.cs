using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Code.Core
{
    [DefaultExecutionOrder(-10)]
    public class GameManager : MonoBehaviour
    {
        [field: SerializeField] public int Priority { get; private set; } = 1;
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance is null)
                    _instance = FindAnyObjectByType<GameManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            GameManager[] managers = FindObjectsByType<GameManager>(FindObjectsSortMode.None);

            if (managers.Length > 1)
            {
                foreach (var m in managers)
                {
                    if (m.Priority > Priority)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public async Task<List<T>> LoadAllAddressablesAsync<T>(string label)
            where T : Object
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return new List<T>(handle.Result);

            return new List<T>();
        }
        
        public async Task<T> LoadAddressableAsync<T>(string key)
            where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            return await handle.Task;
        }
    }
}