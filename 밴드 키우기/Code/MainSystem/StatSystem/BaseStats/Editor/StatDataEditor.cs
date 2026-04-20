using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Core;

namespace Code.MainSystem.StatSystem.BaseStats.Editor
{
    public class StatDataEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset view = default;

        private ListView _assetListView;
        private VisualElement _detailContainer;
        private EnumField _memberEnumField;
        private Label _listTitle;

        private List<StatData> _filteredAssets = new();

        private const string BasePath = "Assets/AddressableAssets/SO/StatData/Stats";
        private MemberType _currentMember = MemberType.Guitar;

        [MenuItem("Tools/Stat/Stat Data Manager")]
        public static void ShowWindow()
            => GetWindow<StatDataEditor>("Stat Data Manager").Show();

        public void CreateGUI()
        {
            if (view == null)
                return;

            view.CloneTree(rootVisualElement);

            BindVisualElements();
            SetupEventHandlers();
            RefreshAssetList();
        }

        private void BindVisualElements()
        {
            _assetListView = rootVisualElement.Q<ListView>("AssetListView");
            _detailContainer = rootVisualElement.Q<VisualElement>("DetailContainer");
            _listTitle = rootVisualElement.Q<Label>("ListTitle");

            _memberEnumField = rootVisualElement.Q<EnumField>("MemberEnumField");
            _memberEnumField.Init(_currentMember);

            _assetListView.makeItem = () => new Label();

            _assetListView.bindItem = (e, i) =>
            {
                var asset = _filteredAssets[i];
                ((Label)e).text = string.IsNullOrEmpty(asset.statName) ? asset.name : asset.statName;
            };

            _assetListView.itemsSource = _filteredAssets;
        }

        private void SetupEventHandlers()
        {
            _memberEnumField.RegisterValueChangedCallback(evt =>
            {
                _currentMember = (MemberType)evt.newValue;

                _assetListView.ClearSelection();
                _detailContainer.Clear();

                RefreshAssetList();
            });

            _assetListView.selectionChanged += OnAssetSelected;
        }

        private void RefreshAssetList()
        {
            _filteredAssets.Clear();
            string targetFolder = $"{BasePath}/{_currentMember}";

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StatData)}", new[] { targetFolder });
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<StatData>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) _filteredAssets.Add(asset);
            }

            _listTitle.text = $"{_currentMember} Category List ({_filteredAssets.Count})";
            _assetListView.Rebuild();
        }

        private void OnAssetSelected(IEnumerable<object> selectedItems)
        {
            StatData selected = selectedItems.FirstOrDefault() as StatData;
            if (selected == null)
            {
                _detailContainer.Clear();
                return;
            }

            _detailContainer.Clear();

            TextField nameField = new TextField("Asset Name") { value = selected.name };
            nameField.RegisterCallback<FocusOutEvent>(_ => RenameAsset(selected, nameField.value));
            _detailContainer.Add(nameField);

            SerializedObject serializedObject = new SerializedObject(selected);
            InspectorElement inspectorElement = new InspectorElement(serializedObject);
            _detailContainer.Add(inspectorElement);
        }

        private void RenameAsset(StatData asset, string newName)
        {
            if (string.IsNullOrEmpty(newName) || asset.name == newName)
                return;

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), newName);
            AssetDatabase.SaveAssets();
            RefreshAssetList();
        }
    }
}