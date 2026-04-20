using Code.MainSystem.NewMainScreen.Data;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.Sound;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.Tree.Addon;
using UnityEngine.UI;
using Spine.Unity;

namespace Code.MainSystem.behavior
{
    [Serializable]
    public class BehaviorMember
    {
        public SkeletonGraphic skeletonGraphic;
        public MemberType      type;
        public Transform       spawnPoint;
    }

    public class BehaviorController : BaseBehaviorAddon
    {
        [Header("Data")]
        [SerializeField] private MemberThrowDataSO          memberThrowDataSO;
        [SerializeField] private List<ActivityStatConfigSO> activityConfigs;

        [Header("UI")]
        [SerializeField] private BehaviorUI       behaviorUI;
        [SerializeField] private GameObject       behaviorUIObject;
        [SerializeField] private ActivityResultUI activityResultUI;
        [SerializeField] private Button           skipButton;

        [Header("Projectile")]
        [SerializeField] private StatIconProjectile projectilePrefab;
        [SerializeField] private int               poolSize = 20;

        [Header("Stat Icons")]
        [SerializeField] private Sprite lyricsIcon;
        [SerializeField] private Sprite teamworkIcon;
        [SerializeField] private Sprite proficiencyIcon;
        [SerializeField] private Sprite melodyIcon;

        [Header("Timing")]
        [SerializeField] private float riseUpDuration          = 1.0f;
        [SerializeField] private float delayBetweenProjectiles = 0.2f;
        [SerializeField] private float delayBetweenRounds      = 1.5f;

        [Header("Member Stat Distribution (회차당) — F등급 기준")]
        [SerializeField] private int minStatPerMember = 1;
        [SerializeField] private int maxStatPerMember = 4;

        [Header("회차별 발사 수 스케일링 (1회차=1.0 기준, 이후 급증)")]
        [Tooltip("인덱스 0=1회차, 1=2회차 ... 5=6회차. 초반 억제, 후반 폭발.")]
        [SerializeField] private float[] roundScaleFactors = { 1.0f, 1.0f, 1.5f, 2.5f, 4.0f, 6.5f };
        
        [Header("Sound")]
        [SerializeField] private SoundSO statUpSound;
        [SerializeField] private SoundSO oneMoreSound;
        
        private readonly Queue<StatIconProjectile>              _projectilePool = new();
        private readonly Dictionary<MusicRelatedStatsType, int>    _currentStats   = new();
        private readonly Dictionary<MusicRelatedStatsType, Sprite> _statIcons      = new();

        private int                         _currentRound;
        private bool                        _isRunning;
        private bool                        _skipRequested;
        private MusicRelatedStatsType       _targetMemberStatType;
        private Dictionary<MemberType, int> _memberEarnedSnapshot = new();
        private ManagementBtnType           _currentActivityType;
        private ActivityStatConfigSO        _currentConfig;

        // 크리티컬 추적
        private StatRankType _currentAvgRank;
        private int          _totalCritCount;
        private int          _currentRoundCritCount;

        public event Action OnBehaviorCompleted;
        public event Action OnMusicReadyToUpload;

