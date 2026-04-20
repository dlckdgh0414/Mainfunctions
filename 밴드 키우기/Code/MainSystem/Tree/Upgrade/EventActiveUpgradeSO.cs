using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 이벤트 발생을 가능하게 해줌
    /// </summary>
    [CreateAssetMenu(fileName = "EventActiveUpgrade", menuName = "SO/Tree/Upgrade/EventActiveUpgrade", order = 0)]
    public class EventActiveUpgradeSO : BaseUpgradeSO
    {
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseEventAddon;
            eventAddon.IsEventActive = true;
        }
    }
}