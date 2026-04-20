using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.StatSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    [Serializable]
    public class MemberInfoUIEntry
    {
        public MemberType memberType;
        public Button memberButton;
        public Image compositionIconImage;
        public Image proficiencyImage;
        public TextMeshProUGUI memberListName;
        public Image IconImage;
        public List<MemberStatUI> memberStatUI;
    }

    public class MemberInfoView : MonoBehaviour, IMemberInfoView
    {
        [SerializeField] private List<MemberInfoUIEntry> entries;
        [SerializeField] private Image compositionImage;
        [SerializeField] private Image proficiencyImage;
        private IMemberInfoView _memberInfoViewImplementation;

        public event Action<MemberType> OnMemberButtonClicked;

        public void InitStatUI()
        {
            foreach (var entry in entries)
            {
                if (entry.memberStatUI == null) continue;
                foreach (var statUI in entry.memberStatUI)
                    statUI?.SetMemberType(entry.memberType);

                var capturedType = entry.memberType;
                entry.memberButton?.onClick.AddListener(() => OnMemberButtonClicked?.Invoke(capturedType));
            }
        }
        
        public void SetMemberListName(MemberType type, string name)
        {
            var entry = Find(type);
            if (entry?.memberListName != null)
                entry.memberListName.SetText(name);
        }

        public void SetMemberIcon(MemberType type, Sprite icon)
        {
            var entry = Find(type);
            if (entry?.IconImage != null)
                entry.IconImage.sprite = icon;
        }
        
        public void SetCompositionSprite(Sprite sprite)
        {
            if (sprite == null) return;
            if (compositionImage != null)
                compositionImage.sprite = sprite;
            foreach (var entry in entries)
                if (entry.compositionIconImage != null)
                    entry.compositionIconImage.sprite = sprite;
        }

        public void SetProficiencySprite(Sprite sprite)
        {
            if (sprite == null) return;
            if (proficiencyImage != null)
                proficiencyImage.sprite = sprite;
            foreach (var entry in entries)
                if (entry.proficiencyImage != null)
                    entry.proficiencyImage.sprite = sprite;
        }

        private MemberInfoUIEntry Find(MemberType type)
            => entries?.Find(e => e.memberType == type);
    }
}