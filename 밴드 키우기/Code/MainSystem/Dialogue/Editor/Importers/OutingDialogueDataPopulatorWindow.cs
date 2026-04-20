using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Code.Core;
using Code.MainSystem.Dialogue.Editor;
using Code.MainSystem.NewMainScreen.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.Dialogue.Editor.Importers
{
    /// <summary>
    /// 외출 다이얼로그 데이터를 일괄 구성하는 에디터 창
    /// </summary>
    public class OutingDialogueDataPopulatorWindow : EditorWindow
    {
        private sealed class PreviewRow
        {
            public MemberType memberType;
            public LocationType locationType;
            public int dialogueCount;
            public string sampleNames;
        }

        private const string DEFAULT_SOURCE_FOLDER = "Assets/_Modules/Dialogue/Remaster/SO";
        private const string DEFAULT_TARGET_ASSET_PATH = "Assets/_Modules/Dialogue/Remaster/SO/OutingDialogueData.asset";

        private OutingDialogueDataSO _target;
        private string _sourceFolder = DEFAULT_SOURCE_FOLDER;
        private readonly List<PreviewRow> PREVIEW_ROWS = new List<PreviewRow>();
        private Vector2 _previewScroll;
        private string _lastResolvedSourceFolder = string.Empty;
        private int _lastPreviewSkippedCount;
        private int _lastPreviewDialogueCount;

        [MenuItem("Tools/Dialogue/Outing Dialogue Populator")]
        public static void OpenWindow()
        {
            OutingDialogueDataPopulatorWindow window = GetWindow<OutingDialogueDataPopulatorWindow>();
            window.titleContent = new GUIContent("Outing Populator");
            window.minSize = new Vector2(760f, 210f);
            window.TryAutoAssignDefaultTarget();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Outing Dialogue Data Populator", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);

            _target = (OutingDialogueDataSO)EditorGUILayout.ObjectField(
                "Target OutingDialogueData",
                _target,
                typeof(OutingDialogueDataSO),
                false);

            EditorGUILayout.BeginHorizontal();
            _sourceFolder = EditorGUILayout.TextField("Source Folder", _sourceFolder);
            if (GUILayout.Button("Select", GUILayout.Width(80f)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Source Folder", Application.dataPath, string.Empty);
                string assetPath = DialogueEditorAssetPathUtility.ConvertAbsoluteToAssetPath(selected);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    _sourceFolder = assetPath;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Supports SO root or subfolders (e.g. Assets/.../SO or .../SO/Guitar).\n" +
                "If a Csv folder is provided, it automatically resolves to the matching SO folder.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Dry Run Preview", GUILayout.Height(28f)))
            {
                RunDryPreview();
            }

            GUI.enabled = _target != null;
            if (GUILayout.Button("Populate OutingDialogueData", GUILayout.Height(28f)))
            {
                Populate();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            DrawPreview();
        }

        private void Populate()
        {
            if (_target == null)
            {
                EditorUtility.DisplayDialog("Populate Failed", "Target OutingDialogueData is null.", "OK");
                return;
            }

            string resolvedSourceFolder = ResolveSourceFolder(_sourceFolder);
            if (string.IsNullOrEmpty(resolvedSourceFolder) || !AssetDatabase.IsValidFolder(resolvedSourceFolder))
            {
                EditorUtility.DisplayDialog(
                    "Populate Failed",
                    $"Source folder is invalid.\nInput: {_sourceFolder}\nResolved: {resolvedSourceFolder}",
                    "OK");
                return;
            }

            if (!TryBuildEntries(resolvedSourceFolder, out List<OutingDialogueEntry> entries, out int skippedCount, out int dialogueCount, out _))
            {
                EditorUtility.DisplayDialog("Populate Failed", $"No DialogueInformationSO found under {resolvedSourceFolder}", "OK");
                return;
            }

            Undo.RecordObject(_target, "Populate OutingDialogueData");

            FieldInfo entriesField = typeof(OutingDialogueDataSO).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);
            if (entriesField == null)
            {
                EditorUtility.DisplayDialog("Populate Failed", "Cannot find private field 'entries' in OutingDialogueDataSO.", "OK");
                return;
            }

            entriesField.SetValue(_target, entries);
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Populate Complete",
                $"Source: {resolvedSourceFolder}\nEntries: {entries.Count}\nDialogues: {dialogueCount}\nSkipped: {skippedCount}",
                "OK");

            Debug.Log($"[OutingDialogueDataPopulator] Completed. entries={entries.Count}, dialogues={dialogueCount}, skipped={skippedCount}");
        }

        private void RunDryPreview()
        {
            string resolvedSourceFolder = ResolveSourceFolder(_sourceFolder);
            if (string.IsNullOrEmpty(resolvedSourceFolder) || !AssetDatabase.IsValidFolder(resolvedSourceFolder))
            {
                PREVIEW_ROWS.Clear();
                _lastResolvedSourceFolder = string.Empty;
                _lastPreviewSkippedCount = 0;
                _lastPreviewDialogueCount = 0;
                EditorUtility.DisplayDialog(
                    "Dry Run Failed",
                    $"Source folder is invalid.\nInput: {_sourceFolder}\nResolved: {resolvedSourceFolder}",
                    "OK");
                return;
            }

            if (!TryBuildEntries(resolvedSourceFolder, out _, out int skippedCount, out int dialogueCount, out List<PreviewRow> previewRows))
            {
                PREVIEW_ROWS.Clear();
                _lastResolvedSourceFolder = resolvedSourceFolder;
                _lastPreviewSkippedCount = 0;
                _lastPreviewDialogueCount = 0;
                EditorUtility.DisplayDialog("Dry Run", $"No DialogueInformationSO found under {resolvedSourceFolder}", "OK");
                return;
            }

            PREVIEW_ROWS.Clear();
            PREVIEW_ROWS.AddRange(previewRows);
            _lastResolvedSourceFolder = resolvedSourceFolder;
            _lastPreviewSkippedCount = skippedCount;
            _lastPreviewDialogueCount = dialogueCount;
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Dry Run Result", EditorStyles.boldLabel);

            if (PREVIEW_ROWS.Count == 0)
            {
                EditorGUILayout.HelpBox("Run 'Dry Run Preview' to inspect inferred member/location groups before apply.", MessageType.None);
                return;
            }

            EditorGUILayout.HelpBox(
                $"Source: {_lastResolvedSourceFolder}\nEntries: {PREVIEW_ROWS.Count} | Dialogues: {_lastPreviewDialogueCount} | Skipped: {_lastPreviewSkippedCount}",
                MessageType.Info);

            _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.MinHeight(180f));
            foreach (PreviewRow row in PREVIEW_ROWS)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{row.memberType} / {row.locationType}  ({row.dialogueCount})", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Sample", row.sampleNames);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private static bool TryBuildEntries(
            string resolvedSourceFolder,
            out List<OutingDialogueEntry> entries,
            out int skippedCount,
            out int dialogueCount,
            out List<PreviewRow> previewRows)
        {
            entries = new List<OutingDialogueEntry>();
            previewRows = new List<PreviewRow>();
            skippedCount = 0;
            dialogueCount = 0;

            string[] dialogueGuids = AssetDatabase.FindAssets("t:DialogueInformationSO", new[] { resolvedSourceFolder });
            if (dialogueGuids == null || dialogueGuids.Length == 0)
            {
                return false;
            }

            Dictionary<(MemberType member, LocationType location), List<(string guid, string path)>> grouped =
                new Dictionary<(MemberType, LocationType), List<(string, string)>>();

            foreach (string guid in dialogueGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!TryExtractMemberAndLocation(path, out MemberType memberType, out LocationType locationType))
                {
                    skippedCount++;
                    Debug.LogWarning($"[OutingDialogueDataPopulator] Skipped (cannot map member/location): {path}");
                    continue;
                }

                (MemberType, LocationType) key = (memberType, locationType);
                if (!grouped.TryGetValue(key, out List<(string guid, string path)> list))
                {
                    list = new List<(string guid, string path)>();
                    grouped[key] = list;
                }

                if (list.All(item => !string.Equals(item.guid, guid, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add((guid, path));
                }
            }

            foreach (KeyValuePair<(MemberType member, LocationType location), List<(string guid, string path)>> kv in grouped)
            {
                List<(string guid, string path)> sortedItems = kv.Value
                    .OrderBy(item => item.path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                OutingDialogueEntry entry = new OutingDialogueEntry
                {
                    memberType = kv.Key.member,
                    locationType = kv.Key.location,
                    dialogues = sortedItems.Select(item => new AssetReference(item.guid)).ToList()
                };

                entries.Add(entry);
                dialogueCount += entry.dialogues?.Count ?? 0;

                string sampleNames = string.Join(", ",
                    sortedItems.Select(item => System.IO.Path.GetFileNameWithoutExtension(item.path)).Take(3));

                previewRows.Add(new PreviewRow
                {
                    memberType = entry.memberType,
                    locationType = entry.locationType,
                    dialogueCount = entry.dialogues?.Count ?? 0,
                    sampleNames = sampleNames
                });
            }

            entries = entries
                .OrderBy(item => item.memberType)
                .ThenBy(item => item.locationType)
                .ToList();

            previewRows = previewRows
                .OrderBy(item => item.memberType)
                .ThenBy(item => item.locationType)
                .ToList();

            return true;
        }

        private static bool TryExtractMemberAndLocation(string assetPath, out MemberType memberType, out LocationType locationType)
        {
            memberType = default;
            locationType = default;

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            string directory = normalizedPath.Contains('/')
                ? normalizedPath.Substring(0, normalizedPath.LastIndexOf('/'))
                : normalizedPath;

            string[] segments = directory.Split('/');

            bool hasMember = false;
            bool hasLocation = false;
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];

                if (!hasMember && TryMapMember(segment, out MemberType parsedMember))
                {
                    memberType = parsedMember;
                    hasMember = true;
                }

                if (!hasLocation && TryMapLocation(segment, out LocationType parsedLocation))
                {
                    locationType = parsedLocation;
                    hasLocation = true;
                }
            }

            return hasMember && hasLocation;
        }

        private static bool TryMapMember(string token, out MemberType memberType)
        {
            memberType = default;
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            switch (token.Trim().ToLowerInvariant())
            {
                case "guitar":
                    memberType = MemberType.Guitar;
                    return true;
                case "drum":
                case "drums":
                    memberType = MemberType.Drums;
                    return true;
                case "bass":
                    memberType = MemberType.Bass;
                    return true;
                case "vocal":
                    memberType = MemberType.Vocal;
                    return true;
                case "keyboard":
                case "piano":
                    memberType = MemberType.Piano;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryMapLocation(string token, out LocationType locationType)
        {
            locationType = default;
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            switch (token.Trim().ToLowerInvariant())
            {
                case "downtown":
                    locationType = LocationType.Downtown;
                    return true;
                case "park":
                    locationType = LocationType.Park;
                    return true;
                case "academy":
                case "academydistrict":
                    locationType = LocationType.AcademyDistrict;
                    return true;
                case "livehouse":
                    locationType = LocationType.LiveHouse;
                    return true;
                case "shop":
                case "musicstore":
                    locationType = LocationType.MusicStore;
                    return true;
                default:
                    return false;
            }
        }

        private static string ResolveSourceFolder(string inputFolder)
        {
            if (string.IsNullOrWhiteSpace(inputFolder))
            {
                return string.Empty;
            }

            string normalized = inputFolder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized)
                && AssetDatabase.FindAssets("t:DialogueInformationSO", new[] { normalized }).Length > 0)
            {
                return normalized;
            }

            string csvToSo = normalized;
            csvToSo = csvToSo.Replace("/Csv/", "/SO/");
            if (csvToSo.EndsWith("/Csv", StringComparison.OrdinalIgnoreCase))
            {
                csvToSo = csvToSo.Substring(0, csvToSo.Length - "/Csv".Length) + "/SO";
            }

            if (AssetDatabase.IsValidFolder(csvToSo))
            {
                return csvToSo;
            }

            return normalized;
        }

        private void TryAutoAssignDefaultTarget()
        {
            if (_target != null)
            {
                return;
            }

            _target = AssetDatabase.LoadAssetAtPath<OutingDialogueDataSO>(DEFAULT_TARGET_ASSET_PATH);
        }

    }
}
