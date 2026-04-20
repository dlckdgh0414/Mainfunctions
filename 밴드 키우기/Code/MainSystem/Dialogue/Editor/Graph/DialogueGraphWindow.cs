using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Code.MainSystem.Dialogue;
using Code.MainSystem.Dialogue.Parser.Editor;
using UnityEngine.AddressableAssets;
using Member.LS.Code.Dialogue.Character;

namespace Code.MainSystem.Dialogue.Editor.Graph
{
    /// <summary>
    /// 다이얼로그 그래프를 시각적으로 편집하는 에디터 창
    /// </summary>
    public class DialogueGraphWindow : EditorWindow
    {
        private DialogueGraphView _graphView;
        private IMGUIContainer _inspectorContainer;
        private TwoPaneSplitView _splitView;
        
        private DialogueInformationSO _currentSO;
        private SerializedObject _serializedSO;
        private string _selectedNodeID;
        
        private DialogueDatabaseSO _database;
        private string[] _characterIDs;
        private string[] _backgroundIDs;
        private Vector2 _scrollPos;

        [MenuItem("Tools/Dialogue Graph Editor")]
        public static void OpenWindow()
        {
            DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            RefreshDatabase();
            ConstructLayout();
        }

        private void OnDisable()
        {
            if (_graphView != null && rootVisualElement.Contains(_splitView))
            {
                rootVisualElement.Remove(_splitView);
            }
        }

        private void RefreshDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueDatabaseSO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _database = AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(path);
                
