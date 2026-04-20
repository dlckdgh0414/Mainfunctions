using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Interface;
using Code.MainSystem.TraitSystem.Manager;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.TraitExtensionMethod;

namespace Code.MainSystem.TraitSystem.Data
{
    public class CharacterTrait : MonoBehaviour, ITraitHolder
    {
        [SerializeField] private MemberType memberType;
        [SerializeField] private int maxPoints;
    
        public MemberType MemberType => memberType;
        
        private readonly List<ActiveTrait> _activeTraits = new();
        private readonly List<object> _modifiers = new();

        public int TotalPoint => _activeTraits.Sum(t => t.Data.Point);
        public int MaxPoints => maxPoints;
        public IReadOnlyList<ActiveTrait> ActiveTraits => _activeTraits;
        public bool IsAdjusting { get; private set; }
        public TraitDataSO PendingTrait { get; private set; }

        private void Start()
        {
            TraitManager.Instance.RegisterHolder(this);
        }

        public void AddTrait(TraitDataSO data)
        {
            if (data is null) return;
    
            ActiveTrait newTrait = new ActiveTrait(data, memberType);
            _activeTraits.Add(newTrait);
            
            if (newTrait.TraitEffect != null)
                RegisterModifier(newTrait.TraitEffect);
        }

        public void RemoveActiveTrait(ActiveTrait trait)
        {
            if (trait == null) return;
            
            if (trait.TraitEffect != null)
                UnregisterModifier(trait.TraitEffect);

            _activeTraits.Remove(trait);
        }
        
        public void ExecuteTrigger(TraitTrigger trigger, object context = null)
        {
            foreach (var trait in _activeTraits)
                trait.TraitEffect?.OnTrigger(trigger, context);
        }

        public float QueryTriggerValue(TraitTrigger trigger, float baseValue, object context = null)
        {
            float flatBonus = 0f;
            float percentBonus = 0f;
            float finalMulti = 1f;
            bool hasAnyEffect = false;

            foreach (var trait in _activeTraits)
            {
                if (trait.TraitEffect == null) continue;

                float amount = trait.TraitEffect.QueryValue(trigger, context);
                if (amount == 0) continue;

                hasAnyEffect = true;

                CalculationType calcType = CalculationType.PercentAdd; 
                if (trait.Data.Impacts != null && trait.Data.Impacts.Count > 0)
                    calcType = trait.Data.Impacts[0].CalcType;

                switch (calcType)
                {
                    case CalculationType.Additive:
                        flatBonus += amount; break;
                    case CalculationType.Subtractive:
                        flatBonus -= amount; break;
                    case CalculationType.PercentAdd:
                        percentBonus += amount * 0.01f; break;
                    case CalculationType.PercentSub:
                        percentBonus -= amount * 0.01f; break;
                    case CalculationType.Multiplicative:
                        if (amount != 0) finalMulti *= amount; break;
                }
            }
            
            if (!hasAnyEffect) return baseValue;
            
            float result = (baseValue + flatBonus) * (1f + percentBonus) * finalMulti;
    
            return Mathf.Max(0, result);
        }

        public bool CheckTriggerCondition(TraitTrigger trigger, object context = null)
        {
            return _activeTraits.Any(t => t.TraitEffect != null && t.TraitEffect.CheckCondition(trigger, context));
        }

        public void RestoreTraits(IEnumerable<ActiveTrait> traits)
        {
            if (traits == null)
                return;
            
            _activeTraits.Clear();
            _modifiers.Clear();
            
            foreach (var trait in traits)
            {
                _activeTraits.Add(trait);
                
                if (trait.TraitEffect != null)
                    RegisterModifier(trait.TraitEffect);
            }
        }

        public void BeginAdjustment(TraitDataSO pendingTrait)
        {
            IsAdjusting = true;
            PendingTrait = pendingTrait;
        }

        public void EndAdjustment()
        {
            IsAdjusting = false;
            PendingTrait = null;
            Bus<TraitAdjusted>.Raise(new TraitAdjusted());
        }

        public float GetCalculatedStat(TraitTarget category, float baseValue, object context = null)
        {
            return TraitCalculator.GetCalculatedStat(this, category, baseValue, context);
        }

        public IEnumerable<T> GetModifiers<T>() where T : class
        {
            return _modifiers.OfType<T>();
        }

        private void RegisterModifier(object modifier)
        {
            if (!_modifiers.Contains(modifier))
                _modifiers.Add(modifier);
        }

        private void UnregisterModifier(object modifier)
        {
            _modifiers.Remove(modifier);
        }
    }
}