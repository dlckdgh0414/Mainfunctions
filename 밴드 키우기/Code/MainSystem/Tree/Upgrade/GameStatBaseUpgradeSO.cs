using Code.Core;
using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 곡을 새로이 만들떄, 지정한 스텟의 기본값이 상승함.
    /// </summary>
    [CreateAssetMenu(fileName = "GameStatUpgrade", menuName = "SO/Tree/Upgrade/GameStatUpgrade", order = 0)]
    public class GameStatBaseUpgradeSO : BaseUpgradeSO
    {
        public MusicRelatedStatsType statType; 
        public int plusValue;
        public override void Upgrade(IAddon addon)
        {
            var upgradeAddon = addon as BaseGameStatAddon;
            switch (statType)
            {
                case MusicRelatedStatsType.Lyrics:
                    upgradeAddon.BaseSongLyricsValue += plusValue;
                    break;
                case MusicRelatedStatsType.Teamwork:
                    upgradeAddon.BaseSongTeamworkValue += plusValue;
                    break;
                case MusicRelatedStatsType.Proficiency:
                    upgradeAddon.BaseSongProficiencyValue += plusValue;
                    break;
                case MusicRelatedStatsType.Melody:
                    upgradeAddon.BaseSongMelodyValue += plusValue;
                    break;
                default:
                    Debug.LogError($"{statType}이 음악 스텟이 아닙니다");
                    break;
            }
        }
    }
}