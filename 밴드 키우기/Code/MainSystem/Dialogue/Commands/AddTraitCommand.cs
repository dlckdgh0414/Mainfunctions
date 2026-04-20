using System;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;

namespace Code.MainSystem.Dialogue.Commands
{
    /// <summary>
    /// 특성 추가 명령어
    /// </summary>
    [Serializable]
    public class AddTraitCommand : IDialogueCommand
    {
        public MemberType targetMember;
        public string traitId;

        public void Execute()
        {
            // TraitDatabaseSO가 Resources 안에 "Database/TraitDatabase" 로 있다고 가정 (추후 매니저 연동 가능)
            TraitDatabaseSO database = UnityEngine.Resources.Load<TraitDatabaseSO>("Database/TraitDatabase");
            if (database != null)
            {
                TraitDataSO traitData = database.GetTrait(traitId);
                if (traitData != null)
                {
                    Bus<TraitAddRequested>.Raise(new TraitAddRequested(targetMember, traitData));
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[AddTraitCommand] Trait '{traitId}' not found in Database.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[AddTraitCommand] TraitDatabase not found in Resources/Database/TraitDatabase.");
            }
        }
    }
}
