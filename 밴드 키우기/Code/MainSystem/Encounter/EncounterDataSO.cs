using Code.MainSystem.Dialogue;
using UnityEngine;

namespace Code.MainSystem.Encounter
{
    public enum EncounterConditionType
    {
        BuskingCaseFall, // 버스킹 조건 불만족
        BuskingFall, // 버스킹 클리어 실패
        BuskingSuccess, // 버스킹 성공
        StatCaseFall, // 스텟 검사
        LiveCaseFall, // 라이브 조건실패
        LiveFall, // 라이브 실패
        LiveSuccess, // 라이브 성공
        TrainingEnd, // 훈련 종료시
        TeamPractice, // 합주시
        TraitsGet, //특성 일정량 획득
        TurnStart, // 턴 시작
    }
    
    [CreateAssetMenu(fileName = "EncounterData", menuName = "SO/Encounter/Data", order = 0)]
    public class EncounterDataSO : ScriptableObject
    {
        public DialogueInformationSO dialogue; // 해당하는 다이알로그
        [Range(0, 1.0f)] public float percent; // 발생 확률(1이 최대, 무조건 발생은 1로)
        public EncounterConditionType type; // 언제 나올지
    }
}