using Code.Core;
using Code.MainSystem.StatSystem.BaseStats;

namespace Code.MainSystem.TraitSystem.TraitEffect
{
    public abstract class MemberStatEffect : MultiStatModifierEffect
    {
        /// <summary>
        /// 멤버 타입에 따른 주요 스탯 배열을 반환합니다.
        /// </summary>
        protected StatType[] GetMajorStatsByMember(MemberType memberType)
        {
            return memberType switch
            {
                // MemberType.Guitar => new[] { StatType.GuitarEndurance, StatType.GuitarConcentration },
                // MemberType.Drums => new[] { StatType.DrumsSenseOfRhythm, StatType.DrumsPower },
                // MemberType.Bass => new[] { StatType.BassDexterity, StatType.BassSenseOfRhythm },
                // MemberType.Vocal => new[] { StatType.VocalVocalization, StatType.VocalBreathing },
                // MemberType.Piano => new[] { StatType.PianoDexterity, StatType.PianoStagePresence },
                _ => null
            };
        }
    }
}