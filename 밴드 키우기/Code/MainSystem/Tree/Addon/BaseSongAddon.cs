using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Upgrade;
using UnityEngine;

namespace Code.MainSystem.Tree.Addon
{
    public class BaseSongAddon : MonoBehaviour, IAddon
    {
        public int PlusSongGoldValue { get; set; }
        public float PercentSongGoldValue { get; set; }
        public int PlusSongPenValue { get; set; }
        public float PercentSongPenValue { get; set; }
        
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
            PlusSongGoldValue = 0;
            PlusSongPenValue = 0;
            PercentSongGoldValue = 0;
            PercentSongPenValue = 0;
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
            if(evt.Type != TreeUpgradeType.SongResult) return;
            Debug.Log(evt.UpgradeSO.effectDescription);
            Upgrades.Add(evt.UpgradeSO);
        }
    }
}