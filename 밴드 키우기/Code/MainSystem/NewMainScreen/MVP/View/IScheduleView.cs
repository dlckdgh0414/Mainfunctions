using System;
using Code.Core;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public interface IScheduleView
    {
        event Action<ManagementBtnType> OnActivityClicked;
        event Action OnRegisterClicked;
        event Action OnStartAllClicked;
        event Action OnCancelClicked;
        event Action OnScheduleGroupEntered;

        void SetAppBarVisible(bool visible);
        void SetActivityButtonsInteractable(bool interactable);
        void SetTreeBarVisible(bool visible);
        void SetStartBarVisible(bool visible);
        void SetMemberListBarVisible(bool visible);
        void SetParticipationInfoVisible(bool visible);
        void SetParticipationInfoText(string text);
        void SetCellPhoneTimeVisible(bool visible);
        void SetCellPhoneText(string text);
        void SetStartAllButtonVisible(bool visible);
        void SetRegisterButtonInteractable(bool interactable);
        void SetRegisteredScheduleBarVisible(bool visible);
        void SetMusicUploadButtonVisible(bool visible);
        bool IsInScheduleGroup { get; }
        void ReturnToLastGroup();
    }
}