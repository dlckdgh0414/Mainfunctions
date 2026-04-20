using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.Tree.Addon
{
    public class BaseGameStatAddon : MonoBehaviour, IAddon
    {
        public int BaseSongLyricsValue { get; set; }
        public int BaseSongTeamworkValue { get; set; }
        public int BaseSongProficiencyValue { get; set; }
        public int BaseSongMelodyValue { get; set; }
        
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
            BaseSongLyricsValue = 0;
            BaseSongTeamworkValue = 0;
            BaseSongProficiencyValue = 0;
            BaseSongMelodyValue = 0;
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
            if(evt.Type != TreeUpgradeType.GameStat) return;
            Upgrades.Add(evt.UpgradeSO);
        }
    }
}