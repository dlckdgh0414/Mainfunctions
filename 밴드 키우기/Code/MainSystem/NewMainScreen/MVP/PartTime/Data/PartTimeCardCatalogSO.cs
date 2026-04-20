using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.PartTime.Data
{
    /// <summary>
    /// 아르바이트 카드 카탈로그 설정 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "PartTimeCardCatalog", menuName = "SO/MainScreen/PartTime/CardCatalog")]
    public class PartTimeCardCatalogSO : ScriptableObject
    {
        [Header("Cards")]
        [SerializeField] private List<PartTimeCardConfigSO> cards = new List<PartTimeCardConfigSO>();

        [Header("Balance Log")]
        [SerializeField] private string catalogVersion = "v1.0.0";
        [SerializeField] [TextArea(2, 6)] private string catalogMemo;

        public IReadOnlyList<PartTimeCardConfigSO> Cards => cards;
        public string CatalogVersion => catalogVersion;
        public string CatalogMemo => catalogMemo;
    }
}
