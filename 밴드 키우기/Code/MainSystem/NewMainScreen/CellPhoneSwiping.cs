using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;

namespace Code.MainSystem.NewMainScreen
{
    public class CellPhoneSwiping : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("스와이프 설정")]
        [SerializeField] private float swipeThreshold = 100f;

        [Header("핸드폰 이동 설정")]
        [SerializeField] private RectTransform phoneRectTransform;
        [SerializeField] private Vector2 hiddenPosition;
        [SerializeField] private float moveSpeed = 8f;

        [Header("시작 설정")]
        [SerializeField] private bool startHidden = true;

        [Header("레이아웃")]
        [SerializeField] private LayoutGroup layoutGroup;

        [Header("이벤트")]
        public UnityEvent onPhonePutAway;
        public UnityEvent onPhoneTakeOut;

        private Vector2 _startDragPos;
        private bool    _isPhoneHidden;
        private Vector2 _shownPosition;
        private bool    _isAnimating;
        private Vector2 _targetPosition;
        private bool    _swipeEnabled = true;

        private void Awake()
        {
            if (phoneRectTransform == null)
                phoneRectTransform = GetComponent<RectTransform>();

            _shownPosition = phoneRectTransform.anchoredPosition;

            if (startHidden)
            {
                phoneRectTransform.anchoredPosition = hiddenPosition;
                _isPhoneHidden = true;
            }
        }

        private void Update()
        {
            if (_isAnimating) AnimatePhone();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_swipeEnabled) return;

            // 애니메이션 중 드래그 시작되면 목표 위치로 즉시 스냅
            if (_isAnimating)
            {
                phoneRectTransform.anchoredPosition = _targetPosition;
                _isAnimating = false;
            }

            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
                Canvas.ForceUpdateCanvases();
            }

            _startDragPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_swipeEnabled) return;

            Vector2 delta = eventData.position - _startDragPos;

            if (!_isPhoneHidden)
            {
                float clampedY = Mathf.Min(0, delta.y);
                phoneRectTransform.anchoredPosition = _shownPosition + new Vector2(0, clampedY);
            }
            else
            {
                float clampedY = Mathf.Max(0, delta.y);
                phoneRectTransform.anchoredPosition = hiddenPosition + new Vector2(0, clampedY);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_swipeEnabled) return;

            float deltaY = eventData.position.y - _startDragPos.y;

            if (Mathf.Abs(deltaY) < swipeThreshold)
            {
                _targetPosition = _isPhoneHidden ? hiddenPosition : _shownPosition;
                _isAnimating = true;
                return;
            }

            if (deltaY < 0 && !_isPhoneHidden)
                PutAwayPhone();
            else if (deltaY > 0 && _isPhoneHidden)
                TakeOutPhone();
            else
            {
                _targetPosition = _isPhoneHidden ? hiddenPosition : _shownPosition;
                _isAnimating = true;
            }
        }

        private void PutAwayPhone()
        {
            _targetPosition = hiddenPosition;
            _isAnimating    = true;
            _isPhoneHidden  = true;
            onPhonePutAway?.Invoke();
        }

        private void TakeOutPhone()
        {
            _targetPosition = _shownPosition;
            _isAnimating    = true;
            _isPhoneHidden  = false;
            onPhoneTakeOut?.Invoke();
            Bus<TutorialPhoneOpenedEvent>.Raise(new TutorialPhoneOpenedEvent());
        }

        private void AnimatePhone()
        {
            phoneRectTransform.anchoredPosition = Vector2.Lerp(
                phoneRectTransform.anchoredPosition,
                _targetPosition,
                Time.deltaTime * moveSpeed
            );

            if (Vector2.Distance(phoneRectTransform.anchoredPosition, _targetPosition) < 0.5f)
            {
                phoneRectTransform.anchoredPosition = _targetPosition;
                _isAnimating = false;

                if (layoutGroup != null)
                {
                    layoutGroup.enabled = true;
                    Canvas.ForceUpdateCanvases();
                }
            }
        }

        public void SetSwipeEnabled(bool enabled) => _swipeEnabled = enabled;

        public void ForceHidePhone() => PutAwayPhone();
        public void ForceShowPhone() => TakeOutPhone();
        public bool IsPhoneHidden    => _isPhoneHidden;
    }
}