using System.Collections.Generic;
using System.Threading;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.Dialogue.Parser;
using Cysharp.Threading.Tasks;
using Member.LS.Code.Dialogue.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// 다이알로그의 흐름과 리소스 로딩을 총괄하는 매니저 클래스
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        private enum DialogueState { IDLE, ACTIVE }

        [SerializeField] private DialogueDatabaseSO dialogueDatabase;

        private DialogueState _state = DialogueState.IDLE;
        private DialogueInformationSO _dialogueInformation;
        private DialogueVariableContext _currentVariableContext;
        private string _currentNodeID;
        private bool _isAcceptingInput = false;
        private CancellationTokenSource _dialogueCts;

        public NameTagPositionType CurrentPosition => _nodeCache.ContainsKey(_currentNodeID) 
            ? _nodeCache[_currentNodeID].NameTagPosition 
            : NameTagPositionType.Left;

        private Dictionary<string, DialogueNode> _nodeCache = new Dictionary<string, DialogueNode>();

        private string _cachedCharacterID;
        private string _cachedBackgroundID;
        private CharacterEmotionType _cachedEmotion;
        
        private AsyncOperationHandle<CharacterInformationSO> _characterHandle;
        private AsyncOperationHandle<Sprite> _backgroundHandle;
        private AsyncOperationHandle<Sprite> _emotionSpriteHandle;
        private readonly HashSet<string> WARNED_EMPTY_DIALOGUE_NODE_IDS = new HashSet<string>();
        private readonly HashSet<string> WARNED_MISSING_CHARACTER_IDS = new HashSet<string>();
        private readonly HashSet<string> WARNED_MISSING_BACKGROUND_IDS = new HashSet<string>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Bus<ContinueDialogueEvent>.OnEvent += OnContinueDialogue;
            Bus<DialogueSkipEvent>.OnEvent += OnDialogueSkip;
            Bus<DialogueStartEvent>.OnEvent += OnDialogueStart;
            Bus<DialogueChoiceSelectedEvent>.OnEvent += OnChoiceSelected;
        }

        private void OnDisable()
        {
            Bus<ContinueDialogueEvent>.OnEvent -= OnContinueDialogue;
            Bus<DialogueSkipEvent>.OnEvent -= OnDialogueSkip;
            Bus<DialogueStartEvent>.OnEvent -= OnDialogueStart;
            Bus<DialogueChoiceSelectedEvent>.OnEvent -= OnChoiceSelected;
            
            CancelDialogueTasks();
            ReleaseAllHandles();
        }

        private void OnDialogueStart(DialogueStartEvent evt)
        {
            if (evt.DialogueSO == null || evt.DialogueSO.DialogueNodes.Count == 0)
            {
                Debug.LogError("Cannot start dialogue: DialogueInformationSO is null or empty.");
                return;
            }
            
            CancelDialogueTasks();
            DialogueSessionState.BeginSession();
            _dialogueCts = new CancellationTokenSource();
            StartDialogueProcessAsync(evt, _dialogueCts.Token).Forget();
        }

        private async UniTaskVoid StartDialogueProcessAsync(DialogueStartEvent evt, CancellationToken token)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            _dialogueInformation = evt.DialogueSO;
            _currentVariableContext = evt.VariableContext;
            _nodeCache.Clear();
            WARNED_EMPTY_DIALOGUE_NODE_IDS.Clear();
            WARNED_MISSING_CHARACTER_IDS.Clear();
            WARNED_MISSING_BACKGROUND_IDS.Clear();

            foreach (DialogueNode node in _dialogueInformation.DialogueNodes)
            {
                if (!string.IsNullOrEmpty(node.NodeID))
                {
                    _nodeCache[node.NodeID] = node;
                }
            }

            // 시작 노드 결정: StartNodeID가 유효하면 사용, 아니면 첫 번째 노드 사용
            if (!string.IsNullOrEmpty(_dialogueInformation.StartNodeID) && _nodeCache.ContainsKey(_dialogueInformation.StartNodeID))
            {
                _currentNodeID = _dialogueInformation.StartNodeID;
            }
            else
            {
                _currentNodeID = _dialogueInformation.DialogueNodes[0].NodeID;
            }

            _state = DialogueState.ACTIVE;
            _isAcceptingInput = false;

            await ProcessCurrentNodeAsync(token);
            EnableInputAfterFrameAsync(token).Forget();
        }

        private void OnContinueDialogue(ContinueDialogueEvent e)
        { 
            if (_state != DialogueState.ACTIVE || !_isAcceptingInput) return;
            
            DialogueNode currentNode = _nodeCache[_currentNodeID];

            // 1. 일반 선택지가 있는 경우 (입력 대기 중이므로 무시)
            if (currentNode.Choices != null && currentNode.Choices.Count > 0) return;

            // 2. NextNodeID가 존재하는 경우 다음 노드로 이동
            if (!string.IsNullOrEmpty(currentNode.NextNodeID) && _nodeCache.ContainsKey(currentNode.NextNodeID))
            {
                _currentNodeID = currentNode.NextNodeID;
                if (_dialogueCts != null)
                {
                    ProcessCurrentNodeAsync(_dialogueCts.Token).Forget();
                }
                return;
            }

            // 3. NextNodeID가 없거나 유효하지 않으면 다이알로그 종료
            EndDialogue();
        }

        private void OnChoiceSelected(DialogueChoiceSelectedEvent evt)
        {
            if (_state != DialogueState.ACTIVE) return;
            if (_dialogueCts != null)
            {
                ProcessChoiceEventsAsync(evt, _dialogueCts.Token).Forget();
            }
        }

        private async UniTaskVoid ProcessChoiceEventsAsync(DialogueChoiceSelectedEvent evt, CancellationToken token)
        {
            if (!_nodeCache.TryGetValue(_currentNodeID, out DialogueNode currentNode)
                || currentNode.Choices == null
                || evt.ChoiceIndex < 0
                || evt.ChoiceIndex >= currentNode.Choices.Count)
            {
                Debug.LogWarning($"[DialogueManager] Invalid choice index '{evt.ChoiceIndex}' at node '{_currentNodeID}'.");
                return;
            }

            DialogueChoice selectedChoice = currentNode.Choices[evt.ChoiceIndex];
            if (!EvaluateChoiceConditions(selectedChoice))
            {
                Debug.LogWarning($"[DialogueManager] Locked choice selected. Node '{_currentNodeID}', index '{evt.ChoiceIndex}'.");
                return;
            }

            if (selectedChoice.Commands != null)
            {
                foreach (IDialogueCommand command in selectedChoice.Commands)
                {
                    command.Execute();
                }
            }

            if (string.IsNullOrEmpty(selectedChoice.NextNodeID))
            {
                EndDialogue();
                return;
            }
            else
            {
                if (!_nodeCache.ContainsKey(selectedChoice.NextNodeID))
                {
                    Debug.LogError($"Dialogue Node ID '{selectedChoice.NextNodeID}' not found in database.");
                    EndDialogue();
                    return;
                }
                _currentNodeID = selectedChoice.NextNodeID;
            }

            await ProcessCurrentNodeAsync(token);
        }

        private async UniTask ProcessCurrentNodeAsync(CancellationToken token)
        {
            _isAcceptingInput = false;
            DialogueNode currentNode = _nodeCache[_currentNodeID];

            // 보이스 제어: 새 노드 시작 시 이전 보이스 중지
            Bus<StopVoiceEvent>.Raise(new StopVoiceEvent());

            // 새 보이스 재생
            if (!string.IsNullOrEmpty(currentNode.VoiceID))
            {
                Bus<PlayVoiceEvent>.Raise(new PlayVoiceEvent(currentNode.VoiceID));
            }
            
            // 리소스 로딩 시작
            if (_cachedBackgroundID != currentNode.BackgroundID)
            {
                SafeReleaseHandle(ref _backgroundHandle);

                if (dialogueDatabase != null && dialogueDatabase.TryGetBackground(currentNode.BackgroundID, out AssetReferenceSprite backgroundRef))
                {
                    _backgroundHandle = backgroundRef.LoadAssetAsync();
                }
                else
                {
                    _backgroundHandle = default;
                    if (!string.IsNullOrWhiteSpace(currentNode.BackgroundID)
                        && WARNED_MISSING_BACKGROUND_IDS.Add(currentNode.BackgroundID))
                    {
                        Debug.LogWarning($"[DialogueManager] Background ID '{currentNode.BackgroundID}' not found. Node '{currentNode.NodeID}' will render without background image.");
                    }
                }

                _cachedBackgroundID = currentNode.BackgroundID;
            }

            if (_cachedCharacterID != currentNode.CharacterID)
            {
                SafeReleaseHandle(ref _characterHandle);
                SafeReleaseHandle(ref _emotionSpriteHandle);

                if (dialogueDatabase != null && dialogueDatabase.TryGetCharacter(currentNode.CharacterID, out AssetReferenceT<CharacterInformationSO> characterRef))
                {
                    _characterHandle = characterRef.LoadAssetAsync();
                }
                else
                {
                    _characterHandle = default;
                    if (!string.IsNullOrWhiteSpace(currentNode.CharacterID)
                        && WARNED_MISSING_CHARACTER_IDS.Add(currentNode.CharacterID))
                    {
                        Debug.LogWarning($"[DialogueManager] Character ID '{currentNode.CharacterID}' not found. Node '{currentNode.NodeID}' will render dialogue text only.");
                    }
                }

                _cachedCharacterID = currentNode.CharacterID;
                _cachedEmotion = (CharacterEmotionType)(-1);
            }

            if (_backgroundHandle.IsValid() && !_backgroundHandle.IsDone)
            {
                await UniTask.WaitUntil(() => _backgroundHandle.IsDone, cancellationToken: token);
            }
            if (_characterHandle.IsValid() && !_characterHandle.IsDone)
            {
                await UniTask.WaitUntil(() => _characterHandle.IsDone, cancellationToken: token);
            }

            if (_cachedEmotion != currentNode.CharacterEmotion)
            {
                SafeReleaseHandle(ref _emotionSpriteHandle);
                
                if (_characterHandle.IsValid() && _characterHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    CharacterInformationSO charInfo = _characterHandle.Result;
                    if (charInfo.CharacterEmotions.TryGetValue(currentNode.CharacterEmotion, out AssetReferenceSprite spriteRef))
                    {
                        _emotionSpriteHandle = spriteRef.LoadAssetAsync();
                        await UniTask.WaitUntil(() => _emotionSpriteHandle.IsDone, cancellationToken: token);
                    }
                }
                _cachedEmotion = currentNode.CharacterEmotion;
            }

            // --- 변수 치환 적용 ---
            string resolvedDialogue = DialogueVariableResolver.Resolve(currentNode.DialogueDetail, _currentVariableContext);
            if (string.IsNullOrWhiteSpace(resolvedDialogue))
            {
                string nodeIdForLog = string.IsNullOrWhiteSpace(currentNode.NodeID) ? _currentNodeID : currentNode.NodeID;
                if (WARNED_EMPTY_DIALOGUE_NODE_IDS.Add(nodeIdForLog))
                {
                    Debug.LogWarning($"[DialogueManager] Empty DialogueDetail at Node '{nodeIdForLog}'.");
                }
            }

            // --- 오토 모드용 대기 시간 계산 시작 ---
            float calculatedWaitTime = 0f;
            if (!string.IsNullOrEmpty(currentNode.VoiceID))
            {
                float voiceLength = 3.0f; 
                calculatedWaitTime = voiceLength + 0.5f;
            }
            else
            {
                calculatedWaitTime = (resolvedDialogue.Length * 0.05f) + 1.0f;
            }

            bool hasChoices = currentNode.Choices != null && currentNode.Choices.Count > 0;

            // --- 독백(Monologue) 판정 및 따옴표 제거 시작 ---
            string processedDialogue = resolvedDialogue;
            bool isMonologue = false;
            string pureText = System.Text.RegularExpressions.Regex.Replace(processedDialogue, "<.*?>", string.Empty).Trim();
            
            if (pureText.Length >= 2 && pureText.StartsWith("'") && pureText.EndsWith("'"))
            {
                isMonologue = true;
                int firstQuoteIndex = processedDialogue.IndexOf('\'');
                if (firstQuoteIndex >= 0) processedDialogue = processedDialogue.Remove(firstQuoteIndex, 1);
                
                int lastQuoteIndex = processedDialogue.LastIndexOf('\'');
                if (lastQuoteIndex >= 0) processedDialogue = processedDialogue.Remove(lastQuoteIndex, 1);
            }

            string characterName = string.Empty;
            Sprite characterSprite = null;
            if (_characterHandle.IsValid() && _characterHandle.Status == AsyncOperationStatus.Succeeded && _characterHandle.Result != null)
            {
                if (!string.IsNullOrWhiteSpace(_characterHandle.Result.CharacterName))
                {
                    characterName = _characterHandle.Result.CharacterName;
                }
                else if (!string.IsNullOrWhiteSpace(currentNode.CharacterID))
                {
                    Debug.LogWarning($"[DialogueManager] Character '{currentNode.CharacterID}' has empty CharacterName. Node '{currentNode.NodeID}' will render dialogue text only.");
                }

                if (_emotionSpriteHandle.IsValid() && _emotionSpriteHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    characterSprite = _emotionSpriteHandle.Result;
                }
            }

            if (string.IsNullOrWhiteSpace(characterName))
            {
                characterSprite = null;
            }

            // UI 갱신 이벤트 발생
            DialogueProgressEvent progressEvent = new DialogueProgressEvent(
                processedDialogue,
                characterName,
                characterSprite,
                _backgroundHandle.IsValid() && _backgroundHandle.Status == AsyncOperationStatus.Succeeded ? _backgroundHandle.Result : null,
                currentNode.NameTagPosition,
                calculatedWaitTime,
                hasChoices,
                isMonologue
            );
            
            Bus<DialogueProgressEvent>.Raise(progressEvent);

            // 중요: 명령어를 리소스 로딩과 UI 갱신 이후에 실행하도록 위치 변경
            if (currentNode.Commands != null)
            {
                foreach (IDialogueCommand command in currentNode.Commands)
                {
                    command.Execute();
                }
            }

            // 선택지 처리
            if (currentNode.Choices != null && currentNode.Choices.Count > 0)
            {
                if (currentNode.Choices.Count > 1 || !string.IsNullOrEmpty(currentNode.Choices[0].ChoiceText))
                {
                    List<DialogueChoiceViewData> choiceViewDataList = new List<DialogueChoiceViewData>(currentNode.Choices.Count);
                    for (int i = 0; i < currentNode.Choices.Count; i++)
                    {
                        DialogueChoice choice = currentNode.Choices[i];
                        DialogueChoiceViewData viewData = new DialogueChoiceViewData
                        {
                            ChoiceIndex = i,
                            ChoiceText = DialogueVariableResolver.Resolve(choice.ChoiceText, _currentVariableContext),
                            IsLocked = !EvaluateChoiceConditions(choice),
                            NextNodeID = choice.NextNodeID,
                            Commands = choice.Commands
                        };

                        string resolvedSubText = DialogueVariableResolver.Resolve(choice.SubText, _currentVariableContext);
                        string resolvedLockedSubText = DialogueVariableResolver.Resolve(choice.LockedSubText, _currentVariableContext);
                        viewData.SubText = viewData.IsLocked
                            ? (string.IsNullOrWhiteSpace(resolvedLockedSubText) ? resolvedSubText : resolvedLockedSubText)
                            : resolvedSubText;

                        choiceViewDataList.Add(viewData);
                    }
                    
                    Bus<DialogueShowChoiceEvent>.Raise(new DialogueShowChoiceEvent(choiceViewDataList));
                }
                else
                {
                    _isAcceptingInput = true;
                }
            }
            else
            {
                _isAcceptingInput = true;
            }
        }

        private void OnDialogueSkip(DialogueSkipEvent e)
        { 
            if (_state != DialogueState.ACTIVE || !_isAcceptingInput) return;
            EndDialogue();
        }

        private void EndDialogue()
        {
            CancelDialogueTasks();
            Bus<StopVoiceEvent>.Raise(new StopVoiceEvent());
            ReleaseAllHandles();
            Bus<DialogueEndEvent>.Raise(new DialogueEndEvent());
            DialogueSessionState.EndSession();
            
            _currentNodeID = null;
            _dialogueInformation = null;
            _currentVariableContext = null;
            WARNED_EMPTY_DIALOGUE_NODE_IDS.Clear();
            WARNED_MISSING_CHARACTER_IDS.Clear();
            WARNED_MISSING_BACKGROUND_IDS.Clear();
            _state = DialogueState.IDLE;
            _isAcceptingInput = false;
        }

        private void CancelDialogueTasks()
        {
            if (_dialogueCts != null)
            {
                _dialogueCts.Cancel();
                _dialogueCts.Dispose();
                _dialogueCts = null;
            }
        }

        private void SafeReleaseHandle<T>(ref AsyncOperationHandle<T> handle)
        {
            if (!handle.IsValid()) return;
            if (handle.IsDone) Addressables.Release(handle);
            else
            {
                AsyncOperationHandle<T> handleToRelease = handle;
                handleToRelease.Completed += (h) => { if (h.IsValid()) Addressables.Release(h); };
            }
            handle = default;
        }

        private void ReleaseAllHandles()
        {
            SafeReleaseHandle(ref _characterHandle);
            SafeReleaseHandle(ref _backgroundHandle);
            SafeReleaseHandle(ref _emotionSpriteHandle);
            
            _cachedCharacterID = null;
            _cachedBackgroundID = null;
            _cachedEmotion = (CharacterEmotionType)(-1);
        }
        
        private async UniTaskVoid EnableInputAfterFrameAsync(CancellationToken token)
        { 
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
            if (_state == DialogueState.ACTIVE && !string.IsNullOrEmpty(_currentNodeID))
            {
                DialogueNode node = _nodeCache[_currentNodeID];
                if (node.Choices != null && node.Choices.Count > 0 && (node.Choices.Count > 1 || !string.IsNullOrEmpty(node.Choices[0].ChoiceText)))
                    return;
            }
            _isAcceptingInput = true;
        }

        /// <summary>
        /// 선택지 조건 목록을 평가하여 선택 가능 여부를 반환
        /// </summary>
        /// <param name="choice">평가할 선택지 데이터</param>
        private static bool EvaluateChoiceConditions(DialogueChoice choice)
        {
            if (choice.Conditions == null || choice.Conditions.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < choice.Conditions.Count; i++)
            {
                IDialogueCondition condition = choice.Conditions[i];
                if (condition != null && !condition.Evaluate())
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Tracks per-dialogue accumulated deltas for placeholders and runtime checks.
    /// </summary>
    public static class DialogueSessionState
    {
        private static readonly Dictionary<(MemberType memberType, StatType statType), int> STAT_DELTAS
            = new Dictionary<(MemberType memberType, StatType statType), int>();

        private static readonly Dictionary<MemberType, int> CONDITION_DELTAS
            = new Dictionary<MemberType, int>();

        private static bool _isActive;
        private static int _goldDelta;

        public static bool IsActive => _isActive;
        public static int GoldDelta => _goldDelta;

        public static void BeginSession()
        {
            _isActive = true;
            _goldDelta = 0;
            STAT_DELTAS.Clear();
            CONDITION_DELTAS.Clear();
        }

        public static void EndSession()
        {
            _isActive = false;
            _goldDelta = 0;
            STAT_DELTAS.Clear();
            CONDITION_DELTAS.Clear();
        }

        public static void AddStatDelta(MemberType memberType, StatType statType, int delta)
        {
            if (!_isActive || delta == 0)
            {
                return;
            }

            (MemberType memberType, StatType statType) key = (memberType, statType);
            if (STAT_DELTAS.TryGetValue(key, out int existing))
            {
                STAT_DELTAS[key] = existing + delta;
            }
            else
            {
                STAT_DELTAS[key] = delta;
            }
        }

        public static int GetStatDelta(MemberType memberType, StatType statType)
        {
            (MemberType memberType, StatType statType) key = (memberType, statType);
            return STAT_DELTAS.TryGetValue(key, out int value) ? value : 0;
        }

        public static IReadOnlyDictionary<(MemberType memberType, StatType statType), int> GetAllStatDeltas()
        {
            return STAT_DELTAS;
        }

        public static void AddGoldDelta(int delta)
        {
            if (!_isActive || delta == 0)
            {
                return;
            }

            _goldDelta += delta;
        }

        public static void AddConditionDelta(MemberType memberType, int delta)
        {
            if (!_isActive || delta == 0)
            {
                return;
            }

            if (CONDITION_DELTAS.TryGetValue(memberType, out int existing))
            {
                CONDITION_DELTAS[memberType] = existing + delta;
            }
            else
            {
                CONDITION_DELTAS[memberType] = delta;
            }
        }

        public static int GetConditionDelta(MemberType memberType)
        {
            return CONDITION_DELTAS.TryGetValue(memberType, out int value) ? value : 0;
        }
    }
}
