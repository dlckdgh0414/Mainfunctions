using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class PromotionView : MonoBehaviour
    {
        [SerializeField] private GameObject promotionPanel;
        [SerializeField] private Transform listParent;
        [SerializeField] private PromotionListBar listBarPrefab;
        [SerializeField] private Button prevBtn;
        [SerializeField] private Button nextBtn;

        private readonly List<PromotionListBar> _bars = new();

        public event Action<PromotionListBar> OnBarClicked;
        public event Action OnPrevClicked;
        public event Action OnNextClicked;

        private void Awake()
        {
            if (promotionPanel != null)
                promotionPanel.SetActive(false);

            if (prevBtn != null)
                prevBtn.onClick.AddListener(() => OnPrevClicked?.Invoke());
            if (nextBtn != null)
                nextBtn.onClick.AddListener(() => OnNextClicked?.Invoke());
        }

        private void OnDestroy()
        {
            prevBtn?.onClick.RemoveAllListeners();
            nextBtn?.onClick.RemoveAllListeners();
        }

        public void Show() => promotionPanel?.SetActive(true);
        public void Hide() => promotionPanel?.SetActive(false);

        public void ClearBars()
        {
            foreach (var bar in _bars)
                if (bar != null) DestroyImmediate(bar.gameObject);
            _bars.Clear();
        }

        public PromotionListBar CreateBar()
        {
            if (listBarPrefab == null || listParent == null) return null;
            var bar = Instantiate(listBarPrefab, listParent);
            bar.OnClicked += b => OnBarClicked?.Invoke(b);
            _bars.Add(bar);
            return bar;
        }

        public void SetPrevBtnInteractable(bool interactable)
        {
            if (prevBtn != null)
                prevBtn.interactable = interactable;
        }

        public void SetNextBtnInteractable(bool interactable)
        {
            if (nextBtn != null)
                nextBtn.interactable = interactable;
        }
    }
}