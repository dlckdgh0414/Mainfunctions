using Code.MainSystem.Synergy.Runtime;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Synergy.Effect
{
    public enum SynergyTrigger
    {
        None,
        OnBattleStart,      // 공연 시작 시
        OnFeverStart,       // 피버 시작
        OnFeverEnd,         // 피버 끝
        OnMemberSwitch,     // 피버 중 멤버 전환
        OnNoteHit,          // 노트 판정 시
        OnComboChanged,     // 콤보 도달 시
        OnMissAvoided,      // MISS 판정 방어 시
        OnActionExecuted,   // 추가 행동 시
        OnResultCalculate   // 최종 점수 정산 시
    }

    public abstract class AbstractSynergyEffect
    {
        protected ActiveSynergy _status;

        public virtual void Initialize(ActiveSynergy status)
            => _status = status;
        
        protected float GetTieredValue()
        {
            var data = _status.SynergyData;
            if (data.Thresholds == null || data.EffectValues == null) 
                return 0f;

            float result = 0f;
            for (int i = 0; i < data.Thresholds.Count; i++)
            {
                if (_status.CurrentCount < data.Thresholds[i]) 
                    continue;
                
                if (i < data.EffectValues.Count) 
                    result = data.EffectValues[i];
                else 
                    break;
            }
            
            return result;
        }
        public virtual void OnUpdate() { }
        public virtual void OnReset() { }

        public virtual void OnTrigger(SynergyTrigger trigger, object context = null) { }
        public virtual float QueryValue(SynergyTrigger trigger, object context = null) => 0f;
        public abstract bool IsTargetStat(TraitTag category);
    }
}