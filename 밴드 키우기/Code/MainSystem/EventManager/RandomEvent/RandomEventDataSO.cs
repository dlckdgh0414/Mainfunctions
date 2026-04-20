using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.EventManager.RandomEvent
{
    public enum RandomEventType
    {
        Good,
        Bad,
    }

    public enum RandomEventEffectType
    {
        AddFunds,
        AddFans,
        AddExp,
        AddConditionAll,
    }

    [Serializable]
    public class RandomEventEffect
    {
        public RandomEventEffectType type;
        public int amount;
    }
    [CreateAssetMenu(fileName = "RandomEvent", menuName = "SO/Event/RandomEvent", order = 0)]
    public class RandomEventDataSO : ScriptableObject
    {
        public string eventId;
        public RandomEventType type;

        [TextArea(2, 4)]
        public string message;

        public List<RandomEventEffect> effects = new();

        [Range(0f, 5f)]
        public float weight = 1f;
    }
}