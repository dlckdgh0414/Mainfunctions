using System;
using System.Collections;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem
{
    [Serializable]
    public class MessageIconData
    {
        public SystemMessageIconType icon;
        public Sprite sprite;
    }

    public class SystemMessageUI : MonoBehaviour
    {
        [SerializeField] private GameObject BG;
        [SerializeField] private List<MessageIconData> messageIcons;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI messageText;

        [SerializeField] private float hideDelay = 1.5f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            BG.SetActive(false);
            DontDestroyOnLoad(gameObject);
            Bus<SystemMessageEvent>.OnEvent += HandlleMessageEvent;
        }

        private void HandlleMessageEvent(SystemMessageEvent evt)
        {
            BG.SetActive(true);

            foreach (var data in messageIcons)
            {
                if (evt.IconType == data.icon)
                    icon.sprite = data.sprite;
            }

            messageText.text = evt.Message;

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(hideDelay);
            BG.SetActive(false);
            _hideCoroutine = null;
        }

        private void OnDestroy()
        {
            Bus<SystemMessageEvent>.OnEvent -= HandlleMessageEvent;
        }
    }
}