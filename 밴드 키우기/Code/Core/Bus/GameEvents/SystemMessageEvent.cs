using UnityEngine;

namespace Code.Core.Bus.GameEvents
{
    public struct SystemMessageEvent : IEvent
    {
        public SystemMessageIconType IconType;
        public string Message;

        public SystemMessageEvent(SystemMessageIconType type, string message)
        {
            IconType = type;
            this.Message = message;
        }
    }
}