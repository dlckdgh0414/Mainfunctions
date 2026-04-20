using System.Collections;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.UI
{
    public class StatIncreaseDecreaseUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject statIncreaseArrow;
        [SerializeField] private GameObject statDecreaseArrow;
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private Image statIcon;
        [SerializeField] private TextMeshProUGUI statAmountText;

        [Header("Animation")]
        [SerializeField] private float moveDistance = 40f;
        [SerializeField] private float duration = 0.6f;

        private RectTransform rect;
        private CanvasGroup canvasGroup;
        private Coroutine playRoutine;

        private Vector2 originPos;

        private void Awake()
        {
            rect = root.GetComponent<RectTransform>();
            canvasGroup = root.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = root.AddComponent<CanvasGroup>();

            originPos = rect.anchoredPosition;
            root.SetActive(false);

            Bus<StatIncreaseDecreaseEvent>.OnEvent += HandleStatIncreaseDecrease;
            Bus<StopEvent>.OnEvent += HandleStopEvent;
        }

        private void HandleStopEvent(StopEvent evt)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            rect.anchoredPosition = originPos;
            canvasGroup.alpha = 0f;
            root.SetActive(false);
        }

        private void HandleStatIncreaseDecrease(StatIncreaseDecreaseEvent evt)
        {
            root.SetActive(true);

            if (playRoutine != null)
                StopCoroutine(playRoutine);

            canvasGroup.alpha = 1f;
            rect.anchoredPosition = originPos;

            playRoutine = StartCoroutine(Play(evt));
        }

        private IEnumerator Play(StatIncreaseDecreaseEvent evt)
        {
            statIncreaseArrow.SetActive(evt.Increase);
            statDecreaseArrow.SetActive(!evt.Increase);

            statText.text = evt.StatName;
            statAmountText.text = evt.Increase ? "+" + evt.Amount : "-" + evt.Amount;
            statIcon.sprite = evt.StatIcon;

            yield return new WaitForSeconds(0.2f);

            Vector2 targetPos = originPos + Vector2.up * (evt.Increase ? moveDistance : -moveDistance);

            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                rect.anchoredPosition = Vector2.Lerp(originPos, targetPos, t);
                canvasGroup.alpha = 1f - t;

                yield return null;
            }

            root.SetActive(false);
            playRoutine = null;
        }

        private void OnDestroy()
        {
            Bus<StatIncreaseDecreaseEvent>.OnEvent -= HandleStatIncreaseDecrease;
            Bus<StopEvent>.OnEvent -= HandleStopEvent;
        }
    }
}
