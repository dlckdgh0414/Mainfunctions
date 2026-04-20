using System;
using System.Linq;
using Code.MainSystem.Synergy.Data;
using Code.MainSystem.Synergy.Effect;
using UnityEditor;

namespace Code.MainSystem.Synergy.Editor
{
    [CustomEditor(typeof(TraitSynergyDataSO))]
    public class SynergyDataEditor : UnityEditor.Editor
    {
        private Type[] _types;
        private string[] _typeNames;

        private void OnEnable()
        {
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(AbstractSynergyEffect).IsAssignableFrom(p) && !p.IsAbstract)
                .ToArray();
            _typeNames = _types.Select(t => t.Name).Prepend("None").ToArray();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var data = (TraitSynergyDataSO)target;

            int selectedIndex = data.EffectTemplate == null ? 0 : 
                Array.IndexOf(_types, data.EffectTemplate.GetType()) + 1;

            int newIndex = EditorGUILayout.Popup("Synergy Effect Class", selectedIndex, _typeNames);

            if (newIndex != selectedIndex)
            {
                Undo.RecordObject(data, "Change Synergy Effect");
                data.EffectTemplate = (newIndex == 0) ? null : 
                    (AbstractSynergyEffect)Activator.CreateInstance(_types[newIndex - 1]);
                EditorUtility.SetDirty(data);
            }
        }
    }
}