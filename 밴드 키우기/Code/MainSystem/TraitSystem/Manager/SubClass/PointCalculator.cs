using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.Manager.SubClass
{
    public class PointCalculator : MonoBehaviour, IPointCalculator
    {
        public int CalculateTotalPoints(IEnumerable<ActiveTrait> traits)
        {
            return traits.Sum(t => t.Data.Point);
        }
    }
}