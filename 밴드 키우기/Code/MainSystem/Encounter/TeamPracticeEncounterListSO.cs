using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.MainSystem.Encounter
{
    [Serializable]
    public struct TeamPracticeEncounter
    {
        public FlagMember members;
        public EncounterDataSO encounterData;
    }
    
    [CreateAssetMenu(fileName = "TeamPracticeEncounterList", menuName = "SO/Encounter/TeamPracticeList", order = 0)]
    public class TeamPracticeEncounterListSO : ScriptableObject
    {
        public List<TeamPracticeEncounter> list;
    }
}