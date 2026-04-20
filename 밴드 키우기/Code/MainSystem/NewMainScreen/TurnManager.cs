using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance;

        [Header("설정")]
        [SerializeField] private int startYear = 2070;

        private const int DaysPerMonth  = 28;
        private const int DaysPerWeek   = 7;
        private const int MonthsPerYear = 12;
        private const int DaysPerYear   = DaysPerMonth * MonthsPerYear;

        private int _elapsedDays = 280;

        public int TotalDays       => _elapsedDays;
        public int CurrentYear     => startYear + _elapsedDays / DaysPerYear;
        public int CurrentMonth    => _elapsedDays % DaysPerYear / DaysPerMonth + 1;
        public int CurrentDay      => _elapsedDays % DaysPerMonth + 1;
        public int CurrentWeek     => _elapsedDays / DaysPerWeek + 1;
        public bool IsMaxReached   => false;
        public int StartYear       => startYear;
        
        public int DaysUntilYearEnd
        {
            get
            {
                int remain = DaysPerYear - (_elapsedDays % DaysPerYear);
                return remain == DaysPerYear ? 0 : remain;
            }
        }

        public event Action<int> OnWeekEnd;
        public event Action<int> OnMonthEnd;
        public event Action<int> OnNewYear;
        public event Action<int, int, int> OnDateChanged;
        public event Action OnYearEnd;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        public void AdvanceOneWeek()
        {
            int prevMonth = CurrentMonth;
            int prevYear  = CurrentYear;

            _elapsedDays += DaysPerWeek;

            OnWeekEnd?.Invoke(CurrentWeek);

            if (CurrentMonth != prevMonth)
                OnMonthEnd?.Invoke(prevMonth);

            if (CurrentYear != prevYear)
            {
                OnNewYear?.Invoke(CurrentYear);
                OnYearEnd?.Invoke();
            }

            OnDateChanged?.Invoke(CurrentYear, CurrentMonth, CurrentDay);
        }

        public string GetDayOfWeek()
            => KoreanDays[_elapsedDays % DaysPerWeek];

        private static readonly string[] KoreanDays = { "월", "화", "수", "목", "금", "토", "일" };

        public string GetCellPhoneDisplayText()
        {
            DateTime now    = DateTime.Now;
            string   period = now.Hour >= 12 ? "PM" : "AM";
            int      hour12 = now.Hour % 12 == 0 ? 12 : now.Hour % 12;
            return $"{period} : {hour12:D2}:{now.Minute:D2}\n{CurrentYear}년 {CurrentMonth}월 {CurrentDay}일 {GetDayOfWeek()}";
        }

        [ContextMenu("1주 경과 (테스트)")]
        private void DebugAdvanceWeek() => AdvanceOneWeek();

        [ContextMenu("날짜 초기화 (테스트)")]
        private void DebugReset()
        {
            _elapsedDays = 280;
            OnDateChanged?.Invoke(CurrentYear, CurrentMonth, CurrentDay);
        }
    }
}