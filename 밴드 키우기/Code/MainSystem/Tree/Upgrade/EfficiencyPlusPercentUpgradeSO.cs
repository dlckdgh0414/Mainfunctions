using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 합주나 곡 제작시 프로젝타일 나오는 양 퍼센트로 증가
    /// </summary>
    [CreateAssetMenu(fileName = "EfficiencyPlusPercentUpgrade", menuName = "SO/Tree/Upgrade/EfficiencyPlusPercentUpgrade", order = 0)]
    public class EfficiencyPlusPercentUpgradeSO : BaseUpgradeSO
    {
        public int plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseBehaviorAddon;
            eventAddon.ActivityEfficiencyPlusPercent += plusValue;
        }
    }
}