using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.MainSystem.TraitSystem.TraitEffect;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.TraitSystem.Data
{
    [Serializable]
    public struct StatImpact
    {
        public TraitTarget Target;       // N번째 효과가 적용될 대상
        public CalculationType CalcType; // 계산 방식
        public string RequiredTag;       // 특정 조건
    }

    [Serializable]
    public class MemberTraitComment
    {
        public MemberType MemberType;
        [TextArea(3, 6)] public string Title;
        [TextArea(3, 6)] public string Content;
        [TextArea(3, 6)] public string Thoughts;
    }

    [CreateAssetMenu(fileName = "Trait data", menuName = "SO/Trait/Trait data")]
    public class TraitDataSO : ScriptableObject
    {
        public int IDHash => _idHash == 0 ? _idHash = name.GetHashCode() : _idHash;
        private int _idHash;
        
        public string TraitName;
        public AssetReferenceSprite TraitIcon;

        public int Point;
        public bool IsRemovable = true;
        
        public List<StatImpact> Impacts;
        public List<MemberTraitComment> MemberComments = new List<MemberTraitComment>();
        public TraitTag TraitTag;
        
        public List<float> Effects = new();
        
        [TextArea] public string DescriptionEffect;
        
        [HideInInspector] public string SpecialLogicClassName;
        
        private Dictionary<MemberType, MemberTraitComment> _commentCache;

        public AbstractTraitEffect CreateEffectInstance()
        {
            if (string.IsNullOrEmpty(SpecialLogicClassName))
                return new MultiStatModifierEffect();
            
            Type type = Type.GetType(SpecialLogicClassName);
            
            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(SpecialLogicClassName);
                    if (type != null)
                        break;
                }
            }
            
            if (type != null)
            {
                return (AbstractTraitEffect)Activator.CreateInstance(type);
            }
            
            return new MultiStatModifierEffect();
        }
        
        public MemberTraitComment GetComment(MemberType type)
        {
            _commentCache ??= MemberComments.ToDictionary(m => m.MemberType);

            return _commentCache.GetValueOrDefault(type);
        }
    }
}