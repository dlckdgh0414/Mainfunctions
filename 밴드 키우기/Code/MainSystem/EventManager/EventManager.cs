using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.MainSystem.EventManager.Upgarde;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.Sound;
using Code.MainSystem.Tree.Addon;
using UnityEngine;

namespace Code.MainSystem.EventManager
{
    public class EventManager : BaseEventAddon
    {
        [SerializeField, Range(0f, 100f)] private float turnEndEventChance = 30f;
        [SerializeField] private EventMainUI eventMainUI;
        [SerializeField] private TextEventUI eventUITextUI;
        
        private bool _isPlayUpgardeEvent = false;
        public static EventManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                EventHandle();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            TurnManager.Instance.OnWeekEnd += HandleWeekEnd;
            Bus<ShowTextEvent>.OnEvent += HandleShowTextEvent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnWeekEnd -= HandleWeekEnd;
            Bus<ShowTextEvent>.OnEvent -= HandleShowTextEvent;
        }

        public void ResetManager()
        {
            _isPlayUpgardeEvent = false;
        }
        
        private void HandleShowTextEvent(ShowTextEvent evt)
        {
            eventUITextUI.ShowEvent(evt.EventText, evt.EventAmount);
        }

        private void HandleWeekEnd(int currentWeek)
        {
            if (!IsEventActive) return;
            if (Random.Range(0f, 100f) <= turnEndEventChance && _isPlayUpgardeEvent == false)
            {
                _isPlayUpgardeEvent = true;
                eventMainUI.SetUPUI();
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Force Trigger Event")]
        private void ForceTriggerEvent()
        {
            if (_isPlayUpgardeEvent)
            {
                Debug.LogWarning("[EventManager] 이미 이벤트 진행 중");
                return;
            }
            _isPlayUpgardeEvent = true;
            eventMainUI.SetUPUI();
            Debug.Log("[EventManager] 이벤트 강제 실행");
        }

        [ContextMenu("Reset Event State")]
        private void ResetEventState()
        {
            ResetManager();
            Debug.Log("[EventManager] 이벤트 상태 초기화");
        }
#endif
    }
}