using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Code.MainSystem.NewMainScreen.Data
{
    [Serializable]
    public class MemberCoditionModes
    {
        public MemberConditionMode mode;
        public string str;
        public Sprite icon;
    }
    [CreateAssetMenu(fileName = "Bass", menuName = "SO/NewMemberData/Data", order = 0)]
    public class MemberDataSO : ScriptableObject
    {
        [Tooltip("멤버 정보관련")]
        public string memberListName;
        public string memberName;
        public MemberType memberType;
        public MemberConditionMode currentmod;
        public List<MemberCoditionModes> memberConditionModes;
        
        [Tooltip("멤버 이미지관련")]
        public AssetReferenceSprite memberLdSprite;
        public AssetReferenceSprite memberSDSprite;
        public AssetReferenceSprite memberIconSprite;

        [Tooltip("멤버 스탯관련")] 
        public List<AssetReference> memberStatList;

        [Tooltip("실제 쓰이는거")] 
        [NonSerialized] public Sprite LdSprite;
        [NonSerialized] public Sprite SDsprite;
        [NonSerialized] public Sprite IconSprite;
        [NonSerialized] public List<StatData> Stats;
        [NonSerialized] private bool _isLoaded = false;
        
        public async UniTask LoadAssets()
        {
            if (_isLoaded) return;
            
            if (Stats == null)
                Stats = new List<StatData>();
            else
                Stats.Clear();
            
            if (memberLdSprite != null && memberLdSprite.RuntimeKeyIsValid())
            {
                try
                {
                    LdSprite = await memberLdSprite.LoadAssetAsync<Sprite>().Task;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MemberDataSO] Failed to load sprite for {memberListName}: {e.Message}");
                }
            }
            if (memberSDSprite != null && memberSDSprite.RuntimeKeyIsValid())
            {
                try
                {
                    SDsprite = await memberSDSprite.LoadAssetAsync<Sprite>().Task;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MemberDataSO] Failed to load icon for {memberListName}: {e.Message}");
                }
            }
            if (memberIconSprite != null && memberIconSprite.RuntimeKeyIsValid())
            {
                try
                {
                    IconSprite = await memberIconSprite.LoadAssetAsync<Sprite>().Task;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MemberDataSO] Failed to load icon for {memberListName}: {e.Message}");
                }
            }

            if (memberStatList != null)
            {
                foreach (var statRef in memberStatList)
                {
                    if (statRef != null && statRef.RuntimeKeyIsValid())
                    {
                        try
                        {
                            var stat = await statRef.LoadAssetAsync<StatData>().Task;
                            if (stat != null) Stats.Add(stat);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[MemberDataSO] Failed to load icon for {memberListName}: {e.Message}");
                        }
                    }
                }
            }
            _isLoaded = true;
        }
        
        public void UnloadAssets()
        {
            if (!_isLoaded) return;

            if (memberLdSprite != null && memberLdSprite.IsValid())
            {
                memberLdSprite.ReleaseAsset();
            }
            LdSprite = null;

            if (memberSDSprite != null && memberSDSprite.IsValid())
            {
                memberSDSprite.ReleaseAsset();
            }
            SDsprite = null;
            
            if ( memberIconSprite != null && memberIconSprite.IsValid())
            {
                memberIconSprite.ReleaseAsset();
            }
            IconSprite = null;

            if (memberStatList != null)
            {
                foreach (var statRef in memberStatList)
                {
                    if (statRef != null && statRef.IsValid())
                    {
                        statRef.ReleaseAsset();
                    }
                }
            }

            Stats?.Clear();
            _isLoaded = false;

            Debug.Log($"[MemberDataSO] {memberListName} assets unloaded");
        }

        private void OnDestroy()
        {
            UnloadAssets();
        }

    }
}