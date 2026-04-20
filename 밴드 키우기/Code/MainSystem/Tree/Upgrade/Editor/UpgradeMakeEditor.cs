using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.Tree.Upgrade.Editor
{
    public class UpgradeManager : EditorWindow
    {
        private List<Type> _upgradeTypes = new List<Type>();
        private List<BaseUpgradeSO> _existingAssets = new List<BaseUpgradeSO>();
        
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _searchText = "";
        private const string DefaultPath = "Assets/_Modules/Tree/Upgrades";

        private BaseUpgradeSO _selectedAsset;
        private UnityEditor.Editor _cachedEditor;
        
        private BaseUpgradeSO _tempCreationAsset;
        private string _newAssetName = "NewUpgrade";
        private bool _isCreationMode = false;

        [MenuItem("Tools/Tree System/Upgrade Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<UpgradeManager>("Upgrade Manager");
            window.minSize = new Vector2(700, 400);
        }

        private void OnEnable() => RefreshData();
        private void OnDisable() => CleanUpEditor();

        private void RefreshData()
        {
            _upgradeTypes = Assembly.GetAssembly(typeof(BaseUpgradeSO))
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseUpgradeSO)))
                .OrderBy(t => t.Name).ToList();

            _existingAssets.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BaseUpgradeSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BaseUpgradeSO>(path);
                if (asset != null) _existingAssets.Add(asset);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("↻", EditorStyles.miniButton, GUILayout.Width(25))) RefreshData();
            EditorGUILayout.EndHorizontal();

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, "Box");

            DrawSectionHeader("✚ Create New");
            foreach (var type in _upgradeTypes)
            {
                if (!IsMatchSearch(type.Name)) continue;
                if (GUILayout.Button(type.Name, EditorStyles.miniButtonLeft)) StartCreation(type);
            }

            EditorGUILayout.Space(15);

            DrawSectionHeader($"Existing Assets ({_existingAssets.Count})");
            for (int i = 0; i < _existingAssets.Count; i++)
            {
                var asset = _existingAssets[i];
                if (asset == null) continue;
                if (!IsMatchSearch(asset.name)) continue;

                GUI.backgroundColor = (_selectedAsset == asset && !_isCreationMode) ? Color.cyan : Color.white;
                if (GUILayout.Button($"{asset.name}\n[{asset.GetType().Name}]", GUILayout.Height(35)))
                {
                    SelectAsset(asset);
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical("HelpBox");
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);

            if (_isCreationMode && _tempCreationAsset != null)
            {
                DrawCreationView();
            }
            else if (_selectedAsset != null)
            {
                DrawEditView();
            }
            else
            {
                EditorGUILayout.HelpBox("왼쪽 리스트에서 에셋을 선택하거나 새로 생성하세요.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawCreationView()
        {
            EditorGUILayout.LabelField($"Creating New: {_tempCreationAsset.GetType().Name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            _newAssetName = EditorGUILayout.TextField("Asset Name", _newAssetName);
            EditorGUILayout.Space(5);
            
            DrawEmbeddedEditor(_tempCreationAsset);

            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save to Project", GUILayout.Height(30))) FinalizeCreation();
            if (GUILayout.Button("Cancel", GUILayout.Height(30))) CancelCreation();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEditView()
        {
            // 상단 툴바 레이아웃
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Editing: {_selectedAsset.name}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(50))) 
                EditorGUIUtility.PingObject(_selectedAsset);
            
            GUI.backgroundColor = new Color(1f, 0f, 0f);
            if (GUILayout.Button("Delete Asset", EditorStyles.toolbarButton, GUILayout.Width(100))) 
                DeleteAsset(_selectedAsset);
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            DrawEmbeddedEditor(_selectedAsset);
        }

        private void DrawEmbeddedEditor(BaseUpgradeSO targetSO)
        {
            if (_cachedEditor == null || _cachedEditor.target != targetSO)
            {
                CleanUpEditor();
                _cachedEditor = UnityEditor.Editor.CreateEditor(targetSO);
            }
            _cachedEditor.OnInspectorGUI();
        }

        private void DeleteAsset(BaseUpgradeSO asset)
        {
            if (asset == null) return;

            // 확인 다이얼로그
            if (EditorUtility.DisplayDialog("에셋 삭제", 
                $"정말로 '{asset.name}' 에셋을 프로젝트에서 영구히 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", 
                "삭제", "취소"))
            {
                string path = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _selectedAsset = null;
                CleanUpEditor();
                RefreshData();
            }
        }

        private void StartCreation(Type type)
        {
            _isCreationMode = true;
            _selectedAsset = null;
            _tempCreationAsset = (BaseUpgradeSO)ScriptableObject.CreateInstance(type);
            _newAssetName = $"New_{type.Name}";
            CleanUpEditor();
        }

        private void FinalizeCreation()
        {
            if (!Directory.Exists(DefaultPath)) Directory.CreateDirectory(DefaultPath);
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{DefaultPath}/{_newAssetName}.asset");
    
            // 1. 에셋 생성
            AssetDatabase.CreateAsset(_tempCreationAsset, fullPath);
            AssetDatabase.SaveAssets();

            // 2. 중요: CancelCreation을 호출하지 말고 상태만 초기화합니다.
            BaseUpgradeSO savedAsset = _tempCreationAsset;
    
            _isCreationMode = false;
            _tempCreationAsset = null; // 참조만 끊어줍니다.
            CleanUpEditor();

            RefreshData();
            SelectAsset(savedAsset);
        }

        private void CancelCreation()
        {
            if (_isCreationMode && _tempCreationAsset != null)
            {
                // 에러 메시지의 권장사항대로 true를 추가하여 메모리에서 확실히 제거합니다.
                // (아직 CreateAsset 되기 전이라면 안전하게 제거됩니다)
                DestroyImmediate(_tempCreationAsset, true);
            }
            _isCreationMode = false;
            _tempCreationAsset = null;
            CleanUpEditor();
        }

        private void SelectAsset(BaseUpgradeSO asset)
        {
            _isCreationMode = false;
            _selectedAsset = asset;
            _tempCreationAsset = null;
            CleanUpEditor();
        }

        private void CleanUpEditor()
        {
            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }
        }

        private bool IsMatchSearch(string text) => string.IsNullOrEmpty(_searchText) || text.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.whiteBoldLabel);
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(EditorGUI.indentLevel * 15, GUILayoutUtility.GetLastRect().y + 15), new Vector2(300, GUILayoutUtility.GetLastRect().y + 15));
            EditorGUILayout.Space(5);
        }
    }
}