using UnityEngine;

namespace Code.Core.Bus.GameEvents
{
    public struct TargetSettingEvent : IEvent
    {
        public string Title; //어디에 나가는지 알려주는거 ex : 말딸 뭐 어디 주니어스 나가기
        public Sprite Icon; // 목표에 맞는 아이콘 넣어주시고
        public int Target; // 얼마나 남았는지 알려주는 조건
        public bool IsTargetSet; //목표 설정할건가 안한건가 할거면 true 안할거면 false

        public TargetSettingEvent(string title, Sprite icon, int target, bool isTargetSet)
        {
            this.Title = title;
            this.Icon = icon;
            this.Target = target;
            this.IsTargetSet = isTargetSet;
        }
    }
}