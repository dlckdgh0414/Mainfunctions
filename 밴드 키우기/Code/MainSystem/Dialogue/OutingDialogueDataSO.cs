using System;
using System.Collections.Generic;
using Code.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.NewMainScreen.Data
{
    /// <summary>
    /// (멤버 타입 + 지역 타입) 조합에 대응하는 다이알로그 목록 하나의 항목
    /// </summary>
    [Serializable]
    public class OutingDialogueEntry
    {
        public MemberType memberType;
        public LocationType locationType;
        public List<AssetReference> dialogues = new();
    }

    /// <summary>
    /// 외출 다이알로그 데이터베이스 SO
    /// 멤버 + 지역 조합으로 등록된 다이알로그 중 하나를 랜덤 반환
    /// </summary>
    [CreateAssetMenu(fileName = "OutingDialogueData", menuName = "SO/Outing/DialogueData")]
    public class OutingDialogueDataSO : ScriptableObject
    {
        [SerializeField] private List<OutingDialogueEntry> entries = new();

        /// <summary>
        /// 해당 멤버 + 지역 조합에서 AssetReference를 랜덤으로 하나 반환.
        /// 없으면 null 반환.
        /// </summary>
        public AssetReference GetRandom(MemberType memberType, LocationType locationType)
        {
            List<AssetReference> candidates = new();

            foreach (OutingDialogueEntry entry in entries)
            {
                if (entry.memberType == memberType && entry.locationType == locationType)
                {
                    foreach (AssetReference assetRef in entry.dialogues)
                    {
                        if (assetRef != null && assetRef.RuntimeKeyIsValid())
                            candidates.Add(assetRef);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[OutingDialogueDataSO] No dialogue found for {memberType} / {locationType}");
                return null;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }
}