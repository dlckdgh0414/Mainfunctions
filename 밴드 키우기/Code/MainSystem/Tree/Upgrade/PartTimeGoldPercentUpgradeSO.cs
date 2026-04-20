using Code.MainSystem.NewMainScreen.MVP.PartTime;
using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 알바로 얻는 골드가 0.XX% 증가합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PartTimeGoldPlusUpgrade", menuName = "SO/Tree/Upgrade/PartTimeGoldPlusUpgrade", order = 0)]
    public class PartTimeGoldPercentUpgradeSO : BaseUpgradeSO
    {
        public float plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as PartTimeExecutor;
            eventAddon.PartTimeGetGoldPlusValue += plusValue;
        }
    }
}