using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 합주나 곡 제작시 프로젝타일 나오는 양 고정 증가
    /// </summary>
    [CreateAssetMenu(fileName = "EfficiencyPlusValueUpgrade", menuName = "SO/Tree/Upgrade/EfficiencyPlusValueUpgrade", order = 0)]
    public class EfficiencyPlusValueUpgradeSO : BaseUpgradeSO
    {
        public int plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseBehaviorAddon;
            eventAddon.ActivityEfficiencyPlusValue += plusValue;
        }
    }
}