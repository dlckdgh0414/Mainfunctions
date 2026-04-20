using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.MusicRelated;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager.Upgarde
{
    [Serializable]
    public class MemberStatInfoData
    {
        public Image statIcon;
        public Image rankIcon;
        public Sprite statIconSprite;
        public MusicRelatedStatsType statType;
        public GameObject highlightObject; 
    }

    public class MemberStatInfoUI : MonoBehaviour
    {
        [SerializeField] private Image memberIcon;
        [SerializeField] private List<MemberStatInfoData> memberStatInfoList;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private Button startBtn;

        public event Action OnStartEvent;
        public event Action OnCancelEvent;

        private void Awake()
        {
            cancelBtn.onClick.AddListener(() => OnCancelEvent?.Invoke());
            startBtn.onClick.AddListener(() => OnStartEvent?.Invoke());
        }

        public void Setup(Sprite sprite, MemberType memberType, MusicRelatedStatsType targetStat)
        {
            memberIcon.sprite = sprite;

            foreach (var data in memberStatInfoList)
            {
                var statData = GameStatManager.Instance.GetMemberStatData(memberType, data.statType);
                if (statData == null) continue;

                var rankIcon = GameStatManager.Instance.GetRankIcon(statData.currentRank);
                if (rankIcon != null)
                    data.rankIcon.sprite = rankIcon;
                data.statIcon.sprite = data.statIconSprite;
                
                if (data.highlightObject != null)
                    data.highlightObject.SetActive(data.statType == targetStat);
            }
        }
    }
}