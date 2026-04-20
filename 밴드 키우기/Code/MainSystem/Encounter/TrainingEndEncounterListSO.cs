using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Encounter
{
    [Serializable]
    public struct TrainingEndEncounter
    {
        public EncounterDataSO encounterData;
    }
    
    
    [CreateAssetMenu(fileName = "TrainingEndEncounterList", menuName = "SO/Encounter/TrainingEndList", order = 0)]
    public class TrainingEndEncounterListSO : ScriptableObject
    {
        public List<TrainingEndEncounter> list;
    }
}