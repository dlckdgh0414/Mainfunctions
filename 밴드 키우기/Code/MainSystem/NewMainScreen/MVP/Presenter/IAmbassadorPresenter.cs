using Code.MainSystem.NewMainScreen.Data;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public interface IAmbassadorPresenter
    {
        void SetData(MemberAmbassadorDataSO data);
        void OnPhonePutAway();

        void Dispose();
    }
}