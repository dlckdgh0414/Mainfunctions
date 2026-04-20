using System.Linq;
using System.Threading.Tasks;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.Song;
using Code.SubSystem.Minigame.Common.Management;
using UnityEngine;

namespace Code.SubSystem.UI
{
    public class PersonalStatBarContainer : MonoBehaviour
    {
        [Header("SO")]
        [SerializeField] protected MemberThrowDataSO memberThrowSO;
        [SerializeField] private MiniGameResultSenderSO senderSO;
        
        [SerializeField] private PersonalStatBar statBarPrefab;
        
        public async Task PlayUIAnimation()
        {
            for (int i = 0; i < senderSO.ChangeMemberStats.Count; i++)
            {
                PersonalStatBar bar = Instantiate(statBarPrefab, transform);
                var data = senderSO.ChangeMemberStats[i];
                var memberData = memberThrowSO.CurrentMembers
                    .FirstOrDefault(m => m.memberType == data.Item1);
                if (memberData != null)
                {
                }
                    
            }
        }
    }
}