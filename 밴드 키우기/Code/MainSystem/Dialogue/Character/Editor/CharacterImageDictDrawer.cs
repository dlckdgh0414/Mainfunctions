#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Member.LS.Code.Dialogue.Character.Editor
{
    [CustomPropertyDrawer(typeof(Member.LS.Code.Dialogue.Character.CharacterImageDict))]
    public sealed class CharacterImageDictDrawer : PropertyDrawer
    {
        private const Single ROW_PAD = 2.0f;

        public override Single GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty keys = property.FindPropertyRelative("m_Keys");
            Int32 size = keys != null ? keys.arraySize : 0;
            // Title + Header + Rows + Add + (Optional) HelpBox 2줄
            Int32 rows = 1 + 1 + size + 1;
            // HelpBox 두 줄 정도 추가 공간 여유
            return rows * (EditorGUIUtility.singleLineHeight + ROW_PAD) + 6.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keys = property.FindPropertyRelative("m_Keys");
            SerializedProperty values = property.FindPropertyRelative("m_Values");

            if (keys == null || values == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("SerializedDictionary 내부 필드를 찾을 수 없습니다."));
                return;
            }

            // 길이 동기화
            if (keys.arraySize != values.arraySize)
            {
                Int32 max = Math.Max(keys.arraySize, values.arraySize);
                keys.arraySize = max;
                values.arraySize = max;
            }

            position = EditorGUI.IndentedRect(position);
            Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(line, label);
            line.y += EditorGUIUtility.singleLineHeight + ROW_PAD;

            // 헤더
            Single half = (position.width - 24.0f) * 0.5f;
            Rect hKey = new Rect(line.x, line.y, half, EditorGUIUtility.singleLineHeight);
            Rect hVal = new Rect(line.x + half + 4.0f, line.y, half, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(hKey, "Emotion (Key)");
            EditorGUI.LabelField(hVal, "Sprite (Value)");
            line.y += EditorGUIUtility.singleLineHeight + ROW_PAD;

            // 현재 사용 키 통계
            HashSet<Int32> used = new HashSet<Int32>();
            Dictionary<Int32, Int32> countPerKey = new Dictionary<Int32, Int32>();

            Int32 size = keys.arraySize;
            for (Int32 i = 0; i < size; i++)
            {
                SerializedProperty kp = keys.GetArrayElementAtIndex(i);
                if (kp != null && kp.propertyType == SerializedPropertyType.Enum)
                {
                    Int32 idx = kp.enumValueIndex;
                    used.Add(idx);
                    if (!countPerKey.ContainsKey(idx)) countPerKey[idx] = 0;
                    countPerKey[idx]++;
                }
            }

            // 행 렌더링
            for (Int32 i = 0; i < size; i++)
            {
                SerializedProperty keyProp = keys.GetArrayElementAtIndex(i);
                SerializedProperty valProp = values.GetArrayElementAtIndex(i);

                Rect rKey = new Rect(line.x, line.y, half, EditorGUIUtility.singleLineHeight);
                Rect rVal = new Rect(line.x + half + 4.0f, line.y, half - 20.0f, EditorGUIUtility.singleLineHeight);
                Rect rDel = new Rect(line.x + position.width - 18.0f, line.y, 18.0f, EditorGUIUtility.singleLineHeight);

                Boolean isDup = false;
                if (keyProp != null && keyProp.propertyType == SerializedPropertyType.Enum)
                {
                    Int32 idx = keyProp.enumValueIndex;
                    isDup = countPerKey.ContainsKey(idx) && countPerKey[idx] > 1;
                }

                Color prev = GUI.color;
                if (isDup) GUI.color = new Color(1.0f, 0.6f, 0.6f, 1.0f);
                EditorGUI.PropertyField(rKey, keyProp, GUIContent.none, true);
                GUI.color = prev;

                EditorGUI.PropertyField(rVal, valProp, GUIContent.none, true);

                if (GUI.Button(rDel, "×"))
                {
                    keys.DeleteArrayElementAtIndex(i);
                    values.DeleteArrayElementAtIndex(i);
                    break;
                }

                line.y += EditorGUIUtility.singleLineHeight + ROW_PAD;
            }

            // enum 총 개수
            Int32 enumCount = 0;
            if (size > 0)
            {
                SerializedProperty anyKey = keys.GetArrayElementAtIndex(0);
                if (anyKey != null && anyKey.propertyType == SerializedPropertyType.Enum)
                {
                    enumCount = anyKey.enumDisplayNames != null ? anyKey.enumDisplayNames.Length : 0;
                }
            }
            // 비어 있을 때도 집계
            if (size == 0)
            {
                SerializedProperty tmp = property.FindPropertyRelative("m_Keys");
                // enumCount는 0일 수 있음. 그런 경우엔 Add를 허용하고 첫 항목에서 enum 정보를 얻게 됩니다.
            }

            Boolean allUsed = enumCount > 0 && used.Count >= enumCount;

            // Add 버튼
            Rect addRect = new Rect(line.x, line.y, position.width, EditorGUIUtility.singleLineHeight);
            using (new EditorGUI.DisabledScope(allUsed))
            {
                if (GUI.Button(addRect, "Add Emotion Sprite"))
                {
                    if (allUsed)
                    {
                        // 안전장치: 혹시 비활성화를 무시하는 경우에도 추가하지 않음
                        Debug.LogWarning("모든 감정 키를 이미 사용했습니다. 더 이상 항목을 추가할 수 없습니다.");
                    }
                    else
                    {
                        keys.arraySize++;
                        values.arraySize++;
                        Int32 newIndex = keys.arraySize - 1;

                        SerializedProperty keyNew = keys.GetArrayElementAtIndex(newIndex);
                        SerializedProperty valNew = values.GetArrayElementAtIndex(newIndex);

                        if (valNew != null && valNew.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            valNew.objectReferenceValue = null;
                        }

                        if (keyNew != null && keyNew.propertyType == SerializedPropertyType.Enum)
                        {
                            String[] names = keyNew.enumDisplayNames;
                            Int32 max = names != null ? names.Length : 0;

                            // 미사용 enum 인덱스 찾기
                            Int32 candidate = 0;
                            while (candidate < max && used.Contains(candidate))
                            {
                                candidate++;
                            }
                            // 여기서는 allUsed가 false이므로 candidate < max가 보장됨
                            keyNew.enumValueIndex = candidate;
                        }
                    }
                }
            }

            // 경고/도움말
            if (allUsed)
            {
                Rect warn = new Rect(addRect.x, addRect.y + EditorGUIUtility.singleLineHeight + ROW_PAD,
                    position.width, EditorGUIUtility.singleLineHeight * 2.0f);
                EditorGUI.HelpBox(warn,
                    "모든 감정(enum) 값을 이미 사용했습니다. 새 항목을 추가하려면 기존 항목에서 키를 삭제하거나 다른 enum 값을 추가하세요.",
                    MessageType.Info);
            }
            else if (HasDuplicate(countPerKey))
            {
                Rect warn = new Rect(addRect.x, addRect.y + EditorGUIUtility.singleLineHeight + ROW_PAD,
                    position.width, EditorGUIUtility.singleLineHeight * 2.0f);
                EditorGUI.HelpBox(warn,
                    "중복된 감정 키가 있습니다. SerializedDictionary는 중복 키를 허용하지 않습니다.",
                    MessageType.Warning);
            }
        }

        private static Boolean HasDuplicate(Dictionary<Int32, Int32> countPerKey)
        {
            foreach (KeyValuePair<Int32, Int32> kv in countPerKey)
            {
                if (kv.Value > 1) return true;
            }
            return false;
        }
    }
}
#endif
