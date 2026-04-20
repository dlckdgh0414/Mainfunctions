using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.StatSystem.BaseStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

namespace Code.MainSystem.behavior
{
    [Serializable]
    public class MemberPersonalColor
    {
        public MemberType memberType;
        public Color      color = Color.white;
    }

    public class ActivityResultUI : MonoBehaviour
    {
        [Header("결과창 패널")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Button     closeButton;
        [SerializeField] private Button     skipButton;

        [Header("멤버 퍼스널 컬러")]
        [SerializeField] private List<MemberPersonalColor> personalColors = new();

        [Header("멤버 결과 슬롯들")]
        [SerializeField] private List<MemberResultSlot> memberResultSlots;

        [Header("연출 설정")]
        [SerializeField] private float statIncreaseDelay    = 0.5f;
        [SerializeField] private float statIncreaseDuration = 1.0f;

        public event Action OnResultClosed;

        private Sequence                    _currentSequence;
        private List<MemberDataSO>          _currentMembers;
        private Dictionary<MemberType, int> _currentEarnedSnapshot;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(HandleCloseClicked);
            if (skipButton != null)
                skipButton.onClick.AddListener(HandleSkipClicked);

            Hide();
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (skipButton  != null) skipButton.onClick.RemoveAllListeners();
            _currentSequence?.Kill();
        }

        public void Show(List<MemberDataSO> members, Dictionary<MusicRelatedStatsType, int> earnedStats,
            int completionPercent, StatType targetStatType, Dictionary<MemberType, int> memberEarnedSnapshot)
        {
            _currentMembers       = members;
            _currentEarnedSnapshot = memberEarnedSnapshot;

            gameObject.SetActive(true);
            if (resultPanel  != null) resultPanel.SetActive(true);
            if (closeButton  != null) closeButton.interactable = false;
            if (skipButton   != null) skipButton.gameObject.SetActive(true);

            SetupMemberSlots(members, targetStatType, memberEarnedSnapshot);
            PlayStatIncreaseAnimation(members, memberEarnedSnapshot);
        }

        private void SetupMemberSlots(List<MemberDataSO> members, StatType targetStatType,
            Dictionary<MemberType, int> memberEarnedSnapshot)
        {
            foreach (var slot in memberResultSlots)
                if (slot != null) slot.gameObject.SetActive(false);

            for (int i = 0; i < members.Count && i < memberResultSlots.Count; i++)
            {
                var slot   = memberResultSlots[i];
                var member = members[i];
                if (slot == null || member == null) continue;

                slot.gameObject.SetActive(true);
                int earned = memberEarnedSnapshot.GetValueOrDefault(member.memberType, 0);
                slot.Setup(member, targetStatType, earned);

                var colorEntry = personalColors.Find(c => c.memberType == member.memberType);
                if (colorEntry != null)
                    slot.SetPersonalColor(colorEntry.color);
            }
        }

        private void PlayStatIncreaseAnimation(List<MemberDataSO> members,
            Dictionary<MemberType, int> memberEarnedSnapshot)
        {
            _currentSequence?.Kill();
            _currentSequence = DOTween.Sequence();
            _currentSequence.AppendInterval(statIncreaseDelay);

            for (int i = 0; i < members.Count && i < memberResultSlots.Count; i++)
            {
                var slot   = memberResultSlots[i];
                var member = members[i];
                int idx    = i;

                if (slot == null || member == null) continue;

                _currentSequence.AppendCallback(() =>
                {
                    int earned = memberEarnedSnapshot.GetValueOrDefault(members[idx].memberType, 0);
                    memberResultSlots[idx].ApplyStatIncrease(earned, statIncreaseDuration);
                });
                _currentSequence.AppendInterval(statIncreaseDuration + 0.2f);
            }

            _currentSequence.OnComplete(() =>
            {
                if (closeButton != null) closeButton.interactable = true;
                if (skipButton  != null) skipButton.gameObject.SetActive(false);
            });
        }
        
        private void HandleSkipClicked()
        {
            _currentSequence?.Kill();

            if (_currentMembers != null && _currentEarnedSnapshot != null)
            {
                for (int i = 0; i < _currentMembers.Count && i < memberResultSlots.Count; i++)
                {
                    var slot   = memberResultSlots[i];
                    var member = _currentMembers[i];
                    if (slot == null || member == null) continue;

                    int earned = _currentEarnedSnapshot.GetValueOrDefault(member.memberType, 0);
                    slot.SkipToFinal(earned);
                }
            }

            if (closeButton != null) closeButton.interactable = true;
            if (skipButton  != null) skipButton.gameObject.SetActive(false);
        }

        public void Hide()
        {
            _currentSequence?.Kill();
            if (resultPanel != null) resultPanel.SetActive(false);
            gameObject.SetActive(false);
        }

        private void HandleCloseClicked()
        {
            Hide();
            OnResultClosed?.Invoke();
        }
    }
}