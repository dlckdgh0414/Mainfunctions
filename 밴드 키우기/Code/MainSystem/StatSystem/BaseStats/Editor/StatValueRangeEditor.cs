using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Code.MainSystem.StatSystem.BaseStats.Editor
{
    public class StatValueRangeEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset view = default;

        private readonly List<StatValueRange> _statDateRanges = new List<StatValueRange>();
        
        private const string Path = "Assets/AddressableAssets/SO/StatData/StatRange";
        private const string AddressableGroupName = "StatData";
        
        private ListView _itemListView;
        
        private Button _createButton;
        private Button _removeButton;

        private TextField _assetName;
        private IntegerField _minValue;
        private IntegerField _maxValue;
        
        [MenuItem("Tools/Stat/Stat Value Range Manger")]
        private static void ShowWindow()
        {
            GetWindow<StatValueRangeEditor>("Stat Value Range Manger").Show();
        }
        
        public void CreateGUI()
        {
            if (view == null)
                return;

            view.CloneTree(rootVisualElement);

            BindVisualElements();
            LoadAssets();
            SetupListView();
            RegisterCallbacks();
        }

        private void BindVisualElements()
        {
            _itemListView = rootVisualElement.Q<ListView>("ItemListView");
            _createButton =  rootVisualElement.Q<Button>("CreateButton");
            _removeButton = rootVisualElement.Q<Button>("RemoveButton");
            
            _minValue  = rootVisualElement.Q<IntegerField>("MinValueField");
            _maxValue  = rootVisualElement.Q<IntegerField>("MaxValueField");
            _assetName =  rootVisualElement.Q<TextField>("AssetNameField");
        }
        
        private void LoadAssets()
        {
            _statDateRanges.Clear();
            
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                AssetDatabase.Refresh(); 
            }

            string[] guids = AssetDatabase.FindAssets("t:StatValueRange", new[] { Path });
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                _statDateRanges.Add(AssetDatabase.LoadAssetAtPath<StatValueRange>(assetPath));
            }
        }
        
        private void SetupListView()
        {
            _itemListView.itemsSource = _statDateRanges;
            _itemListView.makeItem = () => new Label();
            _itemListView.bindItem = (element, i) =>
            {
                ((Label)element).text = _statDateRanges[i].name;
            };
            
            _itemListView.selectionChanged += (objects) =>
            {
                var selected = objects.FirstOrDefault() as StatValueRange;
                if (selected != null)
                {
                    _assetName.SetEnabled(true);
                    _minValue.SetEnabled(true);
                    _maxValue.SetEnabled(true);
                    
                    _assetName.SetValueWithoutNotify(selected.name);
                    _minValue.SetValueWithoutNotify(selected.Min);
                    _maxValue.SetValueWithoutNotify(selected.Max);
                    
                    EditorUtility.SetDirty(selected);
                }
                else
                {
                    ClearDetail();
                }
            };
        }
        
        private void RegisterCallbacks()
        {
            _createButton.clicked += CreateNewAsset;
            _removeButton.clicked += RemoveSelectedAsset;
            
            _assetName.RegisterValueChangedCallback(evt => 
            {
                var selected = _itemListView.selectedItem as StatValueRange;
                if (selected == null || string.IsNullOrEmpty(evt.newValue)) 
                    return;
                
                string assetPath = AssetDatabase.GetAssetPath(selected);
                string result = AssetDatabase.RenameAsset(assetPath, evt.newValue);
        
                if (string.IsNullOrEmpty(result))
                {
                    AddToAddressable(AssetDatabase.GetAssetPath(selected), evt.newValue);
                    _itemListView.Rebuild();
                }
            });
            
            _minValue.RegisterValueChangedCallback(evt => 
            {
                var selected = _itemListView.selectedItem as StatValueRange;
                if (selected != null)
                {
                    selected.Min = evt.newValue;
                    EditorUtility.SetDirty(selected);
                }
            });
            
            _maxValue.RegisterValueChangedCallback(evt => 
            {
                var selected = _itemListView.selectedItem as StatValueRange;
                if (selected != null)
                {
                    selected.Max = evt.newValue;
                    EditorUtility.SetDirty(selected);
                }
            });
            
            AssetDatabase.SaveAssets();
        }

        private void CreateNewAsset()
        {
            if (!System.IO.Directory.Exists(Path))
                System.IO.Directory.CreateDirectory(Path);

            StatValueRange newAsset = CreateInstance<StatValueRange>();

            string fileName = string.IsNullOrEmpty(_assetName.value) ? "NewStatRange" : _assetName.value;
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{Path}/{fileName}.asset");
            
            AssetDatabase.CreateAsset(newAsset, fullPath);
            AssetDatabase.SaveAssets();

            AddToAddressable(fullPath, fileName);
            
            LoadAssets();
            _itemListView.Rebuild();
            
            int index = _statDateRanges.IndexOf(newAsset);
            if (index >= 0)
                _itemListView.SetSelection(index);
        }
        
        private void AddToAddressable(string assetPath, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;
            
            var group = settings.FindGroup(AddressableGroupName);
            if (group == null)
                group = settings.CreateGroup(AddressableGroupName, false, false, true, null);
            
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
        }
        
        private void RemoveSelectedAsset()
        {
            StatValueRange selected = _itemListView.selectedItem as StatValueRange;
            if (selected == null) 
                return;

            string path = AssetDatabase.GetAssetPath(selected);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();

            LoadAssets();
            _itemListView.Rebuild();
            
            _itemListView.ClearSelection();
            ClearDetail();
        }
        
        private void ClearDetail()
        {
            _assetName.value = "";
            _minValue.value = 0;
            _maxValue.value = 0;
            
            _assetName.SetEnabled(false);
            _minValue.SetEnabled(false);
            _maxValue.SetEnabled(false);
        }
    }
}