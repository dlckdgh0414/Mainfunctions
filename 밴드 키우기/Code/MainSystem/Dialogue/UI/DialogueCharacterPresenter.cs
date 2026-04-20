using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;
using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
using Code.Core.Bus.GameEvents.DialogueEvents.UI;
using Member.LS.Code.Dialogue;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// 좌/우 다중 캐릭터 렌더링 및 포커싱, 애니메이션 연출을 담당하는 Presenter
    /// </summary>
    public class DialogueCharacterPresenter : MonoBehaviour
    {
        [Serializable]
        public class CharacterSlot
        {
            public RectTransform Root;
            public Image Image;
            public CanvasGroup Group;
            [HideInInspector] public float OriginalY; // Y 좌표만 별도로 관리
            [HideInInspector] public float OriginalX;
        }

        [Header("Character Slots")]
        [SerializeField] private CharacterSlot leftSlot;
        [SerializeField] private CharacterSlot rightSlot;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float moveOffset = 50f;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Speech Motion Settings")]
        [SerializeField] private float jumpAmount = 30f; // 기본값 약간 상승
        [SerializeField] private float jumpDuration = 0.2f;

        private CharacterSlot _activeSlot;

        private void Awake()
        {
            // 초기 위치 저장
            if (leftSlot.Root != null)
            {
                leftSlot.OriginalY = leftSlot.Root.anchoredPosition.y;
                leftSlot.OriginalX = leftSlot.Root.anchoredPosition.x;
            }
            if (rightSlot.Root != null)
            {
                rightSlot.OriginalY = rightSlot.Root.anchoredPosition.y;
                rightSlot.OriginalX = rightSlot.Root.anchoredPosition.x;
            }
            
            ResetSlot(leftSlot);
            ResetSlot(rightSlot);
        }

        private void OnEnable()
        {
            Bus<DialogueProgressEvent>.OnEvent += OnDialogueProgress;
            Bus<ClearCharacterEvent>.OnEvent += OnClearCharacter;
            Bus<DialogueEndEvent>.OnEvent += OnDialogueEnd;
            Bus<CharacterEffectEvent>.OnEvent += OnCharacterEffect;
            Bus<DialogueTypingFinishedEvent>.OnEvent += OnTypingFinished;
        }

        private void OnDisable()
        {
            Bus<DialogueProgressEvent>.OnEvent -= OnDialogueProgress;
            Bus<ClearCharacterEvent>.OnEvent -= OnClearCharacter;
            Bus<DialogueEndEvent>.OnEvent -= OnDialogueEnd;
            Bus<CharacterEffectEvent>.OnEvent -= OnCharacterEffect;
            Bus<DialogueTypingFinishedEvent>.OnEvent -= OnTypingFinished;
        }

        private void OnDialogueProgress(DialogueProgressEvent evt)
        {
            bool isLeft = evt.NameTagPosition == NameTagPositionType.Left;
            CharacterSlot currentSlot = isLeft ? leftSlot : rightSlot;
            CharacterSlot otherSlot = isLeft ? rightSlot : leftSlot;

            if (evt.CharacterSprite == null)
            {
                _activeSlot = null;
                FadeOutSlot(leftSlot);
                FadeOutSlot(rightSlot);
                return;
            }

            _activeSlot = currentSlot;

            // 1. 활성 슬롯 연출 (이미지 유무와 상관없이 점프)
            if (currentSlot.Root != null)
            {
                currentSlot.Root.DOKill();
                
                // 캐릭터 이미지 처리
                if (currentSlot.Image.sprite != evt.CharacterSprite)
                {
                    currentSlot.Image.sprite = evt.CharacterSprite;
                    currentSlot.Image.preserveAspect = true;

                    currentSlot.Group.DOKill();
                    if (currentSlot.Group.alpha < 0.1f)
                    {
                        float startX = currentSlot.OriginalX + (isLeft ? -moveOffset : moveOffset);
                        currentSlot.Root.anchoredPosition = new Vector2(startX, currentSlot.OriginalY);
                        currentSlot.Root.DOAnchorPosX(currentSlot.OriginalX, fadeDuration).SetEase(Ease.OutCubic);
                        // TODO: 빌드 환경의 스프라이트 알파/압축 설정 점검 후 캐릭터 알파 페이드 복구 필요
                        // currentSlot.Group.DOFade(1f, fadeDuration);
                        currentSlot.Group.alpha = 1f;
                    }
                }

                currentSlot.Image.DOKill();
                currentSlot.Image.DOColor(activeColor, fadeDuration);

                // 발화 시작: 무조건 점프 업
                currentSlot.Root.DOAnchorPosY(currentSlot.OriginalY + jumpAmount, jumpDuration).SetEase(Ease.OutQuad);
            }

            // 2. 비활성 슬롯 처리 (복귀 및 Dim)
            if (otherSlot.Root != null && otherSlot.Group.alpha > 0.1f)
            {
                otherSlot.Image.DOKill();
                otherSlot.Image.DOColor(inactiveColor, fadeDuration);
                
                otherSlot.Root.DOKill();
                otherSlot.Root.DOAnchorPosY(otherSlot.OriginalY, jumpDuration).SetEase(Ease.InQuad);
            }
        }

        private void OnTypingFinished(DialogueTypingFinishedEvent evt)
        {
            // 타이핑이 끝나면 현재 활성 캐릭터를 다시 제자리로
            if (_activeSlot != null && _activeSlot.Root != null && _activeSlot.Group.alpha > 0.1f)
            {
                _activeSlot.Root.DOKill();
                _activeSlot.Root.DOAnchorPosY(_activeSlot.OriginalY, jumpDuration).SetEase(Ease.InQuad);
            }
        }

        private void OnCharacterEffect(CharacterEffectEvent evt)
        {
            bool isLeft = evt.Position == NameTagPositionType.Left;
            CharacterSlot targetSlot = isLeft ? leftSlot : rightSlot;

            if (targetSlot.Root == null || targetSlot.Image.sprite == null) return;

            // 기존 애니메이션 중단 및 위치 초기화
            targetSlot.Root.DOKill();
            targetSlot.Root.anchoredPosition = new Vector2(targetSlot.OriginalX, targetSlot.OriginalY);

            switch (evt.EffectType)
            {
                case CharacterEffectType.Bounce:
                    targetSlot.Root.DOPunchAnchorPos(Vector2.up * evt.Intensity, evt.Duration, evt.Count, 0.5f);
                    break;

                case CharacterEffectType.Jump:
                    targetSlot.Root.DOJumpAnchorPos(new Vector2(targetSlot.OriginalX, targetSlot.OriginalY), evt.Intensity, evt.Count, evt.Duration);
                    break;

                case CharacterEffectType.Shake:
                    targetSlot.Root.DOShakeAnchorPos(evt.Duration, evt.Intensity, 10, 90, false, true);
                    break;

                case CharacterEffectType.Excited:
                    Sequence seq = DOTween.Sequence().SetTarget(targetSlot.Root);
                    
                    // 1. 점프 (Y축)
                    seq.Join(targetSlot.Root.DOJumpAnchorPos(new Vector2(targetSlot.OriginalX, targetSlot.OriginalY), evt.Intensity, evt.Count, evt.Duration));

                    // 2. 좌우 왕복 (X축)
                    float segmentDuration = evt.Duration / evt.Count;
                    Sequence moveSeq = DOTween.Sequence();
                    for (int i = 0; i < evt.Count; i++)
                    {
                        float direction = (i % 2 == 0) ? 1 : -1;
                        moveSeq.Append(targetSlot.Root.DOAnchorPosX(targetSlot.OriginalX + (evt.Distance * direction), segmentDuration / 2).SetEase(Ease.OutQuad));
                        moveSeq.Append(targetSlot.Root.DOAnchorPosX(targetSlot.OriginalX, segmentDuration / 2).SetEase(Ease.InQuad));
                    }
                    seq.Join(moveSeq);

                    seq.OnComplete(() => targetSlot.Root.anchoredPosition = new Vector2(targetSlot.OriginalX, targetSlot.OriginalY));
                    seq.Play();
                    break;
            }
        }

        private void OnClearCharacter(ClearCharacterEvent evt)
        {
            CharacterSlot target = (evt.Position == NameTagPositionType.Left) ? leftSlot : rightSlot;
            FadeOutSlot(target);
        }

        private void OnDialogueEnd(DialogueEndEvent e)
        {
            FadeOutSlot(leftSlot);
            FadeOutSlot(rightSlot);
            _activeSlot = null;
        }

        private void FadeOutSlot(CharacterSlot slot)
        {
            if (slot == null || slot.Root == null) return;

            slot.Root.DOKill();
            slot.Group.DOKill();
            
            slot.Root.DOAnchorPosY(slot.OriginalY, fadeDuration);
            // TODO: 빌드 환경의 스프라이트 알파/압축 설정 점검 후 캐릭터 알파 페이드 복구 필요
            // slot.Group.DOFade(0f, fadeDuration).OnComplete(() => {
            //     slot.Image.sprite = null;
            // });
            slot.Group.alpha = 0f;
            slot.Image.sprite = null;
        }

        private void ResetSlot(CharacterSlot slot)
        {
            if (slot == null || slot.Root == null) return;

            slot.Group.alpha = 0f;
            slot.Image.sprite = null;
            slot.Image.color = activeColor;
            slot.Root.anchoredPosition = new Vector2(slot.OriginalX, slot.OriginalY);
        }
    }
}
