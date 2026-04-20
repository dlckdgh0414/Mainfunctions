using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.NewMainScreen.Data;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public interface IParticipationPresenter
    {
        int CurrentSlotCount { get; }
        event Action OnSlotChanged;

        void Initialize(List<MemberDataSO> memberDataList);
        void Refresh();
        void ClearCurrent();
        void ClearAll();

        void OnActivityRegistered(ManagementBtnType type, List<MemberDataSO> members);
        void OnActivityUnregistered(ManagementBtnType type);
        void RestoreSlots(ManagementBtnType type, List<MemberDataSO> members);
        void SetMembersInteractable(List<MemberType> memberTypes, bool interactable);
        void SetConditionIconVisible(bool visible);
    }
}