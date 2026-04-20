using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;

namespace Code.MainSystem.TraitSystem.TraitEffect
{
    public struct EnsembleContext
    {
        public Dictionary<(MemberType, StatType), int> DeltaDict;
        
        public StatType MainStatType;
        
        public List<MemberType> Participants;
        
        public EnsembleContext(Dictionary<(MemberType, StatType), int> deltaDict, StatType mainStatType, List<MemberType> participants)
        {
            DeltaDict = deltaDict;
            MainStatType = mainStatType;
            Participants = participants;
        }
    }
}