        private void Awake()
        {
            InitializeProjectilePool();
            InitializeStatIcons();
            EventHandle();

            if (behaviorUIObject != null) behaviorUIObject.SetActive(false);
            if (activityResultUI != null) activityResultUI.OnResultClosed += HandleResultClosed;
            if (skipButton       != null) skipButton.onClick.AddListener(HandleSkipClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (skipButton != null) skipButton.onClick.RemoveAllListeners();
        }

        private void InitializeProjectilePool()
        {
            var fixedDropPoint = behaviorUI.GetFixedDropPoint();
            if (fixedDropPoint == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                var projectile = Instantiate(projectilePrefab, fixedDropPoint);
                var layoutElement = projectile.GetComponent<LayoutElement>()
                    ?? projectile.gameObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;
                projectile.gameObject.SetActive(false);
                _projectilePool.Enqueue(projectile);
            }
        }

        private void InitializeStatIcons()
        {
            _statIcons[MusicRelatedStatsType.Lyrics]      = lyricsIcon;
            _statIcons[MusicRelatedStatsType.Teamwork]    = teamworkIcon;
            _statIcons[MusicRelatedStatsType.Proficiency] = proficiencyIcon;
            _statIcons[MusicRelatedStatsType.Melody]      = melodyIcon;
        }

        public void StartBehavior(ManagementBtnType activityType)
        {
            if (_isRunning) return;

            _currentActivityType = activityType;
            _skipRequested       = false;
            _currentConfig = activityConfigs.FirstOrDefault(c => c.activityType == activityType);
            if (_currentConfig == null)
            {
                Debug.LogError($"[BehaviorController] activityType '{activityType}'에 맞는 ActivityStatConfigSO를 찾을 수 없습니다!");
                return;
            }

            _targetMemberStatType = (activityType == ManagementBtnType.Song)
                ? MusicRelatedStatsType.Composition
                : MusicRelatedStatsType.InstrumentProficiency;

            if (behaviorUIObject != null) behaviorUIObject.SetActive(true);
            if (GameStatManager.Instance != null) GameStatManager.Instance.ResetStats();

            behaviorUI.SetupMembers(GetParticipatingMemberTypes());
            ResetStats();
            behaviorUI.ResetAllStats();

            string headerLabel = activityType switch
            {
                ManagementBtnType.Song    => "가사 제작중",
                ManagementBtnType.Concert => "멜로디 제작중",
                _                         => "활동 진행중"
            };
            behaviorUI.SetHeaderText(headerLabel);

            if (skipButton != null) skipButton.gameObject.SetActive(true);

            _currentRound          = 1;
            _totalCritCount        = 0;
            _currentRoundCritCount = 0;
            _currentAvgRank        = ComputeMedianRank();
            _isRunning             = true;
            StartCoroutine(RunBehaviorRoutine());
        }

        private StatRankType ComputeMedianRank()
        {
            if (memberThrowDataSO.CurrentMembers.Count == 0) return StatRankType.F;
            var ranks = memberThrowDataSO.CurrentMembers
                .Select(m => GameStatManager.Instance.GetMemberStatData(m.memberType, _targetMemberStatType).currentRank)
                .OrderByDescending(r => r).ToList();
            return ranks[ranks.Count / 2];
        }

        private void HandleSkipClicked()
        {
            if (!_isRunning) return;
            _skipRequested = true;
            SkipAllActiveProjectiles();
            ApplySkipStats();
        }

        private void ApplySkipStats()
        {
            if (_currentConfig == null) return;

            int remainingRounds    = SimulateRemainingRounds();
            int totalRoundsToApply = remainingRounds + 1;

            float critChance = _currentConfig.GetCritChance(_currentAvgRank);

            // 랭크 반영 평균 발사 수 계산
            float mult = _currentConfig.GetRankProjectileMultiplier(_currentAvgRank);
            int avgMin = Mathf.Max(1, Mathf.RoundToInt(_currentConfig.minProjectilesPerMember * mult));
            int avgMax = Mathf.Max(avgMin, Mathf.RoundToInt(_currentConfig.maxProjectilesPerMember * mult));
            int avgCount = (avgMin + avgMax) / 2;

            for (int r = 0; r < totalRoundsToApply; r++)
            {
                int totalExpected = 0;
                float rScale = GetRoundScaleFactor(_currentRound + r);
                
                // 보너스 배율 제거됨
                foreach (var _ in memberThrowDataSO.CurrentMembers)
                    totalExpected += Mathf.Max(1, Mathf.RoundToInt(avgCount * rScale));

                int alreadyApplied = 0;
                if (r == 0)
                    foreach (var v in _currentStats.Values) alreadyApplied += v;

                int remaining = Mathf.Max(0, totalExpected - alreadyApplied);

                int roundCrits = 0;
                for (int i = 0; i < remaining; i++)
                {
                    var statType = _currentConfig.GetRandomStat();
                    if (UnityEngine.Random.value < critChance)
                        roundCrits++;

                    _currentStats[statType] += 1;
                    GameStatManager.Instance?.AddScore(1, statType);
                }
                _totalCritCount += roundCrits;
            }

            foreach (var kvp in _currentStats)
                behaviorUI.UpdateStatValue(kvp.Key, kvp.Value);

            _currentRound += remainingRounds;
            behaviorUI.SetRoundText(_currentRound);
        }

        private int SimulateRemainingRounds()
        {
            if (memberThrowDataSO.CurrentMembers.Count == 0) return 0;

            int additional = 0;
            int simRound   = _currentRound;
            while (simRound < ActivityStatConfigSO.MAX_ROUND)
            {
                float chance = _currentConfig.GetContinueChance(_currentAvgRank, simRound);
                if (UnityEngine.Random.Range(0f, 1f) > chance) break;
                additional++;
                simRound++;
            }
            return additional;
        }

        private List<MemberType> GetParticipatingMemberTypes() =>
            memberThrowDataSO.CurrentMembers.Select(m => m.memberType).ToList();

        private void ResetStats()
        {
            _currentStats[MusicRelatedStatsType.Lyrics]      = 0;
            _currentStats[MusicRelatedStatsType.Teamwork]    = 0;
            _currentStats[MusicRelatedStatsType.Proficiency] = 0;
            _currentStats[MusicRelatedStatsType.Melody]      = 0;
        }

        private IEnumerator RunBehaviorRoutine()
        {
            while (_isRunning)
            {
                Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(oneMoreSound));
                if (_skipRequested)
                {
                    _isRunning = false;
                    yield return StartCoroutine(OnBehaviorCompleteRoutine());
                    yield break;
                }

                _currentRoundCritCount = 0;
                behaviorUI.SetRoundText(_currentRound);
                behaviorUI.SetBottomText(_currentConfig.GetRandomWorkingMessage());

                yield return StartCoroutine(FireProjectilesPerMember());

                _totalCritCount += _currentRoundCritCount;

                if (_skipRequested)
                {
                    _isRunning = false;
                    yield return StartCoroutine(OnBehaviorCompleteRoutine());
                    yield break;
                }

                yield return new WaitForSeconds(delayBetweenRounds);

                if (!ShouldContinueToNextRound() || _skipRequested)
                {
                    _isRunning = false;
                    yield return StartCoroutine(OnBehaviorCompleteRoutine());
                }
                else
                {
                    _currentRound++;
                }
            }
        }

