using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.Synergy.Effect.SpecialEffect
{
    public class ImmersionEffect : AbstractSynergyEffect
    {
        private bool _isPenaltyActive = false;
        private float _penaltyTimer = 0f;
        private const float PenaltyDuration = 5f;
        
        public override void OnUpdate()
        {
            if (_isPenaltyActive)
            {
                _penaltyTimer -= Time.deltaTime;
                if (_penaltyTimer <= 0)
                    _isPenaltyActive = false;
            }
        }

        public override void OnReset()
        {
            _isPenaltyActive = false;
            _penaltyTimer = 0f;
        }
        
        public override bool IsTargetStat(TraitTag category) 
            => category.HasFlag(TraitTag.Immersion);

        public override void OnTrigger(SynergyTrigger trigger, object context = null)
        {
            // 콤보가 0이 되었을 때(끊겼을 때) 페널티 활성화
            if (trigger != SynergyTrigger.OnComboChanged || context is not int combo || combo != 0)
                return;

            _isPenaltyActive = true;
            _penaltyTimer = PenaltyDuration;
        }

        public override float QueryValue(SynergyTrigger trigger, object context = null)
        {
            if (trigger != SynergyTrigger.OnComboChanged) 
                return 1.0f;
            
            if (_isPenaltyActive)
                return 0.8f;
            
            if (context is >= 50)
            {
                return 1.2f;
            }

            return 1.0f;
        }
    }
}