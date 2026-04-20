using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager.Upgarde
{
    public class EndEventUUI : MonoBehaviour
    {
        [SerializeField] private Image memberIcon;
        [SerializeField] private TextMeshProUGUI speechText;
        [SerializeField] private string failStr;
        [SerializeField] private string successStr;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private TextMeshProUGUI topText;

        private Action _onConfirm;

        private void Awake()
        {
            confirmBtn.onClick.AddListener(HandleConfirm);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            confirmBtn.onClick.RemoveAllListeners();
        }

        public void Show(Sprite icon, bool success, Action onConfirm)
        {
            _onConfirm = onConfirm;
            memberIcon.sprite = icon;
            speechText.text = success ? successStr : failStr;
            gameObject.SetActive(true);
            confirmBtn.gameObject.SetActive(true);
            topText.text = success ? "업그레이드 성공" : "업그레이드 실패";
        }

        private void HandleConfirm()
        {
            gameObject.SetActive(false);
            _onConfirm?.Invoke();
        }
    }
}