        private IEnumerator FireProjectilesPerMember()
        {
            ApplyAllUpgrades();
            ResetUpgradeValue();

            float critChance = _currentConfig.GetCritChance(_currentAvgRank);

            foreach (var member in memberThrowDataSO.CurrentMembers)
            {
                if (_skipRequested) yield break;

                // 랭크 반영 발사 수 + 회차 스케일링
                int count      = _currentConfig.GetRandomProjectilesPerMember(_currentAvgRank);
                float roundScale = GetRoundScaleFactor(_currentRound);
                int finalCount = Mathf.Max(1, Mathf.RoundToInt(count * roundScale));
                
                finalCount    += ActivityEfficiencyPlusValue;

                for (int i = 0; i < finalCount; i++)
                {
                    if (_skipRequested) yield break;

                    if (UnityEngine.Random.value < critChance)
                        _currentRoundCritCount++;

                    Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(statUpSound));
                    behaviorUI.PlayMemberJump(member.memberType);
                    FireProjectileFromMember(member.memberType, _currentConfig.GetRandomStat(), 1);
                    yield return new WaitForSeconds(delayBetweenProjectiles);
                }

                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(riseUpDuration + 0.5f);
        }

        private void FireProjectileFromMember(MemberType memberType, MusicRelatedStatsType statType, int amount)
        {
            if (_projectilePool.Count == 0) return;

            var projectile = _projectilePool.Dequeue();
            projectile.gameObject.SetActive(true);

            var rect = projectile.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta     = new Vector2(100, 100);
                rect.localPosition = Vector2.zero;
            }

            Transform spawnPoint = behaviorUI.GetMemberSpawnPoint(memberType);
            if (spawnPoint != null && rect != null)
            {
                rect.SetParent(spawnPoint, false);
                rect.localPosition = Vector2.zero;
            }

            Vector2 landingOffset = new Vector2(
                UnityEngine.Random.Range(-30f, 30f),
                UnityEngine.Random.Range(-30f, 30f)
            );

            projectile.Initialize(
                _statIcons.GetValueOrDefault(statType),
                Vector2.zero,
                landingOffset,
                statType,
                amount,
                OnProjectileReached,
                behaviorUI.GetTargetTransform(statType),
                ReturnProjectileToPool
            );
        }

        private void OnProjectileReached(MusicRelatedStatsType statType, int amount)
        {
            if (_skipRequested) return;

            _currentStats[statType] += amount;
            behaviorUI.UpdateStatValue(statType, _currentStats[statType]);
            GameStatManager.Instance.AddScore(amount, statType);
        }

