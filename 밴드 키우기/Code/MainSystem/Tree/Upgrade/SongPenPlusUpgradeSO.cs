using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 곡을 통해 얻는 팬이 수치만큼 증가합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SongPenPlusUpgrade", menuName = "SO/Tree/Upgrade/SongPenPlusUpgrade", order = 0)]
    public class SongPenPlusUpgradeSO : BaseUpgradeSO
    {
        public int plusValue;
        public override void Upgrade(IAddon addon)
        {
            var eventAddon = addon as BaseSongAddon;
            eventAddon.PlusSongPenValue += plusValue;
        }
    }
}