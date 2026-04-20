using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.MainSystem.NewMainScreen
{
    public class MemberDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public int MemberIndex { get; set; }

        public event Action<int>                   OnDragBegin;
        public event Action<PointerEventData>      OnDragging;
        public event Action<int, PointerEventData> OnDragEnd;
        public event Action<int>                   OnClicked;

        private bool _dragged;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragged = true;
            OnDragBegin?.Invoke(MemberIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragging?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnd?.Invoke(MemberIndex, eventData);
            _dragged = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragged) return;
            OnClicked?.Invoke(MemberIndex);
        }
    }
}