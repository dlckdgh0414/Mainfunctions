using System;
using System.Collections.Generic;
using Code.Core.Attributes;
using Code.MainSystem.Synergy.Effect;
using Code.MainSystem.Synergy.Runtime;
using Code.MainSystem.TraitSystem.Data;
using UnityEngine;

namespace Code.MainSystem.Synergy.Data
{

    [CreateAssetMenu(fileName = "Synergy data", menuName = "SO/Synergy/Synergy data")]
    public class TraitSynergyDataSO : ScriptableObject
    {
        public TraitTag TargetTag;
        public string SynergyName;
        public Sprite SynergyIcon;
        [TextArea(3, 6)] public string Description;
        
        public List<int> Thresholds; 
        public List<float> EffectValues;

        [SerializeReference, SubclassSelector] 
        public AbstractSynergyEffect EffectTemplate;
        
        /// <summary>
        /// 인스펙터에 설정된 EffectTemplate의 타입을 기반으로 새로운 런타임 인스턴스를 생성합니다.
        /// </summary>
        public AbstractSynergyEffect CreateEffectInstance(ActiveSynergy status)
        {
            if (EffectTemplate == null) 
                return null;
            
            Type type = EffectTemplate.GetType();
            AbstractSynergyEffect instance = Activator.CreateInstance(type) as AbstractSynergyEffect;
            
            instance?.Initialize(status);
            
            return instance;
        }
    }
}