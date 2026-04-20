using UnityEngine;

namespace Code.SubSystem.Minigame
{
    [CreateAssetMenu(fileName = "QTE", menuName = "SO/MiniGame/Description", order = 0)]
    public class MiniGameDescriptionDataSO : ScriptableObject
    {
        public string name;
        [TextArea]
        public string description;
    }
}