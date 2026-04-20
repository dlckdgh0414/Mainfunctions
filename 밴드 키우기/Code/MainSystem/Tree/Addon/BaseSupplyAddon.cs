using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.Tree.Addon
{
    public class BaseSupplyAddon : MonoBehaviour, IAddon
    {
        public List<BaseUpgradeSO> Upgrades { get; protected set; } = new List<BaseUpgradeSO>();
        
        /// <summary>
        /// 경험치 증가시 추가 증가
        /// </summary>
        public int ExpPlusValue { get; set; }
        
        public void ApplyAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            {
                upgrade.Upgrade(this);
            }
        }

        public void ResetUpgradeValue()
        {
            ExpPlusValue = 0;
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
            if(evt.Type != TreeUpgradeType.Supply) return;
            Upgrades.Add(evt.UpgradeSO);
        }
    }
}