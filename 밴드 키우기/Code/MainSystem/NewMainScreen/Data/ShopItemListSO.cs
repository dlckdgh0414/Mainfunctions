using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "ItemList", menuName = "SO/Shop/List", order = 0)]
    public class ShopItemListSO : ScriptableObject
    {
        public List<ShopItemDataSO>  itemList = new List<ShopItemDataSO>();
    }
}