// Assets/Editor/ListFieldManager.cs
// [Serializable] 커스텀 클래스로 이루어진 List<T> SerializeField를
// 한 화면에서 SerializedProperty로 관리하는 에디터 창
// 메뉴: Tools > List Field Manager

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ListFieldManager : EditorWindow
{
    private class ListEntry
    {
        public MonoBehaviour   Component;
        public SerializedObject SerializedObject;
        public SerializedProperty Property; // List 자체의 SerializedProperty
        public string          FieldName;
        public bool            Foldout;
    }

    private List<ListEntry> _entries = new();
    private Vector2         _scroll;

    private bool     _stylesReady;
    private GUIStyle _headerStyle;

    [MenuItem("Tools/List Field Manager")]
    public static void Open()
    {
        var win = GetWindow<ListFieldManager>("List Fields");
        win.minSize = new Vector2(480, 400);
        win.Scan();
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += Scan;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= Scan;
    }

    // 씬 전체에서 커스텀 Serializable 클래스의 List 필드 수집
    private void Scan()
    {
        _entries.Clear();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            foreach (var mono in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue;
                if (mono.GetType().Assembly.FullName.Contains("UnityEditor")) continue;

                CollectFrom(mono);
            }
        }

        Repaint();
    }

    private void CollectFrom(MonoBehaviour mono)
    {
        var type = mono.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            foreach (var fi in type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsSerializableField(fi)) continue;
                if (!IsCustomSerializableList(fi.FieldType)) continue;

                var so   = new SerializedObject(mono);
                var prop = so.FindProperty(fi.Name);
                if (prop == null) continue;

                _entries.Add(new ListEntry
                {
                    Component        = mono,
                    SerializedObject = so,
                    Property         = prop,
                    FieldName        = fi.Name,
                    Foldout          = true,
                });
            }
            type = type.BaseType;
        }
    }

    private void InitStyles()
    {
        if (_stylesReady) return;

        _headerStyle = new GUIStyle(EditorStyles.foldoutHeader)
        {
            fontStyle = FontStyle.Bold,
            normal    = { textColor = new Color(0.75f, 0.9f, 1f) },
        };

        _stylesReady = true;
    }

    private void OnGUI()
    {
        InitStyles();
        DrawToolbar();

        if (_entries.Count == 0)
        {
            EditorGUILayout.Space(30);
            EditorGUILayout.LabelField(
                "커스텀 Serializable List 필드가 없습니다.",
                EditorStyles.centeredGreyMiniLabel);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var entry in _entries)
            DrawEntry(entry);

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"커스텀 List 필드  {_entries.Count}개", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(50)))
            Scan();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntry(ListEntry entry)
    {
        EditorGUILayout.Space(8);

        // SerializedObject 갱신 (씬 변경 반영)
        entry.SerializedObject.Update();

        string goName    = entry.Component.gameObject.name;
        string compName  = entry.Component.GetType().Name;
        string elemType  = GetListElementTypeName(entry.Component, entry.FieldName);
        string header    = $"{goName}  /  {compName}  /  {entry.FieldName}  <{elemType}>";

        // 헤더 + Foldout
        EditorGUILayout.BeginHorizontal();
        entry.Foldout = EditorGUILayout.Foldout(entry.Foldout, header, true, _headerStyle);

        // Select 버튼
        if (GUILayout.Button("Select", GUILayout.Width(52)))
        {
            Selection.activeGameObject = entry.Component.gameObject;
            EditorGUIUtility.PingObject(entry.Component.gameObject);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.DrawRect(
            EditorGUILayout.GetControlRect(false, 1),
            new Color(0.4f, 0.6f, 1f, 0.25f));

        if (!entry.Foldout) return;

        EditorGUI.indentLevel++;

        // SerializedProperty로 리스트 전체를 그림
        // includeChildren = true 로 하면 각 요소의 모든 하위 필드까지 Inspector와 동일하게 표시
        EditorGUILayout.PropertyField(entry.Property, new GUIContent(entry.FieldName), true);

        EditorGUI.indentLevel--;

        // 변경사항 반영
        if (entry.SerializedObject.hasModifiedProperties)
            entry.SerializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(4);
    }

    // public 이거나 [SerializeField]가 붙은 필드인지 확인
    private static bool IsSerializableField(FieldInfo fi)
    {
        return (fi.IsPublic && fi.GetCustomAttribute<NonSerializedAttribute>() == null)
               || fi.GetCustomAttribute<SerializeField>() != null;
    }

    /// <summary>
    /// List<T>이면서 T가 [Serializable] 커스텀 클래스인지 확인한다.
    /// UnityEngine.Object 계열 리스트는 제외 (이미 Inspector에서 쉽게 다룸)
    /// </summary>
    private static bool IsCustomSerializableList(Type ft)
    {
        if (!ft.IsGenericType) return false;
        if (ft.GetGenericTypeDefinition() != typeof(List<>)) return false;

        var elemType = ft.GetGenericArguments()[0];

        // UnityEngine.Object 계열 제외
        if (typeof(UnityEngine.Object).IsAssignableFrom(elemType)) return false;

        // [Serializable] 어트리뷰트가 있는 클래스만 포함
        return elemType.GetCustomAttribute<SerializableAttribute>() != null;
    }

    // 필드 이름으로 List<T>의 T 이름 문자열 반환
    private static string GetListElementTypeName(MonoBehaviour mono, string fieldName)
    {
        var fi = mono.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fi == null) return "?";

        var args = fi.FieldType.GetGenericArguments();
        return args.Length > 0 ? args[0].Name : "?";
    }
}