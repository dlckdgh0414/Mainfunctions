using Code.Core;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public interface IMemberInfoView
    {
        void SetMemberListName(MemberType type, string name);
        void SetMemberIcon(MemberType type, Sprite icon);
        void SetCompositionSprite(Sprite sprite);
        void SetProficiencySprite(Sprite sprite);
        void InitStatUI();
    }
}