using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "PromotionData", menuName = "SO/Promotion/PromotionData")]
    public class PromotionDataSO : ScriptableObject
    {
        public string promotionName;
        public int promotionPrice;
        public int addFans;
        public int requiredFans;
    }
}