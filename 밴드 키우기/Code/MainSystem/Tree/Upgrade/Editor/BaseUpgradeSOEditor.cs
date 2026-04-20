using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade.Editor
{
    [CustomEditor(typeof(BaseUpgradeSO), true)] // true를 통해 상속받은 모든 클래스에 적용
    [CanEditMultipleObjects]
    public class BaseUpgradeSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _typeProp;
        private SerializedProperty _descriptionProp;

        private void OnEnable()
        {
            _typeProp = serializedObject.FindProperty("type");
            _descriptionProp = serializedObject.FindProperty("effectDescription");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- 1. 공통 설정 섹션 ---
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_typeProp);
            
            EditorGUILayout.LabelField("Effect Description");
            _descriptionProp.stringValue = EditorGUILayout.TextArea(_descriptionProp.stringValue, GUILayout.MinHeight(50));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // --- 2. 클래스별 개별 필드 섹션 ---
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.LabelField($"{target.GetType().Name} Specific Fields", EditorStyles.boldLabel);
            
            // 공통 필드를 제외한 나머지 필드들을 자동으로 그림
            DrawPropertiesExcluding(serializedObject, "m_Script", "type", "effectDescription");
            EditorGUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties())
            {
                // 변경 사항이 있을 때 필요한 로직이 있다면 여기에 추가
            }
        }
    }
}