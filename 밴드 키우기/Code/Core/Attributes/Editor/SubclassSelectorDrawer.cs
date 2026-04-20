using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Code.Core.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        private bool _initialized = false;
        private Type _baseType;
        private List<Type> _inheritedTypes;
        private string[] _typeNames;

        private void Initialize(SerializedProperty property)
        {
            if (_initialized) return;

            // 관리되는 참조의 기본 타입 가져오기
            string typeName = property.managedReferenceFieldTypename;
            int splitIndex = typeName.IndexOf(' ');
            if (splitIndex > 0)
            {
                string assemblyName = typeName.Substring(0, splitIndex);
                string className = typeName.Substring(splitIndex + 1);
                _baseType = Type.GetType(className + ", " + assemblyName);
            }

            if (_baseType != null)
            {
                _inheritedTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => _baseType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                    .ToList();

                _typeNames = new string[_inheritedTypes.Count + 1];
                _typeNames[0] = "None (Null)";
                for (int i = 0; i < _inheritedTypes.Count; i++)
                {
                    _typeNames[i + 1] = _inheritedTypes[i].Name;
                }
            }

            _initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            if (_baseType == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // 현재 할당된 객체의 타입 확인
            string currentTypeName = "None (Null)";
            if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                int splitIndex = property.managedReferenceFullTypename.IndexOf(' ');
                if (splitIndex > 0)
                {
                    currentTypeName = property.managedReferenceFullTypename.Substring(splitIndex + 1);
                    int lastDot = currentTypeName.LastIndexOf('.');
                    if (lastDot >= 0)
                    {
                        currentTypeName = currentTypeName.Substring(lastDot + 1);
                    }
                }
            }

            int currentIndex = Array.IndexOf(_typeNames, currentTypeName);
            if (currentIndex < 0) currentIndex = 0;

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.BeginProperty(position, label, property);
            
            // 드롭다운 그리기
            int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, _typeNames);
            
            // 사용자가 다른 타입을 선택했을 때
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    Type targetType = _inheritedTypes[newIndex - 1];
                    property.managedReferenceValue = Activator.CreateInstance(targetType);
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            // 하위 속성이 있는 경우 그리기
            if (property.managedReferenceValue != null)
            {
                Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty iterator = property.Copy();
                    bool enterChildren = true;
                    
                    float yOffset = EditorGUIUtility.singleLineHeight + 2;
                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        if (SerializedProperty.EqualContents(iterator, property.GetEndProperty()))
                            break;

                        float height = EditorGUI.GetPropertyHeight(iterator, true);
                        Rect propRect = new Rect(position.x, position.y + yOffset, position.width, height);
                        EditorGUI.PropertyField(propRect, iterator, true);
                        yOffset += height + 2;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            
            float height = EditorGUIUtility.singleLineHeight;
            
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                SerializedProperty iterator = property.Copy();
                bool enterChildren = true;
                
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (SerializedProperty.EqualContents(iterator, property.GetEndProperty()))
                        break;
                        
                    height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                }
            }
            
            return height;
        }
    }
}