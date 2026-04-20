using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "Item", menuName = "SO/Shop/Item", order = 0)]
    public class ShopItemDataSO : ScriptableObject
    {
        public Sprite icon;
        public string itemName;
        [TextArea]
        public string itemDesc;
        public int itemPrice;

        [Header("효과")]
        public ShopItemEffectType effectType;
        public ShopItemGrade grade;
        public int effectValue;
        public MusicRelatedStatsType targetStatType;
    }
}