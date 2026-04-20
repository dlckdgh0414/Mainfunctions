using Code.MainSystem.Tree.Upgrade;

namespace Code.Core.Bus.GameEvents.TreeEvents
{
    public enum TreeUpgradeType
    {
        GameStat, // 음악 스텟 기본값
        SongResult, // 음악 결과에서 얻는 양 증가
        GoldGetToPartTime, // 알바로 얻는 골드
        Supply, // 자원 획득량 증가
        Behavior, // 합주나 곡제작쪽
        UnlockEvent, // 이벤트 해금
    }
    
    public struct TreeUpgradeEvent : IEvent
    {
        public TreeUpgradeType Type;
        public BaseUpgradeSO UpgradeSO;
        
        public TreeUpgradeEvent(TreeUpgradeType type, BaseUpgradeSO upgradeSO)
        {
            Type = type;
            UpgradeSO = upgradeSO;
        }
    }
}