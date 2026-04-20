using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Manager.SubClass;

namespace Code.MainSystem.TraitSystem.Interface
{
    public interface ITraitValidator
    {
        ValidationResult CanAdd(ITraitHolder holder, TraitDataSO trait);
        ValidationResult CanRemove(ITraitHolder holder, ActiveTrait trait);
    }
}