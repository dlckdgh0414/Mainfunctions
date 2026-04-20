using System.Collections.Generic;
using Code.MainSystem.NewMainScreen.Data;
using Cysharp.Threading.Tasks;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public interface IMemberInfoPresenter
    {
        UniTask<List<MemberDataSO>> LoadAsync();
        void Refresh();
    }
}
