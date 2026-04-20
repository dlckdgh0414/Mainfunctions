using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.EventManager;
using Code.MainSystem.NewMainScreen;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.EventManager.RandomEvent
{
    public class RandomEventManager : MonoBehaviour
    {
        public static RandomEventManager Instance;

        [SerializeField] private List<RandomEventDataSO> allEvents = new();
        [SerializeField] private TextEventUI textEventUI;

        [Header("발동 확률")]
        [SerializeField, Range(0f, 1f)] private float monthlyTriggerChance = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnMonthEnd += HandleMonthEnd;
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnMonthEnd -= HandleMonthEnd;
        }

        private void HandleMonthEnd(int month)
        {
            var picked = (Random.value <= monthlyTriggerChance) ? PickWeightedEvent() : null;

            if (picked != null)
            {
                foreach (var effect in picked.effects)
                    ApplyEffect(effect);

                if (textEventUI != null)
                {
                    textEventUI.ShowEvent(picked.message, picked.effects,
                        () => BandSupplyManager.Instance?.HandleMonthEnd(month));
                }
                else
                {
                    BandSupplyManager.Instance?.HandleMonthEnd(month);
                }
            }
            else
            {
                BandSupplyManager.Instance?.HandleMonthEnd(month);
            }
        }

        public void TriggerRandomEvent()
        {
            var picked = PickWeightedEvent();
            if (picked == null) return;

            foreach (var effect in picked.effects)
                ApplyEffect(effect);

            if (textEventUI != null)
                textEventUI.ShowEvent(picked.message, picked.effects);
        }

        private RandomEventDataSO PickWeightedEvent()
        {
            if (allEvents == null || allEvents.Count == 0) return null;

            float total = 0f;
            foreach (var e in allEvents)
            {
                if (e == null) continue;
                total += Mathf.Max(0f, e.weight);
            }
            if (total <= 0f) return null;

            float pick = Random.value * total;
            float acc = 0f;
            foreach (var e in allEvents)
            {
                if (e == null) continue;
                acc += Mathf.Max(0f, e.weight);
                if (pick <= acc) return e;
            }
            return null;
        }

        private void ApplyEffect(RandomEventEffect effect)
        {
            switch (effect.type)
            {
                case RandomEventEffectType.AddFunds:
                    if (effect.amount >= 0)
                        BandSupplyManager.Instance?.AddBandFunds(effect.amount);
                    else
                        BandSupplyManager.Instance?.SpendBandFunds(-effect.amount);
                    break;

                case RandomEventEffectType.AddFans:
                    if (effect.amount >= 0)
                        BandSupplyManager.Instance?.AddBandFans(effect.amount);
                    else
                        BandSupplyManager.Instance?.RemoveBandFans(-effect.amount);
                    break;

                case RandomEventEffectType.AddExp:
                    if (effect.amount >= 0)
                        BandSupplyManager.Instance?.AddBandExp(effect.amount);
                    else
                        BandSupplyManager.Instance?.SpendBandExp(-effect.amount);
                    break;

                case RandomEventEffectType.AddConditionAll:
                {
                    var cm = MemberConditionManager.Instance;
                    if (cm == null) break;

                    int steps = Mathf.Abs(effect.amount);
                    if (steps == 0) break;

                    var members = new List<MemberType>(cm.RegisteredMembers);
                    foreach (var member in members)
                    {
                        if (effect.amount > 0)
                            cm.UpCondition(member, steps);
                        else
                            cm.DownCondition(member, steps);
                    }
                    break;
                }
            }
        }
    }
}