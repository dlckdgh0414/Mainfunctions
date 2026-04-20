using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Encounter
{
    [CreateAssetMenu(fileName = "EncounterList", menuName = "SO/Encounter/EncounterList", order = 0)]
    public class CurrentEncounterListSO : ScriptableObject
    {
        public List<EncounterDataSO> encounters;
    }
}