using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.MainSystem.Song.UI
{
    public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _startPosition;
        [HideInInspector] public Transform parentAfterDrag;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            Canvas.ForceUpdateCanvases();
            _startPosition = transform.position;
        }

        [SerializeField] private GameObject feedbackChild; // 드래그 시 켜질 자식 오브젝트

        public void Reset()
        {
            if(_startPosition == Vector3.zero) return;
            transform.position = _startPosition;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = false; // 슬롯이 감지되도록 필수!
            _canvasGroup.alpha = 0.6f;

            // 자식 이미지 활성화
            if (feedbackChild != null) feedbackChild.SetActive(true);

            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / transform.lossyScale.x;

            // 현재 마우스 아래에 있는 오브젝트 검사
            GameObject hoveredObj = eventData.pointerCurrentRaycast.gameObject;

            if (hoveredObj != null && hoveredObj.GetComponent<SongDropSlot>() != null)
            {
                if (feedbackChild != null) feedbackChild.SetActive(true);
            }
            else
            {
                if (feedbackChild != null) feedbackChild.SetActive(false);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (feedbackChild != null) feedbackChild.SetActive(false);

            if (parentAfterDrag == null)
            {
                transform.position = _startPosition;
            }
        }
    }
}