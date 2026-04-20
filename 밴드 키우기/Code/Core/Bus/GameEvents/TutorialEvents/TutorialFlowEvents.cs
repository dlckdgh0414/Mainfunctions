using Code.Core;
using Code.Core.Bus;

namespace Code.Core.Bus.GameEvents.TutorialEvents
{
    /// <summary>
    /// 튜토리얼 진행 단계 식별자.
    /// </summary>
    public enum TutorialFlowStep
    {
        None = 0,
        Welcome,
        UIOverview_Funds,
        UIOverview_Fans,
        UIOverview_Calendar,
        UIOverview_Members,
        PhoneGuide,
        BandManageGuide,
        MusicConfigured,
        FundsGranted,
        ShopGuide,
        PromotionGuide,
        TreeUnlock,
        ReturnToSelectApp,
        ScheduleManageGuide,
        ActivityGuide,            
        ActivitySelected,
        MemberAssigned,
        ScheduleRegistered,
        StartAllPressed,
        ActivityCompleted,
        PartTimeGuide,
        PartTimeRegistrationPending,
        StartAllPressed_Part2,    
        PartTimeCompleted,
        StatGuide,
        UploadReady,
        UploadCompleted,
        WeekAdvanced,
        Completed,
        ShopHomeGuide,
    }

    /// <summary>
    /// 튜토리얼 관리 탭 구분값.
    /// </summary>
    public enum TutorialManageTabType
    {
        None = 0,
        BandManage = 1, 
        ScheduleManage = 2,
    }

    /// <summary>
    /// 튜토리얼 UI 타깃 식별자.
    /// </summary>
    public enum TutorialTargetId
    {
        None = 0,
        PhoneRoot = 10,
        BandManageButton = 20,
        ScheduleManageButton = 30,
        MusicProductionButton = 40,
        ConcertButton = 50,
        SongButton = 60,
        PartTimeButton = 70,
        RegisterButton = 80,
        StartAllButton = 90,
        UploadButton = 100,
        ShopButton = 110,
        TreeButton = 120,
        PromotionButton = 130,
        MemberListPanel = 140,
        ParticipationSlotPanel = 150,
        CancelButton = 160,
    }

    public struct TutorialStartEvent : IEvent
    {
        
    }
    
    /// <summary>
    /// 튜토리얼 단계 변경 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialStepChangedEvent : IEvent
    {
        public TutorialFlowStep Step;

        public TutorialStepChangedEvent(TutorialFlowStep step)
        {
            Step = step;
        }
    }

    /// <summary>
    /// 튜토리얼 설명 팝업 닫힘 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialPopupClosedEvent : IEvent
    {
        public TutorialFlowStep Step;

        public TutorialPopupClosedEvent(TutorialFlowStep step)
        {
            Step = step;
        }
    }

    /// <summary>
    /// 스마트폰 열림 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialPhoneOpenedEvent : IEvent
    {
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialReturnButtonClickedEvent : IEvent
    {
    }

    /// <summary>
    /// 상점 화면 닫힘 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialShopClosedEvent : IEvent
    {
    }

    public struct TutorialShopPurchasedEvent : IEvent
    {
    }

    /// <summary>
    /// 홍보 화면 닫힘 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialPromotionClosedEvent : IEvent
    {
    }

    /// <summary>
    /// 트리 화면 닫힘 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialTreeClosedEvent : IEvent
    {
    }

    /// <summary>
    /// 관리 탭 열림 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialManageTabOpenedEvent : IEvent
    {
        public TutorialManageTabType TabType;

        public TutorialManageTabOpenedEvent(TutorialManageTabType tabType)
        {
            TabType = tabType;
        }
    }

    /// <summary>
    /// 업로드 가능 상태 진입 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialUploadReadyEvent : IEvent
    {
    }

    /// <summary>
    /// 곡 설정 완료 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialMusicConfiguredEvent : IEvent
    {
        public MusicGenreType GenreType;
        public MusicDirectionType DirectionType;
        public MusicDifficultyType DifficultyType;

        public TutorialMusicConfiguredEvent(
            MusicGenreType genreType,
            MusicDirectionType directionType,
            MusicDifficultyType difficultyType)
        {
            GenreType = genreType;
            DirectionType = directionType;
            DifficultyType = difficultyType;
        }
    }

    /// <summary>
    /// 활동 버튼 선택 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialActivitySelectedEvent : IEvent
    {
        public ManagementBtnType ActivityType;

        public TutorialActivitySelectedEvent(ManagementBtnType activityType)
        {
            ActivityType = activityType;
        }
    }

    /// <summary>
    /// 멤버 슬롯 배치 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialMemberAssignedEvent : IEvent
    {
        public ManagementBtnType ActivityType;
        public MemberType MemberType;
        public bool IsTopSlot;

        public TutorialMemberAssignedEvent(ManagementBtnType activityType, MemberType memberType, bool isTopSlot)
        {
            ActivityType = activityType;
            MemberType = memberType;
            IsTopSlot = isTopSlot;
        }
    }

    /// <summary>
    /// 일정 등록 성공 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialScheduleRegisteredEvent : IEvent
    {
        public ManagementBtnType ActivityType;
        public int MemberCount;

        public TutorialScheduleRegisteredEvent(ManagementBtnType activityType, int memberCount)
        {
            ActivityType = activityType;
            MemberCount = memberCount;
        }
    }

    /// <summary>
    /// 전체 일정 실행 버튼 클릭 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialStartAllPressedEvent : IEvent
    {
        public int ScheduledCount;

        public TutorialStartAllPressedEvent(int scheduledCount)
        {
            ScheduledCount = scheduledCount;
        }
    }

    /// <summary>
    /// 개별 일정 실행 시작 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialScheduleExecutionStartedEvent : IEvent
    {
        public ManagementBtnType ActivityType;
        public int MemberCount;

        public TutorialScheduleExecutionStartedEvent(ManagementBtnType activityType, int memberCount)
        {
            ActivityType = activityType;
            MemberCount = memberCount;
        }
    }

    /// <summary>
    /// Song/Concert 활동 완료 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialActivityCompletedEvent : IEvent
    {
        public ManagementBtnType ActivityType;

        public TutorialActivityCompletedEvent(ManagementBtnType activityType)
        {
            ActivityType = activityType;
        }
    }

    /// <summary>
    /// PartTime 완료 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialPartTimeCompletedEvent : IEvent
    {
        public int RewardGold;
        public int ConditionDelta;

        public TutorialPartTimeCompletedEvent(int rewardGold, int conditionDelta)
        {
            RewardGold = rewardGold;
            ConditionDelta = conditionDelta;
        }
    }

    /// <summary>
    /// 주간 진행 완료 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialWeekAdvancedEvent : IEvent
    {
        public int Year;
        public int Month;
        public int Week;

        public TutorialWeekAdvancedEvent(int year, int month, int week)
        {
            Year = year;
            Month = month;
            Week = week;
        }
    }

    /// <summary>
    /// 업로드 결과 확정 시 발생하는 이벤트.
    /// </summary>
    public struct TutorialUploadCompletedEvent : IEvent
    {
        public int Gold;
        public int Fans;
        public int Exp;

        public TutorialUploadCompletedEvent(int gold, int fans, int exp)
        {
            Gold = gold;
            Fans = fans;
            Exp = exp;
        }
    }
}
