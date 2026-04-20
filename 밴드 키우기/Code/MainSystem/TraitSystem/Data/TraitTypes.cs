using System;

namespace Code.MainSystem.TraitSystem.Data
{
    public enum TraitTrigger
    {
        None,

        // void OnTrigger
        OnPracticeSuccess,      // 연습 성공 직후
        OnPracticeFailed,       // 연습 실패 직후
        OnRestStarted,          // 휴식 시작 시 (루틴 초기화 등)
        OnTurnPassed,           // 턴 종료 시
        OnTraitAdded,           // 특성 획득 시 (최대치 확장 등)
        OnTraitRemoved,         // 특성 제거 시
        OnEnsembleSuccess,      // 합주 성공 시 (이심전심 등)
        EnsembleMental,         // 합주 멤버들 멘탈 능력 증가

        // float QueryValue
        CalcStatMultiplier,     // 능력치 상승 효율 배율 (고위험 루틴, 흐름 되살리기)
        CalcSuccessRateBonus,   // 성공률 가산 보너스 (연습 루틴, 규칙적인 생활)
        CalcConditionCost,      // 컨디션 소모 배율 (지나친 열정)
        CalcTrainingReward,     // 훈련 성공 시 컨디션/멘탈 리워드 가산 (록스피릿)
        CalcEnsembleBonus,      // 합주 효과 배율 (이심전심)

        // bool CheckCondition
        CheckAdditionalAction,  // 추가 행동 발생 여부 (반짝이는 눈, 지나친 열정)
        CheckSuccessGuaranteed,   // 최대 컨디션 돌파 가능 여부
    }
    
    [Flags]
    public enum TraitTag
    {
        None = 0,           // 특성 태그 없음
        Teamwork = 1 << 0,  // 팀워크	
        Support = 1 << 1,   // 백업
        Stability = 1 << 2, // 안정감
        Energy = 1 << 3,    // 텐션 업
        Genius = 1 << 4,    // 천재
        Solo = 1 << 5,      // 독주
        Mastery = 1 << 6,   // 극한 연습
        Immersion = 1 << 7, // 몰입
        GuitarSolo = 1 << 8,// 기타 솔로
    }

    public enum TraitTarget
    {
        None,
        Ensemble,           // 합주 효과 관련
        EnsembleCondition,  // 합주 컨디션 관련
        PracticeCondition,  // 개인 연습 컨디션 관련
        PracticeMental,     // 개인 연습 멘탈 관련
        Practice,           // 개인 연습 효과 관련
        Condition,          // 모든 컨디션 소모/변화 관련
        SuccessRate,        // 훈련/합주 성공률 관련
        Training,           // 능력치 상승 효율 관련
        FeverScore,         // 피버 점수 관련
        FeverTime,          // 피버 지속시간 관련
        FeverInput,           // 하모니 관련
        PracticeHealth,     // 개인 연습 체력 관련
    }

    public enum CalculationType
    {
        Additive,       // + (고정치 증가)
        Subtractive,    // - (고정치 감소)
        PercentAdd,     // +% (퍼센트 합연산 증가)
        PercentSub,     // -% (퍼센트 합연산 감소)
        Multiplicative  // x (최종 곱연산, 보통 축복/디버프 등 독립 계산용)
    }
}