using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.NewMainScreen.Data;
using Code.MainSystem.NewMainScreen.MVP.View;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.NewMainScreen.MVP.Presenter
{
    public class MemberInfoPresenter : IMemberInfoPresenter
    {
        private readonly List<AssetReference> _memberDataRefs;
        private readonly IMemberInfoView _view;
        private readonly MemberInfoView _memberInfoView;
        private readonly Sprite _compositionSprite;
        private readonly Sprite _proficiencySprite;

        private readonly List<MemberDataSO> _memberDataList = new();
        public IReadOnlyList<MemberDataSO> MemberDataList => _memberDataList;

        public event Action<MemberType> OnLeaderChanged;

        public MemberInfoPresenter(
            List<AssetReference> memberDataRefs,
            MemberInfoView view,
            Sprite compositionSprite,
            Sprite proficiencySprite)
        {
            _memberDataRefs    = memberDataRefs;
            _view              = view;
            _memberInfoView    = view;
            _compositionSprite = compositionSprite;
            _proficiencySprite = proficiencySprite;

            _memberInfoView.OnMemberButtonClicked += HandleMemberButtonClicked;
        }

        public async UniTask<List<MemberDataSO>> LoadAsync()
        {
            _memberDataList.Clear();

            if (_memberDataRefs == null) return _memberDataList;

            foreach (var memberRef in _memberDataRefs)
            {
                if (memberRef == null || !memberRef.RuntimeKeyIsValid()) continue;

                try
                {
                    var data = await memberRef.LoadAssetAsync<MemberDataSO>().ToUniTask();
                    if (data != null)
                    {
                        await data.LoadAssets();
                        _memberDataList.Add(data);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MemberInfoPresenter] 멤버 데이터 로드 실패: {e.Message}");
                }
            }
            _view.InitStatUI();

            return _memberDataList;
        }

        public void Refresh()
        {
            _view.SetCompositionSprite(_compositionSprite);
            _view.SetProficiencySprite(_proficiencySprite);

            foreach (var memberData in _memberDataList)
            {
                _view.SetMemberListName(memberData.memberType, memberData.memberListName);

                if (memberData.IconSprite != null)
                    _view.SetMemberIcon(memberData.memberType, memberData.IconSprite);
            }
        }

        public void Dispose()
        {
            if (_memberInfoView != null)
                _memberInfoView.OnMemberButtonClicked -= HandleMemberButtonClicked;
        }

        private void HandleMemberButtonClicked(MemberType type)
            => OnLeaderChanged?.Invoke(type);
    }
}