using System.Linq;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;

namespace Code.MainSystem.TraitSystem.TraitEffect
{
    public abstract class AbstractTraitEffect
    {
        protected ActiveTrait _ownerTrait;
        
        public virtual void Initialize(ActiveTrait trait) 
            => _ownerTrait = trait;

        protected float GetValue(int index) => _ownerTrait.CurrentEffects.ElementAtOrDefault(index);
        
        public virtual void OnTrigger(TraitTrigger trigger, object context = null) { }
        public virtual float QueryValue(TraitTrigger trigger, object context = null) => 0f;
        public virtual bool CheckCondition(TraitTrigger trigger, object context = null) => false;
        
        public abstract bool IsTargetStat(TraitTarget category);
        public abstract float GetAmount(TraitTarget category, object context = null);
    }
}