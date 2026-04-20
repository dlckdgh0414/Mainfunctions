using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicRelated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

namespace Code.MainSystem.behavior
{
    public class BehaviorUI : MonoBehaviour
    {
        [Header("멤버 이미지 (미리 배치)")]
        [SerializeField] private List<BehaviorMember> allMemberImages;

        [Header("회차 텍스트")]
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI bottomText;

        [Header("헤더 텍스트")]
        [SerializeField] private TextMeshProUGUI headerText;

        [Header("음악 완성도 게이지")]
        [SerializeField] private Image           musicCompletionGauge;
        [SerializeField] private TextMeshProUGUI musicCompletionText;

        [Header("레이더 차트 스탯")]
        [SerializeField] private TextMeshProUGUI lyricsText;
        [SerializeField] private TextMeshProUGUI teamworkText;
        [SerializeField] private TextMeshProUGUI proficiencyText;
        [SerializeField] private TextMeshProUGUI melodyText;

        [Header("발사 위치")]
        [SerializeField] private Transform fixedDropPoint;

        [Header("레이더 차트 타겟 위치")]
        [SerializeField] private Transform lyricsTarget;
        [SerializeField] private Transform teamworkTarget;
        [SerializeField] private Transform proficiencyTarget;
        [SerializeField] private Transform melodyTarget;

        [Header("애니메이션 설정")]
        [SerializeField] private float gaugeFillDuration = 0.5f;
        [SerializeField] private float textPunchScale    = 0.3f;
        [SerializeField] private float jumpHeight        = 40f;
        [SerializeField] private float jumpDuration      = 0.25f;

        [SerializeField] private Button closeBtn;

        private readonly Dictionary<MusicRelatedStatsType, TextMeshProUGUI> _statTexts         = new();
        private readonly Dictionary<MusicRelatedStatsType, Transform>        _statTargets        = new();
        private readonly Dictionary<MusicRelatedStatsType, Color>            _statOriginalColors = new();

        private int   _currentMusicPercent = 0;
        private Color _musicPercentOriginalColor;

        private void Awake()
        {
            InitializeStatMappings();
            HideAllMembers();

            if (musicCompletionText != null)
                _musicPercentOriginalColor = musicCompletionText.color;
        }

        private void OnEnable()
        {
            if (GameStatManager.Instance != null)
                GameStatManager.Instance.OnStatsChanged += UpdateMusicCompletionGauge;
            if (closeBtn != null) closeBtn.interactable = false;
        }

        private void OnDisable()
        {
            if (GameStatManager.Instance != null)
                GameStatManager.Instance.OnStatsChanged -= UpdateMusicCompletionGauge;

            if (musicCompletionGauge != null) musicCompletionGauge.DOKill();
            if (musicCompletionText  != null) musicCompletionText.DOKill();
        }

        private void OnDestroy()
        {
            if (musicCompletionGauge != null) musicCompletionGauge.DOKill();
            if (musicCompletionText  != null) musicCompletionText.DOKill();
        }

        private void InitializeStatMappings()
        {
            _statTexts[MusicRelatedStatsType.Lyrics]      = lyricsText;
            _statTexts[MusicRelatedStatsType.Teamwork]    = teamworkText;
            _statTexts[MusicRelatedStatsType.Proficiency] = proficiencyText;
            _statTexts[MusicRelatedStatsType.Melody]      = melodyText;

            _statTargets[MusicRelatedStatsType.Lyrics]      = lyricsTarget;
            _statTargets[MusicRelatedStatsType.Teamwork]    = teamworkTarget;
            _statTargets[MusicRelatedStatsType.Proficiency] = proficiencyTarget;
            _statTargets[MusicRelatedStatsType.Melody]      = melodyTarget;

            if (lyricsText      != null) _statOriginalColors[MusicRelatedStatsType.Lyrics]      = lyricsText.color;
            if (teamworkText    != null) _statOriginalColors[MusicRelatedStatsType.Teamwork]    = teamworkText.color;
            if (proficiencyText != null) _statOriginalColors[MusicRelatedStatsType.Proficiency] = proficiencyText.color;
            if (melodyText      != null) _statOriginalColors[MusicRelatedStatsType.Melody]      = melodyText.color;
        }

        private void HideAllMembers()
        {
            foreach (var member in allMemberImages)
                if (member.skeletonGraphic != null)
                    member.skeletonGraphic.gameObject.SetActive(false);
        }

        public void SetupMembers(List<MemberType> participatingMembers)
        {
            HideAllMembers();
            foreach (var memberType in participatingMembers)
            {
                var memberUI = allMemberImages.Find(m => m.type == memberType);
                if (memberUI?.skeletonGraphic != null)
                    memberUI.skeletonGraphic.gameObject.SetActive(true);
            }
        }

        // 멤버 점프 연출
        public void PlayMemberJump(MemberType memberType)
        {
            var memberUI = allMemberImages.Find(m => m.type == memberType);
            if (memberUI?.skeletonGraphic == null) return;

            var rect = memberUI.skeletonGraphic.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.DOKill();
            var originPos = rect.anchoredPosition;
            rect.DOAnchorPosY(originPos.y + jumpHeight, jumpDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    rect.DOAnchorPosY(originPos.y, jumpDuration)
                        .SetEase(Ease.InQuad);
                });
        }

