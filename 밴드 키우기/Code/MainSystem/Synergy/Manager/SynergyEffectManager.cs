using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SynergyEvents;
using Code.MainSystem.Synergy.Data;
using Code.MainSystem.Synergy.Effect;
using Code.MainSystem.Synergy.Runtime;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Interface;
using Code.MainSystem.TraitSystem.Manager;
using UnityEngine;

namespace Code.MainSystem.Synergy.Manager
{
    public class SynergyEffectManager : MonoBehaviour
    {
        [SerializeField] private List<TraitSynergyDataSO> synergyDataList;

        public static SynergyEffectManager Instance { get; private set; }
        public bool Initialize { get; private set; }
        
        public IReadOnlyList<ActiveSynergy> ActiveSynergies => _activeSynergies;
        private readonly List<ActiveSynergy> _activeSynergies = new();
        private readonly Dictionary<TraitSynergyDataSO, AbstractSynergyEffect> _activeEffects = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Debug.Assert(synergyDataList is { Count: > 0 }, $"{name} : Synergy data is missing!");
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }
        
        private void Start() 
        {
            InternalInitialize();
        }

        private void OnEnable()
        {
            Bus<SynergyUpdateEvent>.OnEvent += HandleSynergyUpdate;
        }

        private void OnDestroy()
        {
            Bus<SynergyUpdateEvent>.OnEvent -= HandleSynergyUpdate;
        }

        private void HandleSynergyUpdate(SynergyUpdateEvent evt)
        {
            UpdateSynergyState(evt.Holders);
        }

        private async void InternalInitialize()
        {
            try
            {
                Initialize = false;
                await TraitManager.Instance.ReadyTask;
            
                _activeSynergies.Clear();
                foreach (var data in synergyDataList)
                {
                    var traitsWithTag = TraitManager.Instance.GetTraitsByTag(data.TargetTag);
                    _activeSynergies.Add(new ActiveSynergy(data, traitsWithTag));
                }
                Initialize = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 데이터 변경 시 호출: 인스턴스를 새로 만들지 않고 내부 데이터만 갱신합니다.
        /// </summary>
        private void UpdateSynergyState(IReadOnlyDictionary<MemberType, ITraitHolder> holders)
        {
            var allActiveTraits = holders.Values
                .SelectMany(h => h.ActiveTraits)
                .Select(t => t.Data)
                .ToList();

            foreach (var activeSynergy in _activeSynergies)
            {
                activeSynergy.UpdateState(allActiveTraits);

                var data = activeSynergy.SynergyData;
                bool shouldBeActive = data.Thresholds.Count > 0 && activeSynergy.CurrentCount >= data.Thresholds[0];
                
                if (shouldBeActive)
                {
                    if (_activeEffects.ContainsKey(data)) 
                        continue;
                    
                    var effect = data.CreateEffectInstance(activeSynergy);
                    if (effect != null) 
                        _activeEffects.Add(data, effect);
                }
                else
                {
                    _activeEffects.Remove(data);
                }
            }
        }

        public ActiveSynergy GetActiveSynergy(TraitTag traitTag)
        {
            return _activeSynergies.FirstOrDefault(s => s.SynergyData.TargetTag == traitTag);
        }

        public float QueryValue(TraitTag traitTag, SynergyTrigger trigger, object context = null)
        {
            var effects = _activeEffects.Values
                .Where(e => e.IsTargetStat(traitTag))
                .ToList();

            float defaultValue = IsMultiplicativeTag(traitTag) ? 1.0f : 0.0f;
            if (effects.Count == 0) 
                return defaultValue;

            return IsMultiplicativeTag(traitTag)
                ? effects.Aggregate(1.0f, (acc, effect) => acc * effect.QueryValue(trigger, context))
                : effects.Sum(effect => effect.QueryValue(trigger, context));
        }

        private bool IsMultiplicativeTag(TraitTag traitTag)
        {
            return traitTag is TraitTag.Immersion or TraitTag.Genius or TraitTag.Solo;
        }

        public void SendTrigger(TraitTag traitTag, SynergyTrigger trigger, object context = null)
        {
            var targets = _activeEffects.Values.Where(e => e.IsTargetStat(traitTag));
            foreach (var effect in targets)
            {
                effect.OnTrigger(trigger, context);
            }
        }
        
        public void Tick()
        {
            foreach (var effect in _activeEffects.Values)
                effect.OnUpdate();
        }
        
        public void ResetAll()
        {
            foreach (var effect in _activeEffects.Values)
                effect.OnReset();
        }

        public bool IsSynergyActive(TraitTag traitTag)
        {
            return _activeSynergies.Any(s => s.SynergyData.TargetTag == traitTag &&
                                             s.CurrentCount >= s.SynergyData.Thresholds.FirstOrDefault());
        }
    }
}