using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Code.MainSystem.NewMainScreen.Alarm
{
    public class AlarmItem
    {
        public string    Title;
        public Func<int> GetTargetDays;
        public bool      RepeatWeekly;   // true면 매주 dismiss 초기화
    }

    public class AlarmNotificationUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject alarmPrefab;
        [SerializeField] private Transform  alarmContainer;

        private readonly List<AlarmItem> _alarms             = new();
        private readonly HashSet<string> _dismissedThisCycle = new();

        private void Awake()
        {
            // 알람 데이터 초기 정의
            _alarms.Add(new AlarmItem
            {
                Title         = "어워드 결과 발표",
                RepeatWeekly  = true,
                GetTargetDays = () =>
                {
                    var tm = TurnManager.Instance;
                    return tm.TotalDays + tm.DaysUntilYearEnd;
                }
            });
        }

        private void OnEnable()
        {
            // UI가 활성화될 때마다 이벤트 구독 및 즉시 갱신
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnWeekEnd += OnWeekPassed;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            // UI가 비활성화될 때 이벤트 해제 (메모리 누수 방지)
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnWeekEnd -= OnWeekPassed;
            }
        }

        private void OnWeekPassed(int week)
        {
            // 주간 반복 알람의 경우 새로운 주가 되면 삭제 기록을 초기화
            _dismissedThisCycle.RemoveWhere(k => k.Contains("_W"));
            RefreshUI();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private string BuildCycleKey(AlarmItem alarm)
        {
            var tm = TurnManager.Instance;
            if (alarm.RepeatWeekly)
            {
                // 주차별 고유 키 생성
                int week = tm.TotalDays / 7;
                return $"{alarm.Title}_Y{tm.CurrentYear}_W{week}";
            }
            return $"{alarm.Title}_{tm.CurrentYear}";
        }

        private void RefreshUI()
        {
            // 기존 UI 프리팹들 제거
            foreach (Transform child in alarmContainer)
                Destroy(child.gameObject);

            if (TurnManager.Instance == null) return;

            int today = TurnManager.Instance.TotalDays;

            foreach (var alarm in _alarms)
            {
                string cycleKey = BuildCycleKey(alarm);
                
                // 이번 주기에 이미 닫은 알람이면 건너뜀
                if (_dismissedThisCycle.Contains(cycleKey)) continue;

                int    dDay     = alarm.GetTargetDays() - today;
                string dDayText = dDay > 0 ? $"D-{dDay}" : dDay == 0 ? "D-Day" : "종료";

                var go = Instantiate(alarmPrefab, alarmContainer);
                var row = go.GetComponent<AlarmRow>();
                
                string     capturedKey = cycleKey;
                GameObject capturedGo  = go;

                row.Setup(alarm.Title, dDayText, () =>
                {
                    _dismissedThisCycle.Add(capturedKey);
                    DismissAlarm(capturedGo);
                });
            }
        }

        private void DismissAlarm(GameObject go)
        {
            // 오른쪽으로 슬라이드하며 사라지는 연출
            go.GetComponent<RectTransform>()
              .DOAnchorPosX(600f, 0.3f)
              .SetEase(Ease.InBack)
              .OnComplete(() => Destroy(go));
        }
    }
}