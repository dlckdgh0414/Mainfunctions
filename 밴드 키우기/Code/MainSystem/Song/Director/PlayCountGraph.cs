using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Linq;
using Code.Core.UI;
using TMPro;

namespace Code.MainSystem.Song.Director
{
    public class PlayCountGraph : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UILineRenderer lineRenderer;
        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private TextMeshProUGUI labelPrefab;

        [Header("Font Settings")]
        // 1. 적용하고 싶은 폰트 에셋을 인스펙터에서 할당하세요.
        [SerializeField] private TMP_FontAsset targetFont; 
        [SerializeField] private float fontSize = 24f;
        [SerializeField] private Color fontColor = Color.white;

        private RectTransform _rectTransform;
        private List<GameObject> _instantiatedObjects = new List<GameObject>();
        private List<Vector2> _targetPoints = new List<Vector2>();
        private List<Vector2> _currentLinePoints = new List<Vector2>();

        private void Awake() => _rectTransform = GetComponent<RectTransform>();

        public void Reset() => ResetGraph();

        public async UniTask PlayGraphAnimation(int totalPlayCount)
        {
            ResetGraph();
            Canvas.ForceUpdateCanvases(); 
            
            Rect rect = _rectTransform.rect;
            float padding = 100f;
            float usableWidth = rect.width - (padding * 2);
            float usableHeight = rect.height - (padding * 2);
            float xOffset = rect.xMin + padding; 
            float yOffset = rect.yMin + padding;

            List<int> dailyValues = GenerateDailyValues(totalPlayCount);
            float maxVal = dailyValues.Max();
            if (maxVal <= 0) maxVal = 1f;

            // 축 표시 및 폰트 적용
            DrawAxes(rect, padding, (int)maxVal, xOffset, yOffset, usableWidth, usableHeight);

            float xStep = usableWidth / 6f; 
            for (int i = 0; i < dailyValues.Count; i++)
            {
                float xPos = xOffset + (i * xStep);
                float yPos = yOffset + ((float)dailyValues[i] / maxVal * usableHeight);
                _targetPoints.Add(new Vector2(xPos, yPos));
            }

            for (int i = 0; i < _targetPoints.Count; i++)
            {
                Vector2 targetPos = _targetPoints[i];
                CreateDot(targetPos);

                if (i == 0)
                {
                    _currentLinePoints.Add(targetPos);
                    lineRenderer.SetLinePositions(_currentLinePoints.ToArray());
                }
                else
                {
                    await AnimateLineManual(_currentLinePoints[_currentLinePoints.Count - 1], targetPos);
                }
                await UniTask.WaitForSeconds(0.2f); 
            }
        }

        private void DrawAxes(Rect rect, float padding, int maxVal, float xOffset, float yOffset, float width, float height)
        {
            // 1. X축 라벨 (1일차 ~ 7일차)
            float xStep = width / 6f;
            for (int i = 0; i < 7; i++)
            {
                var label = Instantiate(labelPrefab, _rectTransform);
                _instantiatedObjects.Add(label.gameObject);
                
                // --- 폰트 및 스타일 설정 ---
                ApplyTextStyle(label);
                
                label.text = $"{i + 1}일차";
                label.rectTransform.anchoredPosition = new Vector2(xOffset + (i * xStep), rect.yMin + (padding * 0.5f));
                label.alignment = TextAlignmentOptions.Center;
            }

            // 2. Y축 라벨 (0, Max/2, Max 세 지점 표시)
            int[] yTicks = { 0, maxVal / 2, maxVal };
            foreach (var val in yTicks)
            {
                var label = Instantiate(labelPrefab, _rectTransform);
                _instantiatedObjects.Add(label.gameObject);
                
                // --- 폰트 및 스타일 설정 ---
                ApplyTextStyle(label);
                
                label.text = val.ToString("N0");
                float yPos = yOffset + ((float)val / maxVal * height);
                label.rectTransform.anchoredPosition = new Vector2(rect.xMin + (padding * 0.5f), yPos);
                label.alignment = TextAlignmentOptions.Right;
            }
        }

        /// <summary>
        /// 인스턴스화된 텍스트에 일괄적으로 폰트와 스타일을 적용합니다.
        /// </summary>
        private void ApplyTextStyle(TextMeshProUGUI label)
        {
            if (targetFont != null)
            {
                label.font = targetFont;
            }
            label.fontSize = fontSize;
            label.color = fontColor;
        }

        // --- 이하 애니메이션 및 데이터 생성 로직 동일 ---
        private async UniTask AnimateLineManual(Vector2 startPos, Vector2 endPos)
        {
            _currentLinePoints.Add(startPos);
            int lastIndex = _currentLinePoints.Count - 1;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentLinePoints[lastIndex] = Vector2.Lerp(startPos, endPos, elapsed / duration);
                lineRenderer.SetLinePositions(_currentLinePoints.ToArray());
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _currentLinePoints[lastIndex] = endPos;
            lineRenderer.SetLinePositions(_currentLinePoints.ToArray());
        }

        private List<int> GenerateDailyValues(int total)
        {
            List<float> weights = new List<float>();
            bool isIncreasing = total <= 1000;
            
            for (int i = 0; i < 7; i++)
            {
                float baseWeight = isIncreasing ? (i + 1) : (7 - i);
                float randomFactor = Random.Range(0.8f, 1.2f);
                weights.Add(baseWeight * randomFactor);
            }

            float weightSum = weights.Sum();
            List<int> values = new List<int>();
            int currentSum = 0;

            for (int i = 0; i < 6; i++)
            {
                int dailyVal = Mathf.RoundToInt((weights[i] / weightSum) * total);
                values.Add(dailyVal);
                currentSum += dailyVal;
            }
            values.Add(Mathf.Max(0, total - currentSum));
            return values;
        }

        private void CreateDot(Vector2 position)
        {
            GameObject dot = Instantiate(dotPrefab, _rectTransform);
            _instantiatedObjects.Add(dot);
            dot.GetComponent<RectTransform>().anchoredPosition = position;
            dot.transform.localScale = Vector3.zero;
            dot.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }

        private void ResetGraph()
        {
            foreach (var obj in _instantiatedObjects)
            {
                if(obj != null) Destroy(obj);
            }
            _instantiatedObjects.Clear();
            _targetPoints.Clear();
            _currentLinePoints.Clear();
            lineRenderer.SetLinePositions(System.Array.Empty<Vector2>());
        }
    }
}