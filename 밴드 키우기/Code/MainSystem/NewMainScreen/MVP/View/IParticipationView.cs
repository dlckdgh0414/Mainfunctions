using System;
using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public interface IParticipationView
    {
        event Action<MemberType> OnMemberClicked;
        event Action<MemberType> OnSlotCancelClicked;
        event Action<MemberType, bool> OnMemberDroppedToSlot;

        void AddMemberToSlot(MemberType type, Sprite icon, string name, bool isTop);
        void RemoveMemberFromSlot(MemberType type);
        void SetMemberIcon(MemberType type, Sprite icon);
        void SetConditionIcon(MemberType type, Sprite conditionIcon);
        void SetConditionIconVisible(bool visible);
        void SetMemberVisible(MemberType type, bool visible);
        void SetMemberInteractable(MemberType type, bool interactable);
    }
}