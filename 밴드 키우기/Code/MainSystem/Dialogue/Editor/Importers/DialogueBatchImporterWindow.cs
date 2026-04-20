using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.MainSystem.Dialogue.Editor;
using Code.MainSystem.Dialogue.Parser.Editor;
using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Editor.Importers
{
    /// <summary>
    /// 다이얼로그 CSV를 SO에 일괄 임포트하는 에디터 창
    /// </summary>
    public class DialogueBatchImporterWindow : EditorWindow
    {
        private enum EntryStatus
        {
            Ready,
            MissingSO
        }

        private class ImportEntry
        {
            public string baseName;
            public string csvAbsolutePath;
            public string soAssetPath;
            public EntryStatus status;
        }

        private string _csvFolderPath = "Assets/_Modules/Dialogue/Remaster/Csv";
        private string _soFolderPath = "Assets/_Modules/Dialogue/Remaster/SO";
        private bool _persistAutoCorrectionsToCsv;
        private Vector2 _scrollPosition;
        private List<ImportEntry> _entries = new List<ImportEntry>();

        [MenuItem("Tools/Dialogue Batch Importer")]
        public static void OpenWindow()
        {
            DialogueBatchImporterWindow window = GetWindow<DialogueBatchImporterWindow>();
            window.titleContent = new GUIContent("Dialogue Batch Importer");
            window.minSize = new Vector2(840f, 460f);
        }

        private void OnGUI()
        {
            DrawFolderSelectors();
            EditorGUILayout.Space(6f);
            DrawActions();
            EditorGUILayout.Space(8f);
            DrawSummary();
            EditorGUILayout.Space(4f);
            DrawEntries();
        }

        private void DrawFolderSelectors()
        {
            EditorGUILayout.LabelField("Folder Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _csvFolderPath = EditorGUILayout.TextField("CSV Folder", _csvFolderPath);
            if (GUILayout.Button("Select", GUILayout.Width(80f)))
            {
                string selected = EditorUtility.OpenFolderPanel("CSV Folder", Application.dataPath, string.Empty);
                string assetPath = DialogueEditorAssetPathUtility.ConvertAbsoluteToAssetPath(selected);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    _csvFolderPath = assetPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _soFolderPath = EditorGUILayout.TextField("SO Folder", _soFolderPath);
            if (GUILayout.Button("Select", GUILayout.Width(80f)))
            {
                string selected = EditorUtility.OpenFolderPanel("SO Folder", Application.dataPath, string.Empty);
                string assetPath = DialogueEditorAssetPathUtility.ConvertAbsoluteToAssetPath(selected);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    _soFolderPath = assetPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            _persistAutoCorrectionsToCsv = EditorGUILayout.ToggleLeft(
                "Persist Auto-Corrections To CSV",
                _persistAutoCorrectionsToCsv);
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Scan", GUILayout.Height(28f)))
            {
                ScanEntries();
            }

            GUI.enabled = _entries.Count > 0;
            if (GUILayout.Button("Create Missing SOs", GUILayout.Height(28f)))
            {
                CreateMissingSOs();
            }

            if (GUILayout.Button("Import All Ready", GUILayout.Height(28f)))
            {
                ImportAllReady();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            int readyCount = _entries.Count(entry => entry.status == EntryStatus.Ready);
            int missingCount = _entries.Count(entry => entry.status == EntryStatus.MissingSO);

            EditorGUILayout.HelpBox(
                $"Total: {_entries.Count} | Ready: {readyCount} | Missing SO: {missingCount}",
                MessageType.Info);
        }

        private void DrawEntries()
        {
            EditorGUILayout.LabelField("Mapped Files", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (ImportEntry entry in _entries)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(entry.baseName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                DrawStatusBadge(entry.status);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("CSV", entry.csvAbsolutePath);
                EditorGUILayout.LabelField("SO", string.IsNullOrEmpty(entry.soAssetPath) ? "(missing)" : entry.soAssetPath);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (entry.status == EntryStatus.Ready)
                {
                    if (GUILayout.Button("Import", GUILayout.Width(100f)))
                    {
                        ImportSingle(entry);
                    }
                }
                else
                {
                    if (GUILayout.Button("Create SO", GUILayout.Width(100f)))
                    {
                        CreateSingleSO(entry);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusBadge(EntryStatus status)
        {
            Color previousColor = GUI.color;
            string text;

            switch (status)
            {
                case EntryStatus.Ready:
                    GUI.color = new Color(0.45f, 0.9f, 0.45f);
                    text = "READY";
                    break;
                default:
                    GUI.color = new Color(0.98f, 0.84f, 0.38f);
                    text = "MISSING SO";
                    break;
            }

            GUILayout.Label(text, EditorStyles.miniBoldLabel, GUILayout.Width(78f));
            GUI.color = previousColor;
        }

        private void ScanEntries()
        {
            _entries.Clear();

            if (!AssetDatabase.IsValidFolder(_csvFolderPath))
            {
                EditorUtility.DisplayDialog("Scan Failed", $"CSV folder is invalid: {_csvFolderPath}", "OK");
                return;
            }

            string csvFolderAbsolutePath = DialogueEditorAssetPathUtility.ConvertAssetToAbsolutePath(_csvFolderPath);
            if (string.IsNullOrEmpty(csvFolderAbsolutePath) || !Directory.Exists(csvFolderAbsolutePath))
            {
                EditorUtility.DisplayDialog("Scan Failed", $"Cannot resolve CSV folder: {_csvFolderPath}", "OK");
                return;
            }

            string[] csvFiles = System.IO.Directory.GetFiles(csvFolderAbsolutePath, "*.csv", System.IO.SearchOption.AllDirectories);
            string[] soGuids = AssetDatabase.IsValidFolder(_soFolderPath)
                ? AssetDatabase.FindAssets("t:DialogueInformationSO", new[] { _soFolderPath })
                : Array.Empty<string>();

            Dictionary<string, string> soMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in soGuids)
            {
                string soAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                string soName = System.IO.Path.GetFileNameWithoutExtension(soAssetPath);
                if (!soMap.ContainsKey(soName))
                {
                    soMap[soName] = soAssetPath;
                }
            }

            foreach (string csvPath in csvFiles)
            {
                string baseName = System.IO.Path.GetFileNameWithoutExtension(csvPath);
                bool hasSo = soMap.TryGetValue(baseName, out string soAssetPath);

                _entries.Add(new ImportEntry
                {
                    baseName = baseName,
                    csvAbsolutePath = csvPath,
                    soAssetPath = hasSo ? soAssetPath : string.Empty,
                    status = hasSo ? EntryStatus.Ready : EntryStatus.MissingSO
                });
            }

            _entries = _entries.OrderBy(entry => entry.baseName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void CreateMissingSOs()
        {
            int createdCount = 0;
            foreach (ImportEntry entry in _entries)
            {
                if (entry.status != EntryStatus.MissingSO)
                {
                    continue;
                }

                if (CreateSingleSO(entry))
                {
                    createdCount++;
                }
            }

            if (createdCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Create Missing SOs", $"Created {createdCount} DialogueInformationSO assets.", "OK");
        }

        private bool CreateSingleSO(ImportEntry entry)
        {
            if (entry == null || entry.status != EntryStatus.MissingSO)
            {
                return false;
            }

            if (!DialogueEditorAssetPathUtility.EnsureAssetFolderExists(_soFolderPath))
            {
                EditorUtility.DisplayDialog("SO 생성 실패", $"SO 폴더를 만들 수 없습니다: {_soFolderPath}", "확인");
                return false;
            }

            DialogueInformationSO newAsset = ScriptableObject.CreateInstance<DialogueInformationSO>();
            string targetPath = AssetDatabase.GenerateUniqueAssetPath($"{_soFolderPath}/{entry.baseName}.asset");
            AssetDatabase.CreateAsset(newAsset, targetPath);

            entry.soAssetPath = targetPath;
            entry.status = EntryStatus.Ready;
            EditorUtility.SetDirty(newAsset);
            return true;
        }

        private void ImportAllReady()
        {
            int successCount = 0;
            int failCount = 0;

            foreach (ImportEntry entry in _entries)
            {
                if (entry.status != EntryStatus.Ready)
                {
                    continue;
                }

                bool success = ImportSingle(entry, false);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Batch Import Complete", $"Success: {successCount}\nFailed: {failCount}", "OK");
        }

        private bool ImportSingle(ImportEntry entry, bool showPopup = true)
        {
            DialogueInformationSO so = AssetDatabase.LoadAssetAtPath<DialogueInformationSO>(entry.soAssetPath);
            if (so == null)
            {
                Debug.LogError($"[DialogueBatchImporterWindow] Failed to load SO: {entry.soAssetPath}");
                if (showPopup)
                {
                    EditorUtility.DisplayDialog("임포트 실패", $"SO 로드 실패: {entry.soAssetPath}", "확인");
                }
                return false;
            }

            return DialogueCSVImporter.ImportCSVFromPath(
                so,
                entry.csvAbsolutePath,
                showPopup,
                false,
                _persistAutoCorrectionsToCsv);
        }

    }
}
