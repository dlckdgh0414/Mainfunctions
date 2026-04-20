using Code.MainSystem.TraitSystem.Data;

namespace Code.Core.Bus.GameEvents
{
    public struct SynergyActivatedEvent : IEvent
    {
        public string SynergyName;
        public float FeverBonus;
        public float ScoreBonus;

        public SynergyActivatedEvent(string name, float fever, float score)
        {
            SynergyName = name;
            FeverBonus = fever;
            ScoreBonus = score;
        }
    }
}