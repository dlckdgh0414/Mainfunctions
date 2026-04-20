using System;
using Code.Core;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public interface ISchedulePresenter
    {
        event Action<ManagementBtnType> OnScheduleStarted;
        event Action OnScheduleCancelled;
        event Action OnAllSchedulesCompleted;

        void RegisterCurrentSchedule();
        void StartAllSchedules();
        void Cancel();
        void ExecuteNextSchedule();
        void Dispose();
    }
}