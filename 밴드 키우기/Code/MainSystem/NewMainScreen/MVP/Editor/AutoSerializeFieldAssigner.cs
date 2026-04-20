using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoSerializeFieldAssigner
{
    static AutoSerializeFieldAssigner()
    {
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    // 스크립트 붙이는 순간 자동 실행
    private static void OnComponentAdded(Component component)
    {
        if (component is not MonoBehaviour mono) return;
        if (mono.GetType().Assembly.FullName.Contains("UnityEditor")) return;

        int filled = TryAutoAssign(mono);
        if (filled > 0)
        {
            EditorUtility.SetDirty(mono);
            Debug.Log($"[AutoAssign] {mono.GetType().Name} : {filled}개 필드 자동 할당 완료");
        }
    }

    // Tools 메뉴에서 씬 전체에 수동으로 한 번 더 실행
    [MenuItem("Tools/자동넣기 실행")]
    private static void RunAll()
    {
        int totalFilled = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            foreach (var mono in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue;
                if (mono.GetType().Assembly.FullName.Contains("UnityEditor")) continue;

                int filled = TryAutoAssign(mono);
                if (filled > 0)
                {
                    EditorUtility.SetDirty(mono);
                    totalFilled += filled;
                }
            }
        }

        Debug.Log($"[AutoAssign] 전체 완료 : 총 {totalFilled}개 필드 할당됨");
        EditorUtility.DisplayDialog("자동넣기 완료", $"총 {totalFilled}개 필드를 할당했습니다.", "확인");
    }

    /// <summary>
    /// mono의 null SerializeField를 필드 이름 기준으로 찾아 채운다. 채워진 필드 수를 반환한다
    /// </summary>
    private static int TryAutoAssign(MonoBehaviour mono)
    {
        int count = 0;
        var type  = mono.GetType();

        while (type != null && type != typeof(MonoBehaviour))
        {
            foreach (var fi in type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsSerializableField(fi)) continue;

                var current = fi.GetValue(mono) as Object;
                if (current != null) continue;

                var found = FindObject(fi.Name, fi.FieldType, mono);
                if (found == null) continue;

                Undo.RecordObject(mono, $"AutoAssign {fi.Name}");
                fi.SetValue(mono, found);
                count++;

                Debug.Log($"[AutoAssign]  └ {fi.Name} ({fi.FieldType.Name}) <- {found.name}");
            }

            type = type.BaseType;
        }

        return count;
    }

    /// <summary>
    /// 필드 이름과 일치하는 GameObject를 씬에서 찾고 해당 타입의 컴포넌트를 반환한다.
    /// ScriptableObject는 에셋에서, 프리팹 타입은 에셋 프리팹에서 이름으로 탐색한다.
    /// 이름이 일치하는 오브젝트가 없으면 null을 반환한다
    /// </summary>
    private static Object FindObject(string fieldName, System.Type fieldType, MonoBehaviour requester)
    {
        // ScriptableObject : 1개뿐이면 바로 할당, 여러 개면 이름 매칭
        if (typeof(ScriptableObject).IsAssignableFrom(fieldType))
        {
            var guids = AssetDatabase.FindAssets($"t:{fieldType.Name}");
            if (guids.Length == 0) return null;
            if (guids.Length == 1)
                return AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));

            // 여러 개면 이름 매칭
            foreach (var guid in guids)
            {
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null && NamesMatch(asset.name, fieldName))
                    return asset;
            }
            return null;
        }

        // GameObject : 씬에서 이름 매칭 먼저, 없으면 프리팹 에셋 탐색
        if (fieldType == typeof(GameObject))
        {
            var sceneGo = FindGameObjectByName(fieldName);
            if (sceneGo != null) return sceneGo;

            var guids = AssetDatabase.FindAssets($"t:Prefab {fieldName}");
            foreach (var guid in guids)
            {
                var path   = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && NamesMatch(prefab.name, fieldName))
                    return prefab;
            }
            return null;
        }

        // Component : 이름 매칭 먼저, 실패하면 씬에 1개뿐일 때 바로 할당
        if (typeof(Component).IsAssignableFrom(fieldType))
        {
            var go = FindGameObjectByName(fieldName);
            if (go != null)
            {
                var comp = go.GetComponent(fieldType);
                if (comp != null) return comp;
            }

            // 이름 매칭 실패 시 씬 전체에서 해당 타입이 1개뿐이면 바로 할당
            var all = Object.FindObjectsByType(fieldType, FindObjectsSortMode.None);
            if (all.Length == 1) return all[0];
        }

        return null;
    }

    /// <summary>
    /// 씬 전체에서 이름이 매칭되는 GameObject를 찾는다
    /// </summary>
    private static GameObject FindGameObjectByName(string fieldName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindInChildren(root.transform, fieldName);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static GameObject FindInChildren(Transform parent, string fieldName)
    {
        if (NamesMatch(parent.name, fieldName)) return parent.gameObject;
        foreach (Transform child in parent)
        {
            var found = FindInChildren(child, fieldName);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// 대소문자 무시, 언더스코어/공백 무시로 이름을 비교한다.
    /// concertBtn == ConcertBtn == concert_btn 모두 같은 것으로 본다
    /// </summary>
    private static bool NamesMatch(string goName, string fieldName)
    {
        static string Normalize(string s) =>
            s.Replace("_", "").Replace(" ", "").ToLowerInvariant();

        return Normalize(goName) == Normalize(fieldName);
    }

    // public 이거나 [SerializeField]가 붙은 필드인지 확인
    private static bool IsSerializableField(FieldInfo fi)
    {
        return (fi.IsPublic && fi.GetCustomAttribute<System.NonSerializedAttribute>() == null)
               || fi.GetCustomAttribute<SerializeField>() != null;
    }
}