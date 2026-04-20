using System.Threading.Tasks;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.Interface
{
    public interface ITraitRegistry
    {
        Task InitializationTask { get; }

        Task Initialize();
        TraitDataSO Get(int hash);
        bool Contains(int traitHash);
    }
}