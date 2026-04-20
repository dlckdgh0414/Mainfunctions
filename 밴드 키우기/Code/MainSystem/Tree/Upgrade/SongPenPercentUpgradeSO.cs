using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    [CreateAssetMenu(fileName = "SongPenPercentUpgrade", menuName = "SO/Tree/Upgrade/SongPenPercentUpgrade", order = 0)]
    public class SongPenPercentUpgradeSO : BaseUpgradeSO
    {
        public float plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseSongAddon;
            eventAddon.PercentSongPenValue += plusValue;
        }
    }
}