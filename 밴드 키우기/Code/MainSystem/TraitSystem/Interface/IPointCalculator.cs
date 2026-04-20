using System.Collections.Generic;
using Code.MainSystem.TraitSystem.Runtime;

namespace Code.MainSystem.TraitSystem.Interface
{
    public interface IPointCalculator
    {
        int CalculateTotalPoints(IEnumerable<ActiveTrait> traits);
    }
}