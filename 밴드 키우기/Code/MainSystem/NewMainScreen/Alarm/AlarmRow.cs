using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace Code.MainSystem.NewMainScreen.Alarm
{
    public class AlarmRow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text dDayText;

        private Action        _onDismiss;
        private RectTransform _rect;
        private Vector2       _startPos;

        private const float DismissThreshold = 150f;

        public void Setup(string title, string dDay, Action onDismiss)
        {
            titleText.text = title;
            dDayText.text  = dDay;
            _onDismiss     = onDismiss;
            _rect          = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _startPos = _rect.anchoredPosition; 
        }

        public void OnDrag(PointerEventData e)
        {
            float delta = e.position.x - e.pressPosition.x;
            if (delta > 0)
                _rect.anchoredPosition = _startPos + new Vector2(delta, 0);
        }

        public void OnEndDrag(PointerEventData e)
        {
            float delta = e.position.x - e.pressPosition.x;
            if (delta > DismissThreshold)
                _onDismiss?.Invoke();
            else
                _rect.DOAnchorPos(_startPos, 0.2f).SetEase(Ease.OutQuart);
        }
    }
}