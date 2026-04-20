using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.NewMainScreen.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen
{
    [Serializable]
    public class ScheduleRegistrationUIData
    {
        public MemberType memberType;
        public Image      icon;
        public Image      background;
        public Color      memberColor = Color.white;
    }

    public class ScheduleRegistrationUI : MonoBehaviour
    {
        [SerializeField] private List<ScheduleRegistrationUIData> memberIconList;

        private void Awake()
        {
            foreach (var item in memberIconList)
                if (item.background != null)
                    item.background.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        public void SetupRegistrationUI(List<MemberDataSO> members)
        {
            gameObject.SetActive(true);

            foreach (var item in memberIconList)
                if (item.background != null)
                    item.background.gameObject.SetActive(false);

            foreach (var data in members)
            {
                foreach (var item in memberIconList)
                {
                    if (item.memberType != data.memberType) continue;
                    item.icon.sprite = data.IconSprite;

                    if (item.background != null)
                    {
                        item.background.color = item.memberColor;
                        item.background.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void SetupRegistrationUI(Dictionary<MemberType, Sprite> iconMap)
        {
            gameObject.SetActive(true);

            foreach (var item in memberIconList)
            {
                if (iconMap.TryGetValue(item.memberType, out var sprite))
                {
                    item.icon.sprite = sprite;

                    if (item.background != null)
                    {
                        item.background.color = item.memberColor;
                        item.background.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (item.background != null)
                        item.background.gameObject.SetActive(false);
                }
            }
        }

        public void Hide()
        {
            foreach (var item in memberIconList)
                if (item.background != null)
                    item.background.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }
}