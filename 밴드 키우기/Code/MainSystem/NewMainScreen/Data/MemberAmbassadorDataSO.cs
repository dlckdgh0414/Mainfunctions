using System.Collections.Generic;
using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Data
{
    [CreateAssetMenu(fileName = "Bass", menuName = "SO/Ambassador/Data", order = 0)]
    public class MemberAmbassadorDataSO : ScriptableObject
    {
        public MemberType memberType;
        public string memberName;
        [TextArea]
        public List<string> AmbassadorDataList;
    }
}