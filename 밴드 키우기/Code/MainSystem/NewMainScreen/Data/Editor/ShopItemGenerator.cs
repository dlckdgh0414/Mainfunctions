#if UNITY_EDITOR
using Code.Core;
using UnityEditor;
using UnityEngine;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.MusicRelated;

namespace Code.Editor
{
    public class ShopItemGenerator : EditorWindow
    {
        [MenuItem("Tools/상점 아이템 자동 생성")]
        public static void GenerateItems()
        {
            string folder = "Assets/_Modules/Shop/Data";

            // 폴더 없으면 생성
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets\\_Modules\\Shop\\Data", "Items");
            }

            var itemList = ScriptableObject.CreateInstance<ShopItemListSO>();
            itemList.itemList = new System.Collections.Generic.List<ShopItemDataSO>();

            // 작곡 스탯
            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Composition_Low",
                "작곡 입문 악보", "랜덤 멤버의 작곡 실력이 소폭 증가합니다.",
                500, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.Low, 30, MusicRelatedStatsType.Composition));

            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Composition_Mid",
                "작곡 연습 교재", "랜덤 멤버의 작곡 실력이 증가합니다.",
                1500, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.Mid, 80, MusicRelatedStatsType.Composition));

            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Composition_High",
                "작곡 전속 코치", "원하는 멤버의 작곡 실력을 크게 올립니다.",
                4000, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.High, 180, MusicRelatedStatsType.Composition));

            // 악기숙련도 스탯
            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Instrument_Low",
                "악기 입문 교본", "랜덤 멤버의 악기 숙련도가 소폭 증가합니다.",
                500, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.Low, 30, MusicRelatedStatsType.InstrumentProficiency));

            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Instrument_Mid",
                "악기 연습 교재", "랜덤 멤버의 악기 숙련도가 증가합니다.",
                1500, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.Mid, 80, MusicRelatedStatsType.InstrumentProficiency));

            itemList.itemList.Add(CreateItem(folder, "Item_Stat_Instrument_High",
                "악기 전속 코치", "원하는 멤버의 악기 숙련도를 크게 올립니다.",
                4000, ShopItemEffectType.MemberStatIncrease, ShopItemGrade.High, 180, MusicRelatedStatsType.InstrumentProficiency));

            // 행동 효율
            itemList.itemList.Add(CreateItem(folder, "Item_Efficiency_Low",
                "연습실 대여", "활동 시 프로젝타일 수가 10% 증가합니다.",
                500, ShopItemEffectType.ActivityEfficiencyBonus, ShopItemGrade.Low, 10));

            itemList.itemList.Add(CreateItem(folder, "Item_Efficiency_Mid",
                "스튜디오 대여", "활동 시 프로젝타일 수가 20% 증가합니다.",
                1500, ShopItemEffectType.ActivityEfficiencyBonus, ShopItemGrade.Mid, 20));

            itemList.itemList.Add(CreateItem(folder, "Item_Efficiency_High",
                "전문 스튜디오", "활동 시 프로젝타일 수가 35% 증가합니다.",
                4000, ShopItemEffectType.ActivityEfficiencyBonus, ShopItemGrade.High, 35));

            // 컨디션 회복
            itemList.itemList.Add(CreateItem(folder, "Item_Condition_Low",
                "영양제", "컨디션이 가장 낮은 멤버의 컨디션이 1단계 회복됩니다.",
                500, ShopItemEffectType.ConditionRecovery, ShopItemGrade.Low, 1));

            itemList.itemList.Add(CreateItem(folder, "Item_Condition_Mid",
                "휴식 패키지", "컨디션이 가장 낮은 멤버의 컨디션이 2단계 회복됩니다.",
                1500, ShopItemEffectType.ConditionRecovery, ShopItemGrade.Mid, 2));

            itemList.itemList.Add(CreateItem(folder, "Item_Condition_High",
                "완전 휴양", "컨디션이 가장 낮은 멤버의 컨디션이 완전히 회복됩니다.",
                4000, ShopItemEffectType.ConditionRecovery, ShopItemGrade.High, 99));

            // ShopItemListSO 저장
            AssetDatabase.CreateAsset(itemList, $"{folder}/ShopItemList.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", $"상점 아이템 {itemList.itemList.Count}개 + ShopItemList 생성 완료!\n경로: {folder}", "확인");
            Debug.Log($"[ShopItemGenerator] 생성 완료: {folder}");
        }

        private static ShopItemDataSO CreateItem(
            string folder,
            string fileName,
            string itemName,
            string itemDesc,
            int itemPrice,
            ShopItemEffectType effectType,
            ShopItemGrade grade,
            int effectValue,
            MusicRelatedStatsType targetStatType = MusicRelatedStatsType.Composition)
        {
            var item = ScriptableObject.CreateInstance<ShopItemDataSO>();
            item.itemName       = itemName;
            item.itemDesc       = itemDesc;
            item.itemPrice      = itemPrice;
            item.effectType     = effectType;
            item.grade          = grade;
            item.effectValue    = effectValue;
            item.targetStatType = targetStatType;

            AssetDatabase.CreateAsset(item, $"{folder}/{fileName}.asset");
            return item;
        }
    }
}
#endif