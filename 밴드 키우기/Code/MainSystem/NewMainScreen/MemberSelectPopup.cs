using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    [System.Serializable]
    public struct MemberColorEntry
    {
        public MemberType memberType;
        public Color      personalColor;
        public Sprite     icon;
    }

    public class MemberSelectPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Transform  buttonParent;
        [SerializeField] private Button     memberButtonPrefab;
        [SerializeField] private Button     closeBtn;

        [Header("퍼스널 컬러")]
        [SerializeField] private List<MemberColorEntry> memberColors = new();

        private Dictionary<MemberType, MemberColorEntry> _colorMap;

        private readonly List<Button> _spawnedButtons = new();
        private MusicRelatedStatsType _targetStatType;
        private int    _effectValue;
        private string _itemName;

        private void Awake()
        {
            closeBtn.onClick.AddListener(Hide);
            popupPanel.SetActive(false);

            _colorMap = new();
            foreach (var e in memberColors)
                _colorMap[e.memberType] = e;
        }

        private void OnDestroy() => closeBtn.onClick.RemoveAllListeners();

        public void Show(List<MemberDataSO> members, MusicRelatedStatsType targetStatType, int effectValue, string itemName)
        {
            _targetStatType = targetStatType;
            _effectValue    = effectValue;
            _itemName       = itemName;

            ClearButtons();

            foreach (var member in members)
            {
                var btn  = Instantiate(memberButtonPrefab, buttonParent);
                var view = btn.GetComponent<MemberButtonView>();

                if (view != null)
                {
                    var icon  = member.IconSprite;
                    var color = Color.white;

                    if (_colorMap.TryGetValue(member.memberType, out var entry))
                    {
                        color = entry.personalColor;
                        if (entry.icon != null) icon = entry.icon;
                    }

                    view.Setup(member.memberName, icon, color);
                }

                var captured = member;
                btn.onClick.AddListener(() => HandleMemberSelected(captured));
                _spawnedButtons.Add(btn);
            }

            popupPanel.SetActive(true);
        }

        public void Hide()
        {
            popupPanel.SetActive(false);
            ClearButtons();
        }

        private void HandleMemberSelected(MemberDataSO member)
        {
            Debug.Log($"[MemberSelectPopup] {member.memberName} 선택 → {_targetStatType} +{_effectValue}");

            GameStatManager.Instance?.AddMemberStatDirect(
                member.memberType,
                _targetStatType,
                _effectValue);

            Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(
                SystemMessageIconType.Warning,
                $"{_itemName} 구매 완료! ({member.memberName})"));

            Hide();
        }

        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
                if (btn != null) Destroy(btn.gameObject);
            _spawnedButtons.Clear();
        }
    }
}