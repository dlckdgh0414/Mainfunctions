using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 다이알로그 텍스트 출력 및 타이핑 효과, 진행 상태 관리를 담당하는 Presenter
    /// (효과 처리는 Bus를 통해 다른 시스템에 전달)
    /// </summary>
    public class DialogueTextPresenter : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject nameTag;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.03f;
        [SerializeField] private float startDelay = 0.2f;

        private CancellationTokenSource _typingCts;
        private bool _isTyping = false;
        private DialogueProgressEvent _currentEvent;
        private List<TextEffectData> _currentEffects = new List<TextEffectData>();
        
        // 오토 모드 관련 변수
        private bool _isAutoMode = false;
        private CancellationTokenSource _autoTimerCts;

        private void OnEnable()
        {
            Bus<DialogueProgressEvent>.OnEvent += OnDialogueProgress;
            Bus<DialogueEndEvent>.OnEvent += OnDialogueEnd;
            Bus<UIContinueButtonPressedEvent>.OnEvent += OnUiContinueButtonPressed;
            Bus<ToggleAutoModeEvent>.OnEvent += OnToggleAutoMode;
        }

        private void OnDisable()
        {
            Bus<DialogueProgressEvent>.OnEvent -= OnDialogueProgress;
            Bus<DialogueEndEvent>.OnEvent -= OnDialogueEnd;
            Bus<UIContinueButtonPressedEvent>.OnEvent -= OnUiContinueButtonPressed;
            Bus<ToggleAutoModeEvent>.OnEvent -= OnToggleAutoMode;
        }

        private void OnDialogueProgress(DialogueProgressEvent evt)
        {
            _currentEvent = evt;

            CancelAutoTimer();

            if (!string.IsNullOrEmpty(evt.CharacterName))
            {
                nameTag.SetActive(true);
                nameText.text = evt.CharacterName;
            }
            else
            {
                nameTag.SetActive(false);
            }

            // 파서를 통해 텍스트와 효과 분리
            (string plainText, List<TextEffectData> effects) = DialogueTextParser.Parse(evt.DialogueDetail);
            _currentEffects = effects;
            dialogueText.text = plainText;
            dialogueText.maxVisibleCharacters = 0;

            // Bus를 통해 효과 전달 (관심 있는 Processor들이 수신)
            Bus<TextEffectEvent>.Raise(new TextEffectEvent(effects));

            CancelTyping();
            _typingCts = new CancellationTokenSource();
            
            TypeDialogueAsync(_typingCts.Token).Forget();
        }

        private void OnUiContinueButtonPressed(UIContinueButtonPressedEvent e)
        {
            if (_isTyping)
            {
                CancelTyping();
                dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
                _isTyping = false;

                // 타이핑 스킵 시에도 완료 이벤트 발행
                Bus<DialogueTypingFinishedEvent>.Raise(new DialogueTypingFinishedEvent());

                if (_isAutoMode && !_currentEvent.HasChoices)
                {
                    StartAutoTimer();
                }
            }
            else
            {
                Bus<ContinueDialogueEvent>.Raise(new ContinueDialogueEvent());
            }
        }

        private void OnDialogueEnd(DialogueEndEvent e)
        {
            CancelTyping();
            CancelAutoTimer();
            
            _isTyping = false;
            dialogueText.text = "";
            Bus<TextEffectEvent>.Raise(new TextEffectEvent(new List<TextEffectData>()));
            // 다이알로그 종료 시에도 캐릭터 복귀를 위해 이벤트 발행
            Bus<DialogueTypingFinishedEvent>.Raise(new DialogueTypingFinishedEvent());
        }

        private void OnToggleAutoMode(ToggleAutoModeEvent e)
        {
            _isAutoMode = e.IsAuto;
        }

        private async UniTaskVoid TypeDialogueAsync(CancellationToken token)
        {
            _isTyping = true;
            
            if (startDelay > 0)
            {
                bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: token).SuppressCancellationThrow();
                if (isCancelled) return;
            }

            dialogueText.ForceMeshUpdate();
            int totalCharacters = dialogueText.textInfo.characterCount;

            for (int i = 0; i <= totalCharacters; i++)
            {
                if (token.IsCancellationRequested) return;

                dialogueText.maxVisibleCharacters = i;
                
                if (i < totalCharacters)
                {
                    float currentDelay = GetCurrentTypingSpeed(i);
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelay), cancellationToken: token);
                }
            }
            
            _isTyping = false;
            _typingCts = null;

            // 타이핑 정상 완료 시 이벤트 발행
            Bus<DialogueTypingFinishedEvent>.Raise(new DialogueTypingFinishedEvent());

            if (_isAutoMode && !_currentEvent.HasChoices)
            {
                StartAutoTimer();
            }
        }

        private float GetCurrentTypingSpeed(int characterIndex)
        {
            int stringIndex = dialogueText.textInfo.characterInfo[characterIndex].index;
            
            foreach (TextEffectData effect in _currentEffects)
            {
                if (effect.Type == TextEffectType.Speed && stringIndex >= effect.StartIndex && stringIndex < effect.EndIndex)
                {
                    return effect.Value;
                }
            }
            return typingSpeed;
        }

        private void StartAutoTimer()
        {
            CancelAutoTimer();
            _autoTimerCts = new CancellationTokenSource();
            AutoProceedTimerAsync(1.5f, _autoTimerCts.Token).Forget();
        }

        private async UniTaskVoid AutoProceedTimerAsync(float waitTime, CancellationToken token)
        {
            bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            Bus<ContinueDialogueEvent>.Raise(new ContinueDialogueEvent());
            _autoTimerCts = null;
        }

        private void CancelTyping()
        {
            if (_typingCts != null)
            {
                _typingCts.Cancel();
                _typingCts.Dispose();
                _typingCts = null;
            }
        }

        private void CancelAutoTimer()
        {
            if (_autoTimerCts != null)
            {
                _autoTimerCts.Cancel();
                _autoTimerCts.Dispose();
                _autoTimerCts = null;
            }
        }
    }
}
