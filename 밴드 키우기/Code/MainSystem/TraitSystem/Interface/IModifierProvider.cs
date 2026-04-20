using System.Collections.Generic;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.TraitSystem.Interface
{
    public interface IModifierProvider
    {
        float GetCalculatedStat(TraitTarget category, float baseValue, object context = null);
        IEnumerable<T> GetModifiers<T>() where T : class;
    }
}