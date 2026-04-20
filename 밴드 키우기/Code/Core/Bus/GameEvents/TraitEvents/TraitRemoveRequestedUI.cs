using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.Core.Bus.GameEvents.TraitEvents
{
    /// <summary>
    /// 특성 제거 확인 UI 표시 요청 이벤트
    /// </summary>
    public struct TraitRemoveRequestedUI : IEvent
    {
        public ActiveTrait Trait { get; }
        public ITraitHolder Holder { get; }

        public TraitRemoveRequestedUI(ActiveTrait trait, ITraitHolder holder)
        {
            Trait = trait;
            Holder = holder;
        }
    }

}