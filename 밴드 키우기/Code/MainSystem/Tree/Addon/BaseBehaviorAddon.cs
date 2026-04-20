using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.Tree.Addon
{
    public abstract class BaseBehaviorAddon : MonoBehaviour, IAddon
    {
        public List<BaseUpgradeSO> Upgrades { get; protected set; } = new List<BaseUpgradeSO>();
        
        /// <summary>
        /// 한번에 나오는 개수 증가하는 비율(%)
        /// </summary>
        public int ActivityEfficiencyPlusPercent { get; set; }
        
        /// <summary>
        /// 개수 고정 증가 비율
        /// </summary>
        public int ActivityEfficiencyPlusValue { get; set; }
        
        /// <summary>
        /// 회차가 오를 확률 0.XX로 기록
        /// </summary>
        public float OneMoreChancePlusValue { get; set; }
        
        public void ApplyAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            {
                upgrade.Upgrade(this);
            }
        }

        public void ResetUpgradeValue()
        {
            ActivityEfficiencyPlusPercent = 0;
            ActivityEfficiencyPlusValue = 0;
            OneMoreChancePlusValue = 0;
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
            if(evt.Type != TreeUpgradeType.Behavior) return;
            Upgrades.Add(evt.UpgradeSO);
        }
    }
}