using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class PromotionListBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI promotionNameText;
        [SerializeField] private TextMeshProUGUI promotionPriceText;
        [SerializeField] private Button promotionBtn;

        public event Action<PromotionListBar> OnClicked;

        public string PromotionName { get; private set; }
        public int PromotionPrice { get; private set; }
        public int AddFans { get; private set; }

        private void Awake()
        {
            promotionBtn.onClick.AddListener(() => OnClicked?.Invoke(this));
        }

        private void OnDestroy()
        {
            promotionBtn.onClick.RemoveAllListeners();
        }

        public void Setup(string name, int price, int addFans)
        {
            PromotionName = name;
            PromotionPrice = price;
            AddFans = addFans;

            promotionNameText.text = name;
            promotionPriceText.text = $"{price}";
        }
    }
}