using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Code.MainSystem.StatSystem.BaseStats.Editor
{
    public class StatRankTableEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset view = default;

        private ListView _assetListView;
        private ListView _enumListView;
        private VisualElement _detailContainer;
        private TextField _enumInputField;

        private List<StatRankTable> _foundAssets = new();
        private List<string> _currentEnumNames = new();
        
        private const string AssetPath = "Assets/AddressableAssets/SO/StatData/RankData";
        private const string EnumFilePath = "Assets/Code/MainSystem/StatSystem/BaseStats/StatRankType.cs";
        private const string WindowTitle = "Stat Rank Manager";
        private const string TargetGroupName = "StatData";

        [MenuItem("Tools/Stat/Stat Rank Manager")]
        public static void ShowWindow()
            => GetWindow<StatRankTableEditor>(WindowTitle).Show();

        public void CreateGUI()
        {
            if (view == null)
                return;

            view.CloneTree(rootVisualElement);
            BindVisualElements();
            SetupEventHandlers();

            RefreshAssetList();
            RefreshEnumList();
        }

        private void BindVisualElements()
        {
            _assetListView = rootVisualElement.Q<ListView>("AssetListView");
            _enumListView = rootVisualElement.Q<ListView>("EnumListView");
            _detailContainer = rootVisualElement.Q<VisualElement>("DetailContainer");
            _enumInputField = rootVisualElement.Q<TextField>("EnumInputField");
            
            _assetListView.makeItem = () => new Label();
            _assetListView.bindItem = (e, i) => ((Label)e).text = _foundAssets[i].name;
            _assetListView.itemsSource = _foundAssets;
            
            _enumListView.makeItem = () => new Label();
            _enumListView.bindItem = (e, i) => ((Label)e).text = _currentEnumNames[i];
            _enumListView.itemsSource = _currentEnumNames;
        }

        private void SetupEventHandlers()
        {
            _assetListView.selectionChanged += OnAssetSelected;

            rootVisualElement.Q<Button>("AddAssetButton").clicked += CreateNewAsset;
            rootVisualElement.Q<Button>("RemoveAssetButton").clicked += RemoveSelectedAsset;

            _enumListView.selectionChanged += (items) =>
            {
                if (items.FirstOrDefault() is string val) _enumInputField.value = val;
            };

            rootVisualElement.Q<Button>("AddEnumButton").clicked += () => AddEnumValue(_enumInputField.value);
            rootVisualElement.Q<Button>("RemoveEnumButton").clicked += () => RemoveEnumValue(_enumInputField.value);
        }

        #region Asset Management

        private void RefreshAssetList()
        {
            _foundAssets.Clear();
            
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StatRankTable)}");
            foreach (var guid in guids)
            {
                StatRankTable asset = AssetDatabase.LoadAssetAtPath<StatRankTable>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    _foundAssets.Add(asset);
            }
            
            _assetListView.Rebuild();
            
            if (_foundAssets.Count == 0)
            {
                _detailContainer.Clear();
            }
        }

        private void OnAssetSelected(IEnumerable<object> selectedItems)
        {
            _detailContainer.Clear();
            if (selectedItems.FirstOrDefault() is not StatRankTable selected) return;
            
            var nameField = new TextField("Asset Name") { value = selected.name };
            nameField.RegisterCallback<FocusOutEvent>(_ => RenameAsset(selected, nameField.value));
            _detailContainer.Add(nameField);
            
            _detailContainer.Add(new VisualElement
                { style = { height = 2, backgroundColor = Color.gray, marginTop = 5, marginBottom = 10 } });
            
            var serializedObject = new SerializedObject(selected);
            var inspectorElement = new InspectorElement(serializedObject);
            _detailContainer.Add(inspectorElement);
        }

        private void CreateNewAsset()
        {
            if (!Directory.Exists(AssetPath))
                Directory.CreateDirectory(AssetPath);

            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{AssetPath}/NewStatRank.asset");
            var newInstance = CreateInstance<StatRankTable>();

            AssetDatabase.CreateAsset(newInstance, fullPath);
            AssetDatabase.SaveAssets();

            AddToAddressables(newInstance);
            
            RefreshAssetList();
            
            _assetListView.SetSelection(_foundAssets.IndexOf(newInstance));
        }

        private void RenameAsset(StatRankTable table, string newName)
        {
            if (string.IsNullOrEmpty(newName) || table.name == newName) return;

            string oldPath = AssetDatabase.GetAssetPath(table);
            string error = AssetDatabase.RenameAsset(oldPath, newName);
            
            if (string.IsNullOrEmpty(error))
            {
                AssetDatabase.SaveAssets();
                UpdateAddressableAddress(table, newName);
                RefreshAssetList();
            }
            else
            {
                Debug.LogError($"Rename Error: {error}");
            }
        }

        private void RemoveSelectedAsset()
        {
            StatRankTable selectedItem = _assetListView.selectedItem as StatRankTable;
            if (selectedItem == null) return;

            if (!EditorUtility.DisplayDialog("삭제 확인", $"'{selectedItem.name}' 에셋을 정말 삭제하시겠습니까?", "삭제", "취소"))
                return;
            
            string path = AssetDatabase.GetAssetPath(selectedItem);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            
            _detailContainer.Clear();
            _assetListView.ClearSelection();
    
            RefreshAssetList();
        }
        
        /// <summary>
        /// 에셋을 Addressable 그룹에 추가하고 이름을 설정합니다.
        /// </summary>
        private void AddToAddressables(StatRankTable asset)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var group = GetOrCreateGroup(settings, TargetGroupName);
            
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = asset.name; // 주소를 에셋 이름으로 설정
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }

        /// <summary>
        /// 이름 변경 시 Addressable 주소를 갱신합니다.
        /// </summary>
        private void UpdateAddressableAddress(StatRankTable asset, string newName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = settings.FindAssetEntry(guid);
            
            if (entry != null)
            {
                entry.address = newName;
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
            }
        }

        private AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, settings.DefaultGroup.Schemas);
            }
            return group;
        }

        #endregion

        #region Enum Generation

        private void RefreshEnumList()
        {
            _currentEnumNames.Clear();
            _currentEnumNames.AddRange(Enum.GetNames(typeof(StatRankType)));
            _enumListView.Rebuild();
        }

        private void AddEnumValue(string rankName)
        {
            rankName = rankName?.Trim();
            if (string.IsNullOrEmpty(rankName) || _currentEnumNames.Contains(rankName)) return;

            _currentEnumNames.Add(rankName);
            WriteEnumToFile();
        }

        private void RemoveEnumValue(string rankName)
        {
            if (rankName == "None" || !_currentEnumNames.Contains(rankName)) return;

            if (!EditorUtility.DisplayDialog("Delete Enum Value",
                    $"'{rankName}' 타입을 삭제하면 이를 사용하는 모든 SO 데이터가 손실될 수 있습니다. 진행하시겠습니까?", "삭제", "취소"))
                return;
            _currentEnumNames.Remove(rankName);
            WriteEnumToFile();
        }

        private void WriteEnumToFile()
        {
            try
            {
                string content = GenerateEnumContent(_currentEnumNames);
                File.WriteAllText(EnumFilePath, content);
                AssetDatabase.Refresh();
                Debug.Log("<color=green><b>[StatRankEditor]</b></color> Enum updated. Waiting for compilation...");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update Enum file: {e.Message}");
            }
        }

        private string GenerateEnumContent(List<string> names)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace Code.MainSystem.StatSystem.BaseStats");
            sb.AppendLine("{");
            sb.AppendLine("    public enum StatRankType");
            sb.AppendLine("    {");
            for (int i = 0; i < names.Count; i++)
            {
                sb.Append("        ").Append(names[i]).AppendLine(i == names.Count - 1 ? "" : ",");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion
    }
}