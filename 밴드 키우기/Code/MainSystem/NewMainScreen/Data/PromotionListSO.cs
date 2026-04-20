using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "PromotionList", menuName = "SO/Promotion/PromotionList")]
    public class PromotionListSO : ScriptableObject
    {
        public List<PromotionDataSO> promotionList;
    }
}