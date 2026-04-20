using UnityEngine;
using System.Linq;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.Manager.SubClass
{
    public struct ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }
    
        public static ValidationResult Success() => new(true, "");
        public static ValidationResult Fail(string msg) => new(false, msg);
    
        private ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
    
    public class TraitValidator : MonoBehaviour, ITraitValidator
    {
        public ValidationResult CanAdd(ITraitHolder holder, TraitDataSO trait)
        {
            if (holder.ActiveTraits.Any(t => t.Data.IDHash == trait.IDHash))
                return ValidationResult.Fail("이미 보유 중인 특성입니다.");

            // 최대 개수 체크 (99개)
            return holder.ActiveTraits.Count >= 99 ? 
                ValidationResult.Fail("최대 보유 개수 초과") : ValidationResult.Success();
        }
    
        public ValidationResult CanRemove(ITraitHolder holder, ActiveTrait trait)
        {
            if (!trait.Data.IsRemovable && !holder.IsAdjusting)
                return ValidationResult.Fail("제거 불가능한 특성");
            
            return ValidationResult.Success();
        }
    }
}