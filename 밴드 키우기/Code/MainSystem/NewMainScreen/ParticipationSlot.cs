using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    public class ParticipationSlot : MonoBehaviour
    {
        [SerializeField] private Image           iconImage;
        [SerializeField] private Button          cancelButton;
        [SerializeField] private Image           backgroundImage;

        public bool IsOccupied { get; private set; }

        public event Action OnCancelClicked;

        private void Awake()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
            }
        }

        private void OnDestroy()
        {
            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }

        public void Assign(Sprite icon, string memberName, Color bgColor)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = true;
            IsOccupied = true;

            if (backgroundImage != null)
                backgroundImage.color = bgColor;

            if (cancelButton != null)
                cancelButton.interactable = true;
        }

        public void Clear()
        {
            iconImage.sprite  = null;
            iconImage.enabled = false;
            IsOccupied = false;

            if (backgroundImage != null)
                backgroundImage.color = Color.white;

            if (cancelButton != null)
                cancelButton.interactable = false;
        }
    }
}