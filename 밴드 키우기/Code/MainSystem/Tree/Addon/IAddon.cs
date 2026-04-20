using System.Collections.Generic;
using Code.MainSystem.Tree.Upgrade;

namespace Code.MainSystem.Tree.Addon
{
    public interface IAddon
    {
        public List<BaseUpgradeSO> Upgrades { get; }
        
        public void ApplyAllUpgrades();
        public void ResetUpgradeValue();

        public void EventHandle();
    }
}