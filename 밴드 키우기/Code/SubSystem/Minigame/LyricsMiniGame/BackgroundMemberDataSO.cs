using System.Collections.Generic;
using UnityEngine;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    [CreateAssetMenu(fileName = "MemberData", menuName = "SO/LyricsMiniGame/MemberData", order = 0)]
    public class BackgroundMemberDataSO : ScriptableObject
    {
        public string MemberName;

        [TextArea(1, 3)]
        public List<string> GoodReactions = new List<string>
        {
            "오예!!",
            "잘한다~!",
            "최고야!"
        };

        [TextArea(1, 3)]
        public List<string> BadReactions = new List<string>
        {
            "아이고...",
            "으악!",
            "아 아깝다"
        };
    }
}