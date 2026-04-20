using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 곡을 통해 얻는 골드가 수치만큼 증가합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SongGoldPercentUpgrade", menuName = "SO/Tree/Upgrade/SongGoldPercentUpgrade", order = 0)]
    public class SongGoldPercentUpgradeSO : BaseUpgradeSO
    {
        public float plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseSongAddon;
            eventAddon.PercentSongGoldValue += plusValue;
        }
    }
}