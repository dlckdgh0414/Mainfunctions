using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.Tree.Addon
{
    public class BaseEventAddon : MonoBehaviour, IAddon
    {
        public bool IsEventActive { get; set; }
        public List<BaseUpgradeSO> Upgrades { get; } = new List<BaseUpgradeSO>();
        
        public void ApplyAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            {
                upgrade.Upgrade(this);
            }
        }

        public void ResetUpgradeValue()
        {
            
        }

        public void EventHandle()
        {
            Bus<TreeUpgradeEvent>.OnEvent += HandleAddUpgrade;
        }

        protected virtual void OnDestroy()
        {
            Bus<TreeUpgradeEvent>.OnEvent -= HandleAddUpgrade;
        }
        
        private void HandleAddUpgrade(TreeUpgradeEvent evt)
        {
            if(evt.Type != TreeUpgradeType.UnlockEvent) return;
            Upgrades.Add(evt.UpgradeSO);
        }
    }
}