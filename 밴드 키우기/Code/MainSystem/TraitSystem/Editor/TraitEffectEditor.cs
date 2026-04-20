using System;
using System.Collections.Generic;
using System.Linq;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.TraitEffect;
using UnityEditor;

namespace Code.MainSystem.TraitSystem.Editor
{
    [CustomEditor(typeof(TraitDataSO))]
    public class TraitEffectEditor : UnityEditor.Editor
    {
        private string[] _implementationTypeNames;
        private int _selectedIndex = 0;

        private void OnEnable()
        {
            var baseType = typeof(AbstractTraitEffect);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
            List<Type> types = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var foundTypes = assembly.GetTypes()
                    .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);
                types.AddRange(foundTypes);
            }
            
            List<string> typeNames = new List<string> { "None (Default: MultiStatModifierEffect)" };
            typeNames.AddRange(types.Select(t => t.FullName));
            _implementationTypeNames = typeNames.ToArray();
            
            var property = serializedObject.FindProperty("SpecialLogicClassName");
            string currentName = property.stringValue;
            _selectedIndex = Math.Max(0, Array.IndexOf(_implementationTypeNames, currentName));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, "SpecialLogicClassName");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Logic Selection", EditorStyles.boldLabel);
            
            int newIndex = EditorGUILayout.Popup("Special Logic Class", _selectedIndex, _implementationTypeNames);

            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
                var property = serializedObject.FindProperty("SpecialLogicClassName");
                
                property.stringValue = (_selectedIndex == 0) ? "" : _implementationTypeNames[_selectedIndex];
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}