        // 멤버별 스폰 포인트 반환
        public Transform GetMemberSpawnPoint(MemberType memberType)
        {
            var memberUI = allMemberImages.Find(m => m.type == memberType);
            if (memberUI?.spawnPoint != null) return memberUI.spawnPoint;
            // 스폰 포인트 없으면 fixedDropPoint 폴백
            return fixedDropPoint;
        }

        public void SetHeaderText(string text)
        {
            if (headerText != null) headerText.text = text;
        }

        public void SetRoundText(int round)
        {
            if (roundText != null) roundText.text = $"{round}회차";
        }

        public void SetBottomText(string text)
        {
            if (bottomText != null) bottomText.text = text;
        }

        public void RefreshAllStats()
        {
            if (GameStatManager.Instance == null) return;
            if (lyricsText      != null) lyricsText.text      = GameStatManager.Instance.GetScore(MusicRelatedStatsType.Lyrics).ToString();
            if (teamworkText    != null) teamworkText.text     = GameStatManager.Instance.GetScore(MusicRelatedStatsType.Teamwork).ToString();
            if (proficiencyText != null) proficiencyText.text  = GameStatManager.Instance.GetScore(MusicRelatedStatsType.Proficiency).ToString();
            if (melodyText      != null) melodyText.text       = GameStatManager.Instance.GetScore(MusicRelatedStatsType.Melody).ToString();

            _currentMusicPercent = GameStatManager.Instance.GetMusicPerfectionPercent();
            if (musicCompletionText  != null) musicCompletionText.text    = $"{_currentMusicPercent}%";
            if (musicCompletionGauge != null) musicCompletionGauge.fillAmount = _currentMusicPercent / 100f;
        }

        public void ResetAllStats()
        {
            if (lyricsText      != null) lyricsText.text      = "0";
            if (teamworkText    != null) teamworkText.text     = "0";
            if (proficiencyText != null) proficiencyText.text  = "0";
            if (melodyText      != null) melodyText.text       = "0";

            _currentMusicPercent = 0;
            if (musicCompletionText  != null) musicCompletionText.text    = "0%";
            if (musicCompletionGauge != null) musicCompletionGauge.fillAmount = 0f;
        }

        public void UpdateStatValue(MusicRelatedStatsType statType, int value)
        {
            if (!_statTexts.TryGetValue(statType, out var textComponent) || textComponent == null) return;

            textComponent.text = value.ToString();
            textComponent.DOKill();

            Color originalColor = _statOriginalColors.GetValueOrDefault(statType, Color.black);
            Sequence colorSeq = DOTween.Sequence();
            colorSeq.Append(textComponent.DOColor(Color.yellow, 0.1f));
            colorSeq.Append(textComponent.DOColor(originalColor, 0.2f));

            textComponent.transform.DOKill();
            textComponent.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f);
        }

        private void UpdateMusicCompletionGauge()
        {
            if (GameStatManager.Instance == null) return;

            int newPercent = GameStatManager.Instance.GetMusicPerfectionPercent();
            if (newPercent == _currentMusicPercent) return;

            _currentMusicPercent = newPercent;

            if (musicCompletionGauge != null)
            {
                musicCompletionGauge.DOKill();
                musicCompletionGauge.DOFillAmount(_currentMusicPercent / 100f, gaugeFillDuration).SetEase(Ease.OutQuad);
            }

            if (musicCompletionText != null)
            {
                musicCompletionText.DOKill();
                musicCompletionText.text = $"{_currentMusicPercent}%";

                Sequence colorSeq = DOTween.Sequence();
                colorSeq.Append(musicCompletionText.DOColor(Color.yellow, 0.1f));
                colorSeq.Append(musicCompletionText.DOColor(_musicPercentOriginalColor, 0.3f));
                musicCompletionText.transform.DOPunchScale(Vector3.one * textPunchScale, 0.4f, 1, 0.5f);
            }
        }

        public Transform GetFixedDropPoint() => fixedDropPoint;

        public Transform GetTargetTransform(MusicRelatedStatsType statType)
        {
            if (!_statTargets.TryGetValue(statType, out var target) || target == null)
            {
                Debug.LogError($"[BehaviorUI] Target Transform for {statType}가 null입니다!");
                return null;
            }
            return target;
        }

        public Vector2 GetTargetPosition(MusicRelatedStatsType statType)
        {
            if (!_statTargets.TryGetValue(statType, out var target) || target == null)
                return Vector2.zero;

            var targetRect = target as RectTransform;
            var canvas     = GetComponentInParent<Canvas>();
            var canvasRect = canvas?.GetComponent<RectTransform>();

            if (targetRect != null && canvasRect != null)
                return canvasRect.InverseTransformPoint(targetRect.position);

            return Vector2.zero;
        }
    }
}