                if (_database != null)
                {
                    SerializedObject so = new SerializedObject(_database);
                    _characterIDs = GetIDsFromProperty(so.FindProperty("characterEntries"));
                    _backgroundIDs = GetIDsFromProperty(so.FindProperty("backgroundEntries"));
                }
            }
        }

        private string[] GetIDsFromProperty(SerializedProperty listProp)
        {
            if (listProp == null) return new string[0];
            string[] ids = new string[listProp.arraySize];
            for (int i = 0; i < listProp.arraySize; i++)
            {
                ids[i] = listProp.GetArrayElementAtIndex(i).FindPropertyRelative("Id").stringValue;
            }
            return ids;
        }

        private void ConstructLayout()
        {
            rootVisualElement.Clear();

            Toolbar toolbar = new Toolbar();
            
            ObjectField soField = new ObjectField("Target SO")
            {
                objectType = typeof(DialogueInformationSO),
                allowSceneObjects = false,
                value = _currentSO
            };
            soField.RegisterValueChangedCallback(evt =>
            {
                _currentSO = evt.newValue as DialogueInformationSO;
                if (_currentSO != null)
                {
                    _serializedSO = new SerializedObject(_currentSO);
                    _selectedNodeID = null;
                    _graphView.PopulateFromSO(_currentSO);
                    _graphView.FrameAll();
                }
                else
                {
                    _serializedSO = null;
                    _graphView.ClearGraphSafely();
                }
            });
            toolbar.Add(soField);

            toolbar.Add(new ToolbarSpacer());

            Button saveBtn = new Button(() => { if (_graphView != null) _graphView.SaveToSO(); }) { text = "💾 Save Graph" };
            toolbar.Add(saveBtn);

            Button validateBtn = new Button(() => { ValidateDialogueData(); }) { text = "✔️ Validate" };
            toolbar.Add(validateBtn);

            Button importBtn = new Button(() => { 
                if (_currentSO != null) 
                {
                    DialogueCSVImporter.ImportCSV(_currentSO);
                    _graphView.PopulateFromSO(_currentSO);
                    _graphView.FrameAll();
                }
            }) { text = "📥 CSV Import" };
            toolbar.Add(importBtn);

            Button exportBtn = new Button(() => {
                if (_currentSO != null)
                {
                    DialogueCSVImporter.ExportCSV(_currentSO);
                }
            }) { text = "📤 CSV Export" };
            toolbar.Add(exportBtn);

            Button frameBtn = new Button(() => { _graphView?.FrameAll(); }) { text = "🔍 Focus All" };
            toolbar.Add(frameBtn);

            rootVisualElement.Add(toolbar);

            _splitView = new TwoPaneSplitView(1, 350, TwoPaneSplitViewOrientation.Horizontal);
            _splitView.style.flexGrow = 1; 

            _graphView = new DialogueGraphView { name = "Graph View" };
            _graphView.style.flexGrow = 1;
            _graphView.onNodeSelected = (id) => { _selectedNodeID = id; _inspectorContainer.MarkDirtyRepaint(); };
            _graphView.onNodeUnselected = () => { _selectedNodeID = null; _inspectorContainer.MarkDirtyRepaint(); };
            _graphView.onGraphChanged = () => { _inspectorContainer.MarkDirtyRepaint(); };

            _inspectorContainer = new IMGUIContainer(DrawInspector) { name = "Inspector" };
            _inspectorContainer.style.width = StyleKeyword.Auto;
            _inspectorContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            _splitView.Add(_graphView);
            _splitView.Add(_inspectorContainer);

            rootVisualElement.Add(_splitView);

            if (_currentSO != null)
            {
                _serializedSO = new SerializedObject(_currentSO);
                _graphView.PopulateFromSO(_currentSO);
                rootVisualElement.schedule.Execute(() => _graphView.FrameAll()).StartingIn(100);
            }
        }

        private void DrawInspector()
        {
            if (_currentSO == null || _serializedSO == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Select a DialogueInformationSO asset above", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            _serializedSO.Update();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // --- 1. 노드가 선택된 경우: 노드 세부 편집 ---
            if (!string.IsNullOrEmpty(_selectedNodeID))
            {
                SerializedProperty nodesProp = _serializedSO.FindProperty("dialogueNodes");
                int index = -1;
                for (int i = 0; i < nodesProp.arraySize; i++)
                {
                    if (nodesProp.GetArrayElementAtIndex(i).FindPropertyRelative("NodeID").stringValue == _selectedNodeID)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(index);
                    EditorGUILayout.LabelField($"✏️ Editing Node: {_selectedNodeID}", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    // --- 1. 노드 기본 정보 영역 ---
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("📌 Node Configuration", EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative("NodeID"), new GUIContent("Node ID"));
                    DrawIDPopup("Character ID", nodeProp.FindPropertyRelative("CharacterID"), _characterIDs);
                    EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative("CharacterEmotion"), new GUIContent("Emotion State"));
                    EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative("NameTagPosition"), new GUIContent("Nametag Side"));
                    DrawIDPopup("Background ID", nodeProp.FindPropertyRelative("BackgroundID"), _backgroundIDs);
                    EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative("VoiceID"), new GUIContent("Voice ID"));

                    if (EditorGUI.EndChangeCheck())
                    {
                        // 데이터를 먼저 SO에 확정
                        _serializedSO.ApplyModifiedProperties(); 
                        
                        // ID 등이 바뀌었으므로 전체 재구축 (단, 위치 보존을 위해 현재 상태 저장 선행)
                        _graphView.SaveToSO();
                        _selectedNodeID = nodeProp.FindPropertyRelative("NodeID").stringValue;
                        _graphView.PopulateFromSO(_currentSO);
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);

                    // --- 2. 대사 내용 텍스트 영역 ---
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("💬 Dialogue Content", EditorStyles.boldLabel);
                    SerializedProperty detailProp = nodeProp.FindPropertyRelative("DialogueDetail");

                    EditorGUI.BeginChangeCheck();
                    detailProp.stringValue = EditorGUILayout.TextArea(detailProp.stringValue, GUILayout.Height(100));

                    if (EditorGUI.EndChangeCheck())
                    {
                        // 데이터를 먼저 SO에 확정
                        _serializedSO.ApplyModifiedProperties();
                        
                        // 전체 재구축(PopulateFromSO) 대신 텍스트만 실시간 갱신
                        _graphView.UpdateNodeContent(_selectedNodeID, detailProp.stringValue); 
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("⚡ Logic Commands", EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative("Commands"), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SyncGraphToSerializedObject();
                        _serializedSO.ApplyModifiedProperties(); 
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);

                    // --- 4. 선택지 영역 ---
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("🔀 Choices (선택지 설정)", EditorStyles.boldLabel);

                    SerializedProperty choicesProp = nodeProp.FindPropertyRelative("Choices");
                    int oldChoiceCount = choicesProp.arraySize;

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(choicesProp, new GUIContent("Choices List"), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        SyncGraphToSerializedObject();
                        _serializedSO.ApplyModifiedProperties();

                        if (oldChoiceCount != choicesProp.arraySize)
                        {
                            _graphView.PopulateFromSO(_currentSO);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);

                    DrawPreview(nodeProp);
                }
            }
            // --- 2. 노드가 선택되지 않은 경우: SO 전체 설정 (시작 노드 등) ---
            else
            {
                EditorGUILayout.LabelField("📄 Sequence Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("🚀 Start Configuration", EditorStyles.boldLabel);
                
                SerializedProperty startNodeProp = _serializedSO.FindProperty("startNodeID");
                string[] allNodeIDs = _currentSO.DialogueNodes.Select(n => n.NodeID).ToArray();
                
                if (allNodeIDs.Length > 0)
                {
                    DrawIDPopup("Start Node ID", startNodeProp, allNodeIDs);
                    if (GUI.changed) _graphView.UpdateStartNodeVisuals();
                }
                else
                {
                    EditorGUILayout.HelpBox("No nodes available in this sequence.", MessageType.Info);
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(20);
            EditorGUILayout.EndScrollView();

            _serializedSO.ApplyModifiedProperties();
        }

        private void SyncGraphToSerializedObject()
        {
            if (_graphView != null) _graphView.SaveToSO();
        }

        private void DrawIDPopup(string label, SerializedProperty idProp, string[] idList)
        {
            if (idList == null || idList.Length == 0)
            {
                EditorGUILayout.PropertyField(idProp, new GUIContent(label));
                return;
            }

            int currentIndex = Array.IndexOf(idList, idProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 2));
            int newIndex = EditorGUILayout.Popup(currentIndex, idList);
            if (newIndex != currentIndex || string.IsNullOrEmpty(idProp.stringValue))
            {
                idProp.stringValue = idList[newIndex];
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPreview(SerializedProperty node)
        {
            EditorGUILayout.LabelField("🖼️ Viewport Preview", EditorStyles.boldLabel);
            Rect previewRect = EditorGUILayout.GetControlRect(false, 180);
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));

            if (_database == null) return;

            string charID = node.FindPropertyRelative("CharacterID").stringValue;
            string bgID = node.FindPropertyRelative("BackgroundID").stringValue;
            CharacterEmotionType emotion = (CharacterEmotionType)node.FindPropertyRelative("CharacterEmotion").enumValueIndex;

            // 1. 배경 그리기
            if (!string.IsNullOrEmpty(bgID))
            {
                UnityEngine.AddressableAssets.AssetReferenceSprite bgRef = _database.GetBackground(bgID);
                if (bgRef != null && bgRef.editorAsset != null)
                {
                    Sprite bgSprite = bgRef.editorAsset as Sprite;
                    if (bgSprite != null) GUI.DrawTexture(previewRect, bgSprite.texture, ScaleMode.ScaleAndCrop);
                }
            }

            // 2. 캐릭터 그리기
            if (!string.IsNullOrEmpty(charID))
            {
                UnityEngine.AddressableAssets.AssetReferenceT<Member.LS.Code.Dialogue.Character.CharacterInformationSO> charRef = _database.GetCharacter(charID);
                if (charRef != null && charRef.editorAsset != null)
                {
                    Member.LS.Code.Dialogue.Character.CharacterInformationSO charInfo = charRef.editorAsset as Member.LS.Code.Dialogue.Character.CharacterInformationSO;
                    if (charInfo != null && charInfo.CharacterEmotions.TryGetValue(emotion, out UnityEngine.AddressableAssets.AssetReferenceSprite emoRef))
                    {
                        Sprite charSprite = emoRef.editorAsset as Sprite;
                        if (charSprite != null)
                        {
                            float aspect = (float)charSprite.texture.width / charSprite.texture.height;
                            float charHeight = previewRect.height * 0.95f;
                            float charWidth = charHeight * aspect;
                            Rect charRect = new Rect(
                                previewRect.x + (previewRect.width - charWidth) * 0.5f,
                                previewRect.y + (previewRect.height - charHeight),
                                charWidth,
                                charHeight
                            );
                            GUI.DrawTexture(charRect, charSprite.texture, ScaleMode.ScaleToFit);
                        }
                    }
                }
            }

            // 3. 하단 대사 텍스트창 그리기
            Rect textRect = new Rect(previewRect.x, previewRect.yMax - 35, previewRect.width, 35);
            EditorGUI.DrawRect(textRect, new Color(0, 0, 0, 0.7f));
            GUI.Label(new Rect(textRect.x + 5, textRect.y + 2, textRect.width - 10, textRect.height - 4), 
                node.FindPropertyRelative("DialogueDetail").stringValue, EditorStyles.whiteMiniLabel);

            // 4. 선택지 목록 그리기 (추가된 로직)
            SerializedProperty choicesProp = node.FindPropertyRelative("Choices");
            if (choicesProp != null && choicesProp.arraySize > 0)
            {
                int choiceCount = choicesProp.arraySize;
                float choiceHeight = 18f; // 선택지 하나당 높이
                float spacing = 3f;       // 선택지 사이 간격
                float totalChoiceHeight = (choiceHeight + spacing) * choiceCount;

                // 선택지 영역은 대사창(textRect) 바로 위쪽에 렌더링
                Rect choiceAreaRect = new Rect(previewRect.x + 20f, textRect.y - totalChoiceHeight - 5f, previewRect.width - 40f, totalChoiceHeight);

                GUIStyle choiceStyle = new GUIStyle(EditorStyles.whiteMiniLabel) { alignment = TextAnchor.MiddleCenter };

                for (int i = 0; i < choiceCount; i++)
                {
                    SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    string choiceText = choiceProp.FindPropertyRelative("ChoiceText").stringValue;
                    string subText = choiceProp.FindPropertyRelative("SubText").stringValue;
                    
                    // 텍스트가 없는 연결은 '자동 진행(Auto)'이므로 UI로 그리지 않음
                    if (string.IsNullOrEmpty(choiceText)) continue;

                    Rect singleChoiceRect = new Rect(choiceAreaRect.x, choiceAreaRect.y + (choiceHeight + spacing) * i, choiceAreaRect.width, choiceHeight);
                    
                    // 반투명한 선택지 버튼 배경
                    EditorGUI.DrawRect(singleChoiceRect, new Color(0.2f, 0.2f, 0.2f, 0.9f));
                    
                    // 텍스트 그리기
                    GUI.Label(singleChoiceRect, choiceText, choiceStyle);

                    if (!string.IsNullOrWhiteSpace(subText))
                    {
                        Rect subTextRect = new Rect(singleChoiceRect.x, singleChoiceRect.yMax - 8f, singleChoiceRect.width, 10f);
                        GUI.Label(subTextRect, subText, EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        private void ValidateDialogueData()
        {
            if (_currentSO == null) return;
            _graphView.SaveToSO();
            _graphView.ClearHighlights();

            List<DialogueNode> nodes = _currentSO.DialogueNodes;
            DialogueDataValidator.ValidationResult result = DialogueDataValidator.Validate(nodes, _characterIDs, _backgroundIDs);

            foreach (string nodeId in result.invalidNodeIds)
            {
                _graphView.HighlightNode(nodeId, Color.red);
            }

            if (result.HasErrors)
            {
                Debug.LogError($"[Dialogue Validation] Failed with {result.errors.Count} errors.\n" + string.Join("\n", result.errors));
                EditorUtility.DisplayDialog("Validation Error", "Errors found in dialogue data. Check console and red nodes.", "Fix It");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Success", "All nodes are correctly linked and assets are valid!", "Great");
            }
        }
    }
}
