using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 합주나 곡 제작시 재개될 확률이 증가합니다(0.XX식)
    /// </summary>
    [CreateAssetMenu(fileName = "OneMoreChanceUpgrade", menuName = "SO/Tree/Upgrade/OneMoreChanceUpgrade", order = 0)]
    public class OneMoreChanceUpgradeSO : BaseUpgradeSO
    {
        public float plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseBehaviorAddon;
            eventAddon.OneMoreChancePlusValue += plusValue;
        }
    }
}