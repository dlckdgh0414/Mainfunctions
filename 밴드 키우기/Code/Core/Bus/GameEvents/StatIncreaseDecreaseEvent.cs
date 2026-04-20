using UnityEngine;

namespace Code.Core.Bus.GameEvents
{
    public struct StatIncreaseDecreaseEvent : IEvent
    {
        public bool Increase; // true면 증가 false는 감소
        public string Amount;
        public Sprite StatIcon;
        public string StatName;

        public StatIncreaseDecreaseEvent(bool increase, string amount, Sprite statIcon, string statName)
        {
            this.Increase = increase;
            this.Amount = amount;
            this.StatIcon = statIcon;
            this.StatName = statName;
        }
    }

    public struct StopEvent : IEvent
    {
        
    }
}