        public void ReturnProjectileToPool(StatIconProjectile projectile)
        {
            if (projectile == null) return;

            var fixedDropPoint = behaviorUI.GetFixedDropPoint();
            var rect = projectile.GetComponent<RectTransform>();
            if (rect != null && fixedDropPoint != null)
            {
                rect.SetParent(fixedDropPoint, false);
                rect.localPosition = Vector2.zero;
            }

            projectile.ResetProjectile();
            _projectilePool.Enqueue(projectile);
        }

        private void SkipAllActiveProjectiles()
        {
            var allProjectiles = FindObjectsByType<StatIconProjectile>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var p in allProjectiles)
            {
                if (p.gameObject.activeSelf)
                    p.Skip();
            }
        }

        private float GetRoundScaleFactor(int round)
        {
            int idx = Mathf.Clamp(round - 1, 0, roundScaleFactors.Length - 1);
            return roundScaleFactors[idx];
        }

        private bool ShouldContinueToNextRound()
        {
            if (memberThrowDataSO.CurrentMembers.Count == 0) return false;

            ApplyAllUpgrades();
            float chance = _currentConfig.GetContinueChance(_currentAvgRank, _currentRound) + OneMoreChancePlusValue;
            ResetUpgradeValue();
            return UnityEngine.Random.Range(0f, 1f) <= chance;
        }

        private IEnumerator OnBehaviorCompleteRoutine()
        {
            _skipRequested = false;
            if (skipButton != null) skipButton.gameObject.SetActive(false);

            if (GameStatManager.Instance != null)
            {
                int memberCount = memberThrowDataSO.CurrentMembers.Count;
                GameStatManager.Instance.CompleteActivity(memberCount);

                // 멤버 스탯 — 랭크 배수 반영 (크리티컬 보너스 배율 제거됨)
                float mult = _currentConfig.GetRankProjectileMultiplier(_currentAvgRank);

                foreach (var member in memberThrowDataSO.CurrentMembers)
                {
                    int totalEarned = 0;
                    for (int r = 0; r < _currentRound; r++)
                    {
                        float rScale = GetRoundScaleFactor(r + 1);
                        int earned = Mathf.Max(1, Mathf.RoundToInt(
                            UnityEngine.Random.Range(minStatPerMember, maxStatPerMember + 1) * rScale));
                        totalEarned += earned;
                    }

                    // 랭크 배수만 반영
                    int finalEarned = Mathf.RoundToInt(totalEarned * mult);
                    GameStatManager.Instance.AddMemberStat(member.memberType, finalEarned);
                }

                _memberEarnedSnapshot = new Dictionary<MemberType, int>(
                    GameStatManager.Instance.GetAllMemberEarnedStats());
                GameStatManager.Instance.ApplyAllMemberStats(_targetMemberStatType);

                behaviorUI.SetBottomText("제작 완료");
            }

            yield return new WaitForSeconds(1.0f);

            string resultLabel = _currentActivityType switch
            {
                ManagementBtnType.Song    => "가사 제작 결과",
                ManagementBtnType.Concert => "멜로디 제작 결과",
                _                         => "활동 결과"
            };
            behaviorUI.SetHeaderText(resultLabel);

            if (activityResultUI != null && GameStatManager.Instance != null)
            {
                activityResultUI.Show(
                    new List<MemberDataSO>(memberThrowDataSO.CurrentMembers),
                    GameStatManager.Instance.GetAllStats(),
                    GameStatManager.Instance.GetMusicPerfectionPercent(),
                    (StatType)_targetMemberStatType,
                    _memberEarnedSnapshot);
            }
            else
            {
                HideBehaviorUI();
                FinishActivity();
            }
        }

        private void HandleResultClosed()
        {
            HideBehaviorUI();
            FinishActivity();
        }

        private void HideBehaviorUI()
        {
            if (behaviorUIObject != null) behaviorUIObject.SetActive(false);
        }

        private void FinishActivity()
        {
            ManagementBtnType? completedActivityType = memberThrowDataSO.RunningActivity;
            memberThrowDataSO.CleanupCompletedActivity();

            if (completedActivityType != null)
            {
                Bus<TutorialActivityCompletedEvent>.Raise(
                    new TutorialActivityCompletedEvent(completedActivityType.Value));
            }

            OnBehaviorCompleted?.Invoke();

            if (GameStatManager.Instance != null &&
                GameStatManager.Instance.GetMusicPerfectionPercent() >= 100 &&
                !memberThrowDataSO.HasPendingSchedule)
            {
                Debug.Log("[BehaviorController] 음악 완성! 업로드 버튼 활성화");
                OnMusicReadyToUpload?.Invoke();
            }
        }
    }
}