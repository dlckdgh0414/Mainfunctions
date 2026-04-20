#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;

namespace Code.Editor
{
    public class PromotionGenerator : EditorWindow
    {
        [MenuItem("Tools/홍보 아이템 자동 생성")]
        public static void GeneratePromotions()
        {
            string rootFolder  = "Assets\\_Modules\\Promotion";
            string dataFolder  = rootFolder + "/Data";
            string listFolder  = rootFolder + "/List";

            CreateFolderIfNotExists("Assets/SO", "Promotion");
            CreateFolderIfNotExists(rootFolder, "Data");
            CreateFolderIfNotExists(rootFolder, "List");

            // 초급 5개
            var lowList = new List<PromotionDataSO>
            {
                CreatePromotion(dataFolder, "Promotion_Low_01",
                    "SNS 홍보",       300,   50,   0),
                CreatePromotion(dataFolder, "Promotion_Low_02",
                    "전단지 배포",     500,   100,  100),
                CreatePromotion(dataFolder, "Promotion_Low_03",
                    "버스킹 공연",     800,   180,  250),
                CreatePromotion(dataFolder, "Promotion_Low_04",
                    "지역 커뮤니티",   1200,  280,  400),
                CreatePromotion(dataFolder, "Promotion_Low_05",
                    "유튜브 광고",     1800,  400,  600),
            };

            // 중급 5개
            var midList = new List<PromotionDataSO>
            {
                CreatePromotion(dataFolder, "Promotion_Mid_01",
                    "버스 광고",       2500,  600,  1000),
                CreatePromotion(dataFolder, "Promotion_Mid_02",
                    "음악 잡지 인터뷰", 4000, 1000,  2000),
                CreatePromotion(dataFolder, "Promotion_Mid_03",
                    "지역 라디오",     6000,  1500,  3500),
                CreatePromotion(dataFolder, "Promotion_Mid_04",
                    "음악 페스티벌",   8000,  2200,  5000),
                CreatePromotion(dataFolder, "Promotion_Mid_05",
                    "케이블 TV 광고",  12000, 3000,  7000),
            };

            // 고급 5개
            var highList = new List<PromotionDataSO>
            {
                CreatePromotion(dataFolder, "Promotion_High_01",
                    "지상파 라디오",   15000, 4000,  10000),
                CreatePromotion(dataFolder, "Promotion_High_02",
                    "음악 시상식 참가", 25000, 7000,  15000),
                CreatePromotion(dataFolder, "Promotion_High_03",
                    "지상파 TV 광고",  40000, 12000, 25000),
                CreatePromotion(dataFolder, "Promotion_High_04",
                    "대형 콘서트 홍보", 60000, 20000, 40000),
                CreatePromotion(dataFolder, "Promotion_High_05",
                    "전국 투어 홍보",  100000,35000, 70000),
            };

            // PromotionListSO 3개 생성
            CreatePromotionList(listFolder, "PromotionList_Low",  lowList);
            CreatePromotionList(listFolder, "PromotionList_Mid",  midList);
            CreatePromotionList(listFolder, "PromotionList_High", highList);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료",
                "홍보 SO 15개 + PromotionList 3개 생성 완료!\n" +
                "경로: Assets/SO/Promotion", "확인");

            Debug.Log("[PromotionGenerator] 생성 완료");
        }

        private static PromotionDataSO CreatePromotion(
            string folder,
            string fileName,
            string promotionName,
            int price,
            int addFans,
            int requiredFans)
        {
            var so = ScriptableObject.CreateInstance<PromotionDataSO>();
            so.promotionName = promotionName;
            so.promotionPrice = price;
            so.addFans       = addFans;
            so.requiredFans  = requiredFans;

            AssetDatabase.CreateAsset(so, $"{folder}/{fileName}.asset");
            return so;
        }

        private static void CreatePromotionList(
            string folder,
            string fileName,
            List<PromotionDataSO> list)
        {
            var so = ScriptableObject.CreateInstance<PromotionListSO>();
            so.promotionList = list;
            AssetDatabase.CreateAsset(so, $"{folder}/{fileName}.asset");
        }

        private static void CreateFolderIfNotExists(string parent, string folderName)
        {
            string fullPath = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(fullPath))
                AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif