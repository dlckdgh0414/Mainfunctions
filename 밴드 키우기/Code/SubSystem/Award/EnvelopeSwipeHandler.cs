using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.SubSystem.Award
{
    public class EnvelopeSwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private AwardEnvelopeUI awardEnvelopeUI;
        [SerializeField] private float swipeThreshold = 80f;

        private Vector2 _startPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            float deltaY = eventData.position.y - _startPos.y;
            if (deltaY > swipeThreshold)
                awardEnvelopeUI.TryOpenEnvelope();
        }
    }
}