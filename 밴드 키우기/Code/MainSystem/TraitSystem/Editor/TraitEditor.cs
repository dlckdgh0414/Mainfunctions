using System;
using System.Collections.Generic;
using System.Linq;
using Code.MainSystem.TraitSystem.Data;
using Code.MainSystem.TraitSystem.TraitEffect;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.MainSystem.TraitSystem.Editor
{
    public class TraitEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset mainView = default;
        [SerializeField] private VisualTreeAsset itemView = default;
        [SerializeField] private VisualTreeAsset commentView = default;

        private List<TraitDataSO> _traits = new();
        private SerializedObject _serializedTrait;
        
        private ListView _traitListView;
        private ListView _impactListView;
        private ListView _commentListView;
        private VisualElement _detailPanel;
        
        private const string TraitDataPath = "Assets/AddressableAssets/SO/TraitData";

        [MenuItem("Tools/Trait/Trait Editor")]
        public static void ShowWindow() 
            => GetWindow<TraitEditor>("Trait Editor");

        public void CreateGUI()
        {
            if (mainView == null || itemView == null)
                return;

            mainView.CloneTree(rootVisualElement);

            _traitListView = rootVisualElement.Q<ListView>("trait-list");
            _impactListView = rootVisualElement.Q<ListView>("impact-list");
            _commentListView = rootVisualElement.Q<ListView>("member-comment-list");
            _detailPanel = rootVisualElement.Q<VisualElement>("base-settings");

            SetupTraitListView();
            SetupImpactListView();
            
            rootVisualElement.Q<Button>("add-trait-btn").clicked += CreateNewTrait;
            rootVisualElement.Q<Button>("add-pair-btn").clicked += AddEffectPair;
            
           
            var genBtn = rootVisualElement.Q<Button>("gen-comments-btn");
            if (genBtn != null)
                genBtn.clicked += GenerateDefaultComments;

            RefreshTraitList();
        }

        private void SetupTraitListView()
        {
            _traitListView.makeItem = () => new Label();
            _traitListView.bindItem = (e, i) =>
            {
                var label = e as Label;
                if (i >= _traits.Count)
                    return;
                
                TraitDataSO trait = _traits[i];
                if (label != null) 
                    label.text = string.IsNullOrEmpty(trait.TraitName) ? trait.name : trait.TraitName;
            };

            _traitListView.selectionChanged += (objs) => 
                DrawTraitDetail(objs.FirstOrDefault() as TraitDataSO);
        }

        private void SetupImpactListView()
        {
            _impactListView.makeItem = () => itemView.CloneTree();
            _impactListView.bindItem = BindImpactItem;
        }

        private void BindImpactItem(VisualElement element, int i)
        {
            if (_serializedTrait == null) 
                return;

            SerializedProperty effectsProp = _serializedTrait.FindProperty("Effects");
            SerializedProperty impactsProp = _serializedTrait.FindProperty("Impacts");

            if (i >= effectsProp.arraySize || i >= impactsProp.arraySize)
                return;

            element.Q<Label>("id-label").text = $"N{i + 1}";
            
            SetField(element, "effect-field", effectsProp.GetArrayElementAtIndex(i));
            
            SerializedProperty impactElem = impactsProp.GetArrayElementAtIndex(i);
            SetField(element, "target-field", impactElem.FindPropertyRelative("Target"));
            SetField(element, "calc-field", impactElem.FindPropertyRelative("CalcType"));
            SetField(element, "tag-field", impactElem.FindPropertyRelative("RequiredTag"));
            
            Button removeBtn = element.Q<Button>("remove-btn");
        
            removeBtn.clickable = new Clickable(() => RemoveEffectPair(i));
        }

        private void SetField(VisualElement root, string name, SerializedProperty prop)
        {
            var field = root.Q<PropertyField>(name);
            if (field == null)
                return;
            
            field.BindProperty(prop);
            field.label = "";
        }

        private void DrawTraitDetail(TraitDataSO target)
        {
            _detailPanel.Unbind();
            if (target == null)
                return;

            _detailPanel.style.display = DisplayStyle.Flex;
            _serializedTrait = new SerializedObject(target);
            _detailPanel.Bind(_serializedTrait);
            
            SetupSpecialLogicDropdown(target);
            SetupMemberComments(target, _serializedTrait);

            _impactListView.itemsSource = target.Effects;
            _impactListView.Rebuild();

            _detailPanel.TrackSerializedObjectValue(_serializedTrait, _ => { _traitListView.RefreshItems(); });
        }
        
        private void SetupMemberComments(TraitDataSO target, SerializedObject so)
        {
            if (_commentListView == null)
                return;

            SerializedProperty commentProp = so.FindProperty("MemberComments");
            _commentListView.itemsSource = target.MemberComments;
            
            _commentListView.fixedItemHeight = 120;
            _commentListView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;

            _commentListView.makeItem = () =>
            {
                if (commentView != null)
                    return commentView.CloneTree();
        
                return new Label("MemberCommentItem.uxml을 할당해주세요.");
            };

            _commentListView.bindItem = (element, i) =>
            {
                if (i >= commentProp.arraySize) return;
    
                SerializedProperty prop = commentProp.GetArrayElementAtIndex(i);
                SerializedProperty typeProp = prop.FindPropertyRelative("MemberType");
                SerializedProperty titleProp = prop.FindPropertyRelative("Title");
                SerializedProperty contentProp = prop.FindPropertyRelative("Content");
                SerializedProperty thoughtProp = prop.FindPropertyRelative("Thoughts");
                
                PropertyField fType = element.Q<PropertyField>("m-type");
                if (fType != null) 
                {
                    fType.label = "";
                    fType.BindProperty(typeProp);
                }
                
                PropertyField fTitle = element.Q<PropertyField>("m-title");
                if (fTitle != null)
                {
                    fTitle.label = "";
                    fTitle.BindProperty(titleProp);
                }
                
                PropertyField fContent = element.Q<PropertyField>("m-content");
                if (fContent != null)
                {
                    fContent.label = "";
                    fContent.BindProperty(contentProp);
                }
                
                PropertyField fThought = element.Q<PropertyField>("m-thought");
                if (fThought != null)
                {
                    fThought.label = "";
                    fThought.BindProperty(thoughtProp);
                }
            };
        }

        private void GenerateDefaultComments()
        {
            if (_serializedTrait == null)
                return;
            
            TraitDataSO target = _serializedTrait.targetObject as TraitDataSO;
            
            if (target == null)
                return;

            _serializedTrait.Update();
            var prop = _serializedTrait.FindProperty("MemberComments");
            
            if (prop.arraySize > 0)
                return;
            
            for (int i = 0; i < 5; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("MemberType").enumValueIndex = i;
                element.FindPropertyRelative("Content").stringValue = "";
                element.FindPropertyRelative("Title").stringValue = "";
                element.FindPropertyRelative("Thoughts").stringValue = "";
            }

            _serializedTrait.ApplyModifiedProperties();
            _commentListView.Rebuild();
        }

        private void SetupSpecialLogicDropdown(TraitDataSO target)
        {
            List<string> effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(AbstractTraitEffect).IsAssignableFrom(p) && !p.IsAbstract)
                .Select(t => t.FullName)
                .ToList();
    
            const string defaultNone = "None (Default)";
            effectTypes.Insert(0, defaultNone);

            VisualElement container = _detailPanel.Q<VisualElement>("logic-dropdown-container");
            if (container == null) return;

            container.Clear();
            
            string currentSelection = string.IsNullOrEmpty(target.SpecialLogicClassName) 
                ? defaultNone 
                : target.SpecialLogicClassName;
            
            if (!effectTypes.Contains(currentSelection))
            {
                Debug.LogWarning($"저장된 클래스 {currentSelection}를 찾을 수 없어 기본값으로 표시합니다.");
                currentSelection = defaultNone;
            }
            
            PopupField<string> dropdown = new PopupField<string>("Special Logic", effectTypes, currentSelection);
    
            dropdown.RegisterValueChangedCallback(evt => {
                _serializedTrait.Update();
                SerializedProperty prop = _serializedTrait.FindProperty("SpecialLogicClassName");
                prop.stringValue = (evt.newValue == defaultNone) ? "" : evt.newValue;
                _serializedTrait.ApplyModifiedProperties();
            });
    
            container.Add(dropdown);
        }

        private void AddEffectPair()
        {
            if (_serializedTrait == null) 
                return;
            
            _serializedTrait.Update();
            SerializedProperty eProp = _serializedTrait.FindProperty("Effects");
            SerializedProperty iProp = _serializedTrait.FindProperty("Impacts");

            int idx = eProp.arraySize;
            eProp.InsertArrayElementAtIndex(idx);
            iProp.InsertArrayElementAtIndex(idx);

            _serializedTrait.ApplyModifiedProperties();
            _impactListView.Rebuild();
        }

        private void RemoveEffectPair(int index)
        {
            if (_serializedTrait == null) 
                return;
            
            _serializedTrait.Update();
            SerializedProperty eProp = _serializedTrait.FindProperty("Effects");
            SerializedProperty iProp = _serializedTrait.FindProperty("Impacts");

            if (index < eProp.arraySize)
            {
                eProp.DeleteArrayElementAtIndex(index);
                iProp.DeleteArrayElementAtIndex(index);
            }

            _serializedTrait.ApplyModifiedProperties();
            _impactListView.Rebuild();
        }

        private void RefreshTraitList()
        {
            _traits = AssetDatabase.FindAssets("t:TraitDataSO")
                .Select(guid => AssetDatabase.LoadAssetAtPath<TraitDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(t => t.TraitName)
                .ToList();

            _traitListView.itemsSource = _traits;
            _traitListView.Rebuild();
        }

        private void CreateNewTrait()
        {
            if (!AssetDatabase.IsValidFolder(TraitDataPath))
            {
                System.IO.Directory.CreateDirectory(TraitDataPath);
                AssetDatabase.Refresh();
            }
            
            string fileName = "NewTrait";
            string fullPath = $"{TraitDataPath}/{fileName}.asset";
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
            
            TraitDataSO asset = CreateInstance<TraitDataSO>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            
            RegisterAddressable(asset);
            
            RefreshTraitList();
            
            int newIndex = _traits.IndexOf(asset);
            _traitListView.SetSelection(newIndex);
        }

        private void RegisterAddressable(TraitDataSO asset)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;
            
            AddressableAssetGroup group = settings.FindGroup("TraitData");
            if (group == null) 
                group = settings.CreateGroup("TraitData", false, false, false, settings.DefaultGroup.Schemas);

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = asset.name; // 주소를 에셋 이름과 동일하게 설정
            
            entry.SetLabel("Trait", true);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
        }
    }
}