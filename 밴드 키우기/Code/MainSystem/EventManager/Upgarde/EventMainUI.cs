using System;
using System.Collections.Generic;
using Code.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.MainSystem.EventManager.Upgarde
{
    [Serializable]
    public class MemberIconData
    {
        public Sprite sprite;
        public Sprite sadSprite;
        public Sprite happySprite;
        public MemberType memberType;
        public string name;
    }

    public class EventMainUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private EventStartUI eventStartUI;
        [SerializeField] private MemberStatInfoUI memberStatInfo;
        [SerializeField] private EventInfoUI eventInfo;
        [SerializeField] private List<MemberIconData> memberStats;

        private static readonly MusicRelatedStatsType[] ValidEventStats =
        {
            MusicRelatedStatsType.Lyrics,
            MusicRelatedStatsType.Teamwork,
            MusicRelatedStatsType.Proficiency,
            MusicRelatedStatsType.Melody,
        };

        private MemberType _memberType;
        private Sprite _memberSprite;
        private Sprite _sadSprite;
        private Sprite _happySprite;
        private string _memberName;
        private MusicRelatedStatsType _targetStat;

        private void Awake()
        {
            eventStartUI.OnNextBtnClick += HandleNextUI;
            memberStatInfo.OnStartEvent += HandleStartUI;
            memberStatInfo.OnCancelEvent += HandleCancelUI;

            eventStartUI.gameObject.SetActive(false);
            memberStatInfo.gameObject.SetActive(false);
            eventInfo.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            eventStartUI.OnNextBtnClick -= HandleNextUI;
            memberStatInfo.OnStartEvent -= HandleStartUI;
            memberStatInfo.OnCancelEvent -= HandleCancelUI;
        }

        private void HandleCancelUI()
        {
            memberStatInfo.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void HandleStartUI()
        {
            memberStatInfo.gameObject.SetActive(false);
            eventInfo.gameObject.SetActive(true);
            eventInfo.Setup(_memberName, _memberSprite, _sadSprite, _happySprite, _memberType, _targetStat);
        }

        private void HandleNextUI()
        {
            eventStartUI.gameObject.SetActive(false);
            memberStatInfo.gameObject.SetActive(true);
            memberStatInfo.Setup(_memberSprite, _memberType, _targetStat);
        }

        public void SetUPUI()
        {
            gameObject.SetActive(true);

            var values = System.Enum.GetValues(typeof(MemberType));
            _memberType = (MemberType)values.GetValue(Random.Range(0, values.Length));
            foreach (MemberIconData data in memberStats)
            {
                if (data.memberType == _memberType)
                {
                    _memberSprite = data.sprite;
                    _memberName = data.name;
                    _sadSprite = data.sadSprite;
                    _happySprite = data.happySprite;
                }
            }

            _targetStat = ValidEventStats[Random.Range(0, ValidEventStats.Length)];

            eventStartUI.gameObject.SetActive(true);
            eventStartUI.Setup(_memberSprite);
        }
    }
}