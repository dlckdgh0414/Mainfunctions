using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Code.Core.Addressable
{
    public abstract class AddressableSO : ScriptableObject
    {
        public abstract UniTask LoadAssets();
        public abstract void UnloadAssets();
        
        [NonSerialized] protected bool _isLoaded = false;
        
        /// <summary>
        /// AssetReference를 통해 에셋을 로드합니다.
        /// </summary>
        /// <param name="assetRef">대상의 AssetReference 넣기 </param>
        /// <typeparam name="T">UnityEngine.Object, 반환되길 원하는거로.</typeparam>
        /// <returns></returns>
        protected async UniTask<T> RefLoad<T>(AssetReference assetRef) where T : Object
        {
            if (assetRef != null && assetRef.RuntimeKeyIsValid())
            {
                try
                {
                    var result = await assetRef.LoadAssetAsync<T>().Task;
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }
            }
            return null;
        }
    }
}