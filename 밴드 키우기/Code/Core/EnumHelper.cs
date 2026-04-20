namespace Code.Core
{
    public enum PracticenType
    {
        Personal,
        Team
    }
    
    public enum MemberType
    {
        Guitar, Drums, Bass, Vocal, Piano
    }
    
    public enum ManagementBtnType
    {
        Concert,
        Shop,
        Song,
        Tree,
        PartTime,
        Promotion,
        MusicUpload,
        None
    }

    public enum MusicDifficultyType
    {
        Easy,
        Normal,
        Hard
    }
    
    public enum ShopItemEffectType
    {
        MemberStatIncrease,      // 개인 스탯 증가
        ActivityEfficiencyBonus, // 행동 효율 증가
        ConditionRecovery        // 컨디션 회복
    }
    
    public enum ShopItemGrade
    {
        Low,    // 하
        Mid,    // 중
        High    // 상
    }

    public enum MusicGenreType
    {
        Rock,
        PopRock,
        AlternativeRock,
        HardRock,
        PunkRock,
        HeavyMetal,
        IndieRock,
        Funk
    }

    public enum MusicDirectionType
    {
        Friendliness,
        Maniac,
        Simplicity,
        Complicacy,
        Bright,
        Gloominess,
        Stable,
        Experimental
    }

    public enum MemberConditionMode
    {
        VeryGood,
        Good,
        Commonly,
        Bad,
        VeryBad,
    }

    public enum TextEventType
    {
        MusicProduction,
    }

    public enum MusicRelatedStatsType
    {
        MusicPerfection,        // 음악 완성도(하위 4개 항목의 합)
        Lyrics,                 // 가사
        Teamwork,               // 화합(합주)
        Proficiency,            // 숙련도(합주)
        Melody,                 // 멜로디
        Composition,            //작곡감
        InstrumentProficiency   //악기 숙련도
        
    }

    public enum MusicProductionUnionType
    {
        Good,
        Commonly,
        Bad,
    }

    public enum LocationType
    {
        Downtown,
        Park,
        AcademyDistrict,
        LiveHouse,
        MusicStore
    }

    public enum SystemMessageIconType
    {
        Warning,
    }
    
    /// <summary>
    /// 무언가 수치에 변화가 생기면, 그 변화가 어디에서 왔는지
    /// </summary>
    public enum ChangeFrom
    {
        Outing,
        MiniGame,
    }
}
