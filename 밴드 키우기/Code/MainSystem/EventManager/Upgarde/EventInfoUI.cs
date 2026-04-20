using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.BandFunds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager.Upgarde
{
    [Serializable]
    public class RankSuccessRate
    {
        public StatRankType rank;
        public int successRate;
    }

    public class EventInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI memberName;
        [SerializeField] private Image memberIcon;
        [SerializeField] private TextMeshProUGUI eventName;
        [SerializeField] private Image statIcon;
        [SerializeField] private Button downBtn;
        [SerializeField] private Button upBtn;
        [SerializeField] private Image expIcon;
        [SerializeField] private TextMeshProUGUI spendExpText;
        [SerializeField] private TextMeshProUGUI spendGoldText;
        [SerializeField] private int spendGold;
        [SerializeField] private TextMeshProUGUI successRateText;
        [SerializeField] private Button startBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private PlayEvent playEvent;

        [SerializeField] private int expPerRate = 10;
        [SerializeField] private List<RankSuccessRate> rankSuccessRates;

        [Header("스탯 아이콘")]
        [SerializeField] private Sprite lyricsIcon;
        [SerializeField] private Sprite teamworkIcon;
        [SerializeField] private Sprite proficiencyIcon;
        [SerializeField] private Sprite melodyIcon;

        private MemberType _memberType;
        private MusicRelatedStatsType _targetStat;
        private int _baseSuccessRate;
        private int _bonusRate;
        private Sprite _sprite;
        private Sprite _sadSprite;
        private Sprite _happySprite;

        private void Awake()
        {
            downBtn.onClick.AddListener(OnDownBtn);
            upBtn.onClick.AddListener(OnUpBtn);
            startBtn.onClick.AddListener(HandleStartEvent);
            cancelBtn.onClick.AddListener(HandleCancel);
            cancelBtn.gameObject.SetActive(false);
        }

        private void HandleCancel()
        {
            EventManager.Instance.ResetManager();
            cancelBtn.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void HandleStartEvent()
        {
            if (!BandSupplyManager.Instance.SpendBandFunds(spendGold))
            {
                Debug.LogWarning("[EventInfoUI] 자금 부족");
                return;
            }

            BandSupplyManager.Instance.SpendBandFunds(spendGold);

            startBtn.gameObject.SetActive(false);
            downBtn.interactable = false;
            upBtn.interactable = false;

            int totalRate = Mathf.Clamp(_baseSuccessRate + _bonusRate, 0, 100);
            Sprite targetStatIcon = GetStatIcon(_targetStat);
            playEvent.Setup(_sprite, _sadSprite, _happySprite, targetStatIcon, _targetStat, totalRate, OnEventComplete, OnShowEnd);
        }

        private void OnShowEnd()
        {
            gameObject.SetActive(false);
        }

        private void OnEventComplete(bool success, int gained)
        {
            if (success)
                BandSupplyManager.Instance.AddBandExp(gained);

            gameObject.SetActive(true);
            cancelBtn.gameObject.SetActive(true);
            startBtn.gameObject.SetActive(false);
            downBtn.interactable = true;
            upBtn.interactable = true;
        }

        private void OnDestroy()
        {
            downBtn.onClick.RemoveAllListeners();
            upBtn.onClick.RemoveAllListeners();
            startBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.RemoveAllListeners();
        }

        public void Setup(string memberNameStr, Sprite sprite, Sprite sadSprite, Sprite happySprite,
            MemberType memberType, MusicRelatedStatsType targetStat)
        {
            _memberType = memberType;
            _targetStat = targetStat;
            _bonusRate = 0;
            _sprite = sprite;
            _sadSprite = sadSprite;
            _happySprite = happySprite;

            memberName.text = memberNameStr;
            memberIcon.sprite = sprite;
            spendGoldText.SetText("도전비용 : " + spendGold);
            startBtn.gameObject.SetActive(true);
            cancelBtn.gameObject.SetActive(false);
            downBtn.interactable = true;
            upBtn.interactable = true;

            if (eventName != null)
                eventName.text = targetStat switch
                {
                    MusicRelatedStatsType.Lyrics => "가사 업그레이드 도전",
                    MusicRelatedStatsType.Teamwork => "팀워크 업그레이드 도전",
                    MusicRelatedStatsType.Proficiency => "숙련도 업그레이드 도전",
                    MusicRelatedStatsType.Melody => "멜로디 업그레이드 도전",
                    _ => targetStat.ToString()
                };

            if (statIcon != null)
                statIcon.sprite = GetStatIcon(targetStat);

            var comp = GameStatManager.Instance.GetMemberStatData(memberType, MusicRelatedStatsType.Composition);
            var inst = GameStatManager.Instance.GetMemberStatData(memberType, MusicRelatedStatsType.InstrumentProficiency);

            int compRate = GetRateByRank(comp.currentRank);
            int instRate = GetRateByRank(inst.currentRank);
            _baseSuccessRate = Mathf.Clamp((compRate + instRate) / 2, 0, 100);

            RefreshUI();
        }

        private Sprite GetStatIcon(MusicRelatedStatsType statType) => statType switch
        {
            MusicRelatedStatsType.Lyrics => lyricsIcon,
            MusicRelatedStatsType.Teamwork => teamworkIcon,
            MusicRelatedStatsType.Proficiency => proficiencyIcon,
            MusicRelatedStatsType.Melody => melodyIcon,
            _ => null
        };

        private int GetRateByRank(StatRankType rank)
        {
            var entry = rankSuccessRates.Find(r => r.rank == rank);
            return entry?.successRate ?? 0;
        }

        private void OnUpBtn()
        {
            if (!BandSupplyManager.Instance.CheckBandExp(expPerRate)) return;
            BandSupplyManager.Instance.SpendBandExp(expPerRate);
            _bonusRate++;
            RefreshUI();
        }

        private void OnDownBtn()
        {
            if (_bonusRate <= 0) return;
            BandSupplyManager.Instance.AddBandExp(expPerRate);
            _bonusRate--;
            RefreshUI();
        }

        private void RefreshUI()
        {
            int totalRate = Mathf.Clamp(_baseSuccessRate + _bonusRate, 0, 100);
            successRateText.text = $"성공률 {totalRate}%";
            spendExpText.text = $"{_bonusRate * expPerRate}";
        }
    }
}