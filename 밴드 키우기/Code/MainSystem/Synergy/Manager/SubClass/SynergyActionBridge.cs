using Code.MainSystem.Synergy.Effect;
using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.Synergy.Manager.SubClass
{
    public struct ActionContext
    {
        public readonly TraitTag Tag;
        public readonly SynergyTrigger Trigger;
        public float Value;

        public ActionContext(TraitTag tag, SynergyTrigger trigger, float value)
        {
            Tag = tag;
            Trigger = trigger;
            Value = value;
        }
    }

    public class SynergyActionBridge : MonoBehaviour
    {
        private SynergyEffectManager SynergyManager => SynergyEffectManager.Instance;

        public void ModifyValue(ref ActionContext context)
        {
            context.Value = SynergyManager.QueryValue(context.Tag, context.Trigger, context.Value);
        }

        public void Execute(ActionContext context)
        {
            SynergyManager.SendTrigger(context.Tag, context.Trigger, context);
        }
        
        public void Tick()
        {
            SynergyManager.Tick();
        }

        public void ResetAll()
        {
            SynergyManager.ResetAll();
        }
    }
}