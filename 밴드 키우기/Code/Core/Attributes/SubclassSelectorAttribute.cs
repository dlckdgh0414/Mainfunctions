using System;
using UnityEngine;

namespace Code.Core.Attributes
{
    /// <summary>
    /// SerializeReference 필드에서 인터페이스나 추상 클래스를 상속받는 하위 클래스를 선택할 수 있게 해주는 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}