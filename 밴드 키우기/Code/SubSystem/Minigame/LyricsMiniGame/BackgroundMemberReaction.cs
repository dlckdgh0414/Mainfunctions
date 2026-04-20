using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    public class BackgroundMemberReaction : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private BackgroundMemberDataSO memberData;

        [Header("Speech Bubble")]
        [SerializeField] private GameObject      bubbleRoot;
        [SerializeField] private TextMeshProUGUI bubbleText;

        [Header("Member Image")]
        [SerializeField] private Image memberImage;

        [Header("Display Settings")]
        [SerializeField] private float displayDuration = 2f;

        private static readonly float AlphaDimmed = 143f / 255f;
        private static readonly float AlphaActive = 1f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            HideBubble();
            SetImageAlpha(AlphaDimmed);
        }

        public void ReactToGoodItem()
        {
            if (memberData == null || memberData.GoodReactions.Count == 0) return;
            ShowBubble(memberData.GoodReactions[Random.Range(0, memberData.GoodReactions.Count)]);
        }

        public void ReactToBadItem()
        {
            if (memberData == null || memberData.BadReactions.Count == 0) return;
            ShowBubble(memberData.BadReactions[Random.Range(0, memberData.BadReactions.Count)]);
        }

        private void ShowBubble(string message)
        {
            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            bubbleText.text = message;
            bubbleRoot.SetActive(true);
            SetImageAlpha(AlphaActive);

            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            HideBubble();
        }

        private void HideBubble()
        {
            if (bubbleRoot != null)
                bubbleRoot.SetActive(false);

            SetImageAlpha(AlphaDimmed);
        }

        private void SetImageAlpha(float alpha)
        {
            if (memberImage == null) return;
            Color c = memberImage.color;
            c.a = alpha;
            memberImage.color = c;
        }
    }
}