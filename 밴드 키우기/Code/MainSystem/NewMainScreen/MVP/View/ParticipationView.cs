using System;
using System.Collections.Generic;
using Code.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    [Serializable]
    public class MemberButtonEntry
    {
        public MemberType memberType;
        public Button     memberBtn;
        public Image      memberIconImage;
        public Image      memberConditionImage;
        public GameObject parentObject;
        public Color      memberColor = Color.white;
    }

    public class ParticipationView : MonoBehaviour, IParticipationView
    {
        [Header("고정 슬롯 (인스펙터에서 미리 배치)")]
        [SerializeField] private List<ParticipationSlot> topSlots;
        [SerializeField] private List<ParticipationSlot> bottomSlots;

        [Header("멤버 버튼")]
        [SerializeField] private List<MemberButtonEntry> memberButtonEntries;

        [Header("드래그 설정")]
        [SerializeField] private Canvas rootCanvas;

        public event Action<MemberType> OnMemberClicked;
        public event Action<MemberType> OnSlotCancelClicked;
        public event Action<MemberType, bool> OnMemberDroppedToSlot;

        private readonly Dictionary<MemberType, ParticipationSlot> _occupiedSlots  = new();
        private readonly Dictionary<MemberType, Action>            _cancelHandlers = new();

        private Image _ghostImage;
        private RectTransform _ghostRect;
        private int _draggingIndex = -1;

        private void Awake()
        {
            foreach (var s in topSlots)    s.Clear();
            foreach (var s in bottomSlots) s.Clear();

            SetupDropZones();
            SetupDragHandlers();
            CreateGhostImage();
        }

        private void SetupDropZones()
        {
            for (int i = 0; i < topSlots.Count; i++)
            {
                var zone = topSlots[i].gameObject.GetComponent<ParticipationDropZone>()
                           ?? topSlots[i].gameObject.AddComponent<ParticipationDropZone>();
                zone.IsTopSlot = true;
                zone.SlotIndex = i;
            }
            for (int i = 0; i < bottomSlots.Count; i++)
            {
                var zone = bottomSlots[i].gameObject.GetComponent<ParticipationDropZone>()
                           ?? bottomSlots[i].gameObject.AddComponent<ParticipationDropZone>();
                zone.IsTopSlot = false;
                zone.SlotIndex = i;
            }
        }

        private void SetupDragHandlers()
        {
            for (int i = 0; i < memberButtonEntries.Count; i++)
            {
                var entry = memberButtonEntries[i];
                int idx = i;

                var handler = entry.memberBtn.gameObject.GetComponent<MemberDragHandler>()
                              ?? entry.memberBtn.gameObject.AddComponent<MemberDragHandler>();

                handler.MemberIndex = idx;
                handler.OnDragBegin += HandleDragBegin;
                handler.OnDragging  += HandleDragging;
                handler.OnDragEnd   += HandleDragEnd;
                handler.OnClicked   += HandleMemberClickedByDrag;

                if (entry.memberBtn.gameObject.GetComponent<CanvasGroup>() == null)
                    entry.memberBtn.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void CreateGhostImage()
        {
            if (rootCanvas == null)
                rootCanvas = GetComponentInParent<Canvas>();

            var go = new GameObject("ParticipationGhost", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(rootCanvas.transform, false);
            _ghostImage = go.GetComponent<Image>();
            _ghostImage.raycastTarget = false;
            _ghostImage.gameObject.SetActive(false);
            _ghostRect = go.GetComponent<RectTransform>();
            _ghostRect.sizeDelta = new Vector2(80, 80);
        }

        private void HandleMemberClickedByDrag(int index)
        {
            if (index < 0 || index >= memberButtonEntries.Count) return;
            OnMemberClicked?.Invoke(memberButtonEntries[index].memberType);
        }

        private void HandleDragBegin(int index)
        {
            if (index < 0 || index >= memberButtonEntries.Count) return;
            _draggingIndex = index;

            var entry = memberButtonEntries[index];
            _ghostImage.sprite = entry.memberIconImage.sprite;
            _ghostImage.gameObject.SetActive(true);
            _ghostImage.transform.SetAsLastSibling();

            var cg = entry.memberBtn.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0.4f; }
        }

        private void HandleDragging(PointerEventData eventData)
        {
            if (_ghostRect == null) return;

            Camera cam = (rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                ? null : rootCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.GetComponent<RectTransform>(), eventData.position, cam, out Vector2 localPoint))
            {
                _ghostRect.anchoredPosition = localPoint;
            }
        }

        private void HandleDragEnd(int index, PointerEventData eventData)
        {
            _ghostImage.gameObject.SetActive(false);

            if (index < 0 || index >= memberButtonEntries.Count)
            {
                _draggingIndex = -1;
                return;
            }

            var entry = memberButtonEntries[index];
            var cg = entry.memberBtn.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; }

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            bool dropped = false;
            foreach (var hit in results)
            {
                var zone = hit.gameObject.GetComponentInParent<ParticipationDropZone>();
                if (zone != null)
                {
                    OnMemberDroppedToSlot?.Invoke(entry.memberType, zone.IsTopSlot);
                    dropped = true;
                    break;
                }
            }

            if (!dropped)
            {
                OnMemberClicked?.Invoke(entry.memberType);
            }

            _draggingIndex = -1;
        }

        private void OnDestroy()
        {
            foreach (var entry in memberButtonEntries)
            {
                if (entry.memberBtn != null)
                    entry.memberBtn.onClick.RemoveAllListeners();

                var handler = entry.memberBtn?.gameObject.GetComponent<MemberDragHandler>();
                if (handler != null)
                {
                    handler.OnDragBegin -= HandleDragBegin;
                    handler.OnDragging  -= HandleDragging;
                    handler.OnDragEnd   -= HandleDragEnd;
                    handler.OnClicked   -= HandleMemberClickedByDrag;
                }
            }

            if (_ghostImage != null)
                Destroy(_ghostImage.gameObject);
        }

        public void AddMemberToSlot(MemberType type, Sprite icon, string name, bool isTop)
        {
            if (_occupiedSlots.ContainsKey(type)) return;

            var entry       = memberButtonEntries.Find(e => e.memberType == type);
            var targetSlots = isTop ? topSlots : bottomSlots;
            var slot        = targetSlots.Find(s => !s.IsOccupied);

            if (slot == null)
            {
                Debug.LogWarning($"[ParticipationView] {(isTop ? "Top" : "Bottom")} 슬롯이 가득 찼습니다.");
                return;
            }

            var color = entry != null ? entry.memberColor : Color.white;
            slot.Assign(icon, name, color);
            _occupiedSlots[type] = slot;

            Action handler = () => OnSlotCancelClicked?.Invoke(type);
            slot.OnCancelClicked += handler;
            _cancelHandlers[type] = handler;
        }

        public void RemoveMemberFromSlot(MemberType type)
        {
            if (!_occupiedSlots.TryGetValue(type, out var slot)) return;

            if (_cancelHandlers.TryGetValue(type, out var handler))
            {
                slot.OnCancelClicked -= handler;
                _cancelHandlers.Remove(type);
            }

            slot.Clear();
            _occupiedSlots.Remove(type);
        }

        public void SetMemberIcon(MemberType type, Sprite icon)
        {
            var entry = memberButtonEntries.Find(e => e.memberType == type);
            if (entry?.memberIconImage != null)
                entry.memberIconImage.sprite = icon;
        }

        public void SetConditionIcon(MemberType type, Sprite conditionIcon)
        {
            var entry = memberButtonEntries.Find(e => e.memberType == type);
            if (entry?.memberConditionImage != null)
                entry.memberConditionImage.sprite = conditionIcon;
        }

        public void SetConditionIconVisible(bool visible)
        {
            foreach (var entry in memberButtonEntries)
            {
                if (entry.memberConditionImage != null)
                    entry.memberConditionImage.gameObject.SetActive(visible);
            }
        }

        public void SetMemberVisible(MemberType type, bool visible)
        {
            var entry = memberButtonEntries.Find(e => e.memberType == type);

            if (entry?.parentObject != null)
            {
                entry.parentObject.SetActive(visible);

                var cg = entry.memberBtn.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha          = visible ? 1f : 0f;
                    cg.blocksRaycasts = visible;
                    cg.interactable   = visible;
                }
            }
        }

        public void SetMemberInteractable(MemberType type, bool interactable)
        {
        }
    }
}