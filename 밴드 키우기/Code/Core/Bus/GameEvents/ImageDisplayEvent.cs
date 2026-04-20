using UnityEngine.AddressableAssets;

namespace Code.Core.Bus.GameEvents
{
    public struct ImageDisplayEvent : IEvent
    {
        public readonly AssetReferenceGameObject ImagePrefabReference;

        public ImageDisplayEvent(AssetReferenceGameObject imagePrefabReference)
        {
            ImagePrefabReference = imagePrefabReference;
        }
    }
}