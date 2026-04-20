using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class UIImageMover : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private bool goUp = true;
        [SerializeField] private float speed = 100f;
        [SerializeField] private float moveAmount = 50f;
        [SerializeField] private Image arrowImage;

        private RectTransform _rectTransform;
        private Vector2 _startPos;
        private float _currentOffset = 0f;

        public void OpenCellPhon()
        {
            arrowImage.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
        }

        public void CloseCellPhon()
        {
            arrowImage.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startPos = _rectTransform.anchoredPosition;
            CloseCellPhon();
        }

        void Update()
        {
            float direction = goUp ? 1f : -1f;

            _currentOffset += direction * speed * Time.deltaTime;

            if (_currentOffset >= moveAmount)
            {
                _currentOffset = moveAmount;
                goUp = false;
            }
            else if (_currentOffset <= -moveAmount)
            {
                _currentOffset = -moveAmount;
                goUp = true;
            }

            _rectTransform.anchoredPosition = _startPos + new Vector2(0f, _currentOffset);
        }
    }
}