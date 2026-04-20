using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.behavior;
using Code.MainSystem.MusicRelated;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.EventManager.Upgarde
{
    public class PlayEvent : MonoBehaviour
    {
        [SerializeField] private Image memberIcon;
        [SerializeField] private Image gaugeImage;
        [SerializeField] private GameObject resultObject;
        [SerializeField] private GameObject startObject;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private float fillDuration = 2f;

        [Header("스탯 텍스트")]
        [SerializeField] private TextMeshProUGUI statValueText;
        [SerializeField] private TextMeshProUGUI statNameText;

        [Header("실패 시 스탯 감소")]
        [SerializeField] private int minStatLose = 3;
        [SerializeField] private int maxStatLose = 10;
        [SerializeField] private float statDecreaseInterval = 0.1f;
        [SerializeField] private int statDecreaseSteps = 10;

        [Header("Projectile")]
        [SerializeField] private StatIconProjectile projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private Transform statTargetPoint;
        [SerializeField] private int poolSize = 5;
        [SerializeField] private int projectileCount = 5;
        [SerializeField] private float delayBetweenProjectiles = 0.15f;

        [Header("스탯 상승량")]
        [SerializeField] private int minStatGain = 5;
        [SerializeField] private int maxStatGain = 20;

        [Header("결과 UI")]
        [SerializeField] private EndEventUUI endEventUI;

        private int _successRate;
        private Sprite _sadSprite;
        private Sprite _happySprite;
        private Sprite _memberIconSprite;
        private Sprite _statIcon;
        private MusicRelatedStatsType _targetStat;
        private readonly Queue<StatIconProjectile> _pool = new();
        private int _arrivedCount;

        private void Awake()
        {
            InitPool();
        }

        private void InitPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var p = Instantiate(projectilePrefab, projectileSpawnPoint);
                p.gameObject.SetActive(false);
                _pool.Enqueue(p);
            }
        }

        public void Setup(Sprite icon, Sprite sadIcon, Sprite happyIcon,
            Sprite statIcon, MusicRelatedStatsType targetStat,
            int successRate, Action<bool, int> onComplete = null, Action onShowEnd = null)
        {
            _sadSprite = sadIcon;
            _happySprite = happyIcon;
            _memberIconSprite = icon;
            _statIcon = statIcon;
            _targetStat = targetStat;
            _successRate = successRate;
            startObject.gameObject.SetActive(true);
            memberIcon.sprite = icon;
            resultObject.SetActive(false);

            gaugeImage.rectTransform.localScale = Vector3.zero;

            progressText.text = "진행 중...";
            progressText.gameObject.SetActive(true);

            int currentScore = GameStatManager.Instance.GetScore(targetStat);
            statValueText.text = currentScore.ToString();
            if (statNameText != null)
                statNameText.text = targetStat.ToString();

            gameObject.SetActive(true);
            PlayRoutineAsync(onComplete, onShowEnd).Forget();
        }

        private async UniTaskVoid PlayRoutineAsync(Action<bool, int> onComplete, Action onShowEnd)
        {
            float elapsed = 0f;
            while (elapsed < fillDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fillDuration);
                gaugeImage.rectTransform.localScale = new Vector3(t, 1f, 1f);
                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
            }
            gaugeImage.rectTransform.localScale = Vector3.one;

            bool success = UnityEngine.Random.Range(0, 100) < _successRate;
            int statChange = 0;

            if (success)
            {
                statChange = UnityEngine.Random.Range(minStatGain, maxStatGain + 1);
                memberIcon.sprite = _happySprite;
                progressText.text = "성공!";
                await UniTask.WaitForSeconds(0.8f, cancellationToken: destroyCancellationToken);
                await FireProjectilesAsync(statChange);
            }
            else
            {
                statChange = UnityEngine.Random.Range(minStatLose, maxStatLose + 1);
                memberIcon.sprite = _sadSprite;
                progressText.text = "실패...";
                await UniTask.WaitForSeconds(0.8f, cancellationToken: destroyCancellationToken);
                await FailRoutineAsync(statChange);
            }

            if (endEventUI != null)
            {
                onShowEnd?.Invoke();
                var tcs = new UniTaskCompletionSource();
                endEventUI.Show(_memberIconSprite, success, () => tcs.TrySetResult());
                await tcs.Task;
            }

            onComplete?.Invoke(success, statChange);
        }

        private async UniTask FailRoutineAsync(int loseAmount)
        {
            startObject.gameObject.SetActive(false);
            resultObject.SetActive(true);

            int currentScore = GameStatManager.Instance.GetScore(_targetStat);
            int targetScore = Mathf.Max(0, currentScore - loseAmount);
            float displayValue = currentScore;
            float decreasePerStep = (float)(currentScore - targetScore) / statDecreaseSteps;

            Color originalColor = statValueText.color;
            statValueText.color = Color.red;

            for (int i = 0; i < statDecreaseSteps; i++)
            {
                displayValue -= decreasePerStep;
                statValueText.text = Mathf.RoundToInt(displayValue).ToString();

                statValueText.transform.DOKill();
                statValueText.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 1, 0.5f);

                await UniTask.WaitForSeconds(statDecreaseInterval, cancellationToken: destroyCancellationToken);
            }

            statValueText.text = targetScore.ToString();
            GameStatManager.Instance.AddScore(-loseAmount, _targetStat);

            statValueText.DOKill();
            statValueText.DOColor(originalColor, 0.3f);

            await UniTask.WaitForSeconds(1f, cancellationToken: destroyCancellationToken);
        }

        private async UniTask FireProjectilesAsync(int totalGain)
        {
            startObject.gameObject.SetActive(false);
            resultObject.SetActive(true);

            _arrivedCount = 0;
            int perProjectile = Mathf.Max(1, totalGain / projectileCount);

            for (int i = 0; i < projectileCount; i++)
            {
                if (_pool.Count == 0) break;
                var projectile = _pool.Dequeue();
                projectile.gameObject.SetActive(true);

                var rect = projectile.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(80, 80);
                    rect.localPosition = Vector2.zero;
                }

                Vector2 landingOffset = new Vector2(
                    UnityEngine.Random.Range(-20f, 20f),
                    UnityEngine.Random.Range(-20f, 20f)
                );

                projectile.Initialize(
                    _statIcon,
                    Vector2.zero,
                    landingOffset,
                    _targetStat,
                    perProjectile,
                    OnProjectileReached,
                    statTargetPoint,
                    ReturnToPool
                );

                await UniTask.WaitForSeconds(delayBetweenProjectiles, cancellationToken: destroyCancellationToken);
            }

            await UniTask.WaitUntil(() => _arrivedCount >= projectileCount, cancellationToken: destroyCancellationToken);
        }

        private void OnProjectileReached(MusicRelatedStatsType statType, int amount)
        {
            _arrivedCount++;
            GameStatManager.Instance.AddScore(amount, statType);
            int newScore = GameStatManager.Instance.GetScore(statType);
            statValueText.text = newScore.ToString();
            statValueText.transform.DOKill();
            statValueText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0.5f);
        }

        private void ReturnToPool(StatIconProjectile projectile)
        {
            if (projectile == null) return;
            var rect = projectile.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.SetParent(projectileSpawnPoint, false);
                rect.localPosition = Vector2.zero;
            }
            projectile.ResetProjectile();
            _pool.Enqueue(projectile);
        }
    }
}