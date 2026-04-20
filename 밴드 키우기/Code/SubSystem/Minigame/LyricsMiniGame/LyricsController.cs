using System;
using System.Collections;
using System.Collections.Generic;
using Chuh007Lib.ObjectPool.RunTime;
using Code.Core;
using Code.Core.Bus.GameEvents.MiniGameEvent;
using Code.MainSystem.NewMainScreen.Data;
using Code.SubSystem.Minigame.Common.Management;
using TMPro;
using UnityEngine;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    [Serializable]
    public class MemberReactionEntry
    {
        public MemberType               type;
        public BackgroundMemberReaction reaction;
    }

    public class LyricsController : BaseMiniGame
    {
        [SerializeField] private List<LyricsDropObjDataSO> dropObjDatas = new List<LyricsDropObjDataSO>();
        [SerializeField] private TextMeshProUGUI            numberText;

        [Header("Pool")]
        [SerializeField] private PoolManagerMono poolManager;
        [SerializeField] private PoolItemSO       dropObjPoolItem;

        [Header("Member Setup")]
        [SerializeField] private LyricsPlayer              lyricsPlayer;
        [SerializeField] private List<MemberReactionEntry> memberReactionMap = new List<MemberReactionEntry>();

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnArea;
        [SerializeField] private float     spawnInterval = 1.2f;
        [SerializeField] private float     fallSpeed     = 3f;
        [SerializeField] private float     spawnXMin     = -3f;
        [SerializeField] private float     spawnXMax     = 3f;

        [Header("Game Rules")]
        [SerializeField] private int targetCount = 15;

        private List<BackgroundMemberReaction> _activeBackgroundMembers = new();
        private List<LyricsDropObj> _activeDropObjects = new();
        private MemberType _mainMemberType;
        private int        _numberCnt;

        private Coroutine _spawnCoroutine;

        protected void Start()
        {
             Init();
            _numberCnt = targetCount;
            SetupMembers();
            UpdateText();
        }

        protected override void HandleStart(StartCountingEndEvent evt)
        {
            base.HandleStart(evt);
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private void SetupMembers()
        {
            _activeBackgroundMembers.Clear();

            if (memberThrowSO == null || memberThrowSO.CurrentMembers.Count == 0)
            {
                Debug.LogWarning("[LyricsController] MemberThrowDataSO가 비어있습니다.");
                return;
            }

            var selected  = memberThrowSO.CurrentMembers;
            int mainIndex = UnityEngine.Random.Range(0, selected.Count);
            _mainMemberType = selected[mainIndex].memberType;

            if (lyricsPlayer != null)
                lyricsPlayer.Initialize(selected[mainIndex]);

            for (int i = 0; i < selected.Count; i++)
            {
                if (i == mainIndex) continue;

                var reaction = GetReactionByType(selected[i].memberType);
                if (reaction != null)
                {
                    reaction.gameObject.SetActive(true);
                    _activeBackgroundMembers.Add(reaction);
                }
            }

            foreach (var entry in memberReactionMap)
            {
                if (entry.reaction != null && !_activeBackgroundMembers.Contains(entry.reaction))
                    entry.reaction.gameObject.SetActive(false);
            }
        }

        private BackgroundMemberReaction GetReactionByType(MemberType type)
        {
            foreach (var entry in memberReactionMap)
                if (entry.type == type) return entry.reaction;
            return null;
        }

        private IEnumerator SpawnRoutine()
        {
            while (_isPlaying)
            {
                SpawnDropObject();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnDropObject()
        {
            if (dropObjDatas == null || dropObjDatas.Count == 0) return;
            if (poolManager     == null)                          return;
            if (dropObjPoolItem == null)                          return;

            var data   = dropObjDatas[UnityEngine.Random.Range(0, dropObjDatas.Count)];
            float x    = UnityEngine.Random.Range(spawnXMin, spawnXMax);
            float y    = spawnArea != null ? spawnArea.position.y : 6f;

            LyricsDropObj obj = poolManager.Pop<LyricsDropObj>(dropObjPoolItem);
            obj.transform.position = new Vector3(x, y, 0f);
            obj.transform.rotation = Quaternion.identity;
            obj.Initialize(data, this, fallSpeed);
            
            _activeDropObjects.Add(obj);
        }

        public void OnDropObjectDestroyed(LyricsDropObj obj)
        {
            _activeDropObjects.Remove(obj);
        }

        public void OnItemCollected(bool isGoodItem)
        {
            if (isGoodItem)
                _numberCnt--;
            else
                _numberCnt++;

            UpdateText();

            if (_activeBackgroundMembers.Count > 0)
            {
                var member = _activeBackgroundMembers[UnityEngine.Random.Range(0, _activeBackgroundMembers.Count)];
                if (isGoodItem) member.ReactToGoodItem();
                else            member.ReactToBadItem();
            }

            if (_numberCnt <= 0)
                GameEnd();
        }

        protected override void GameEnd()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            ClearAllDropObjects();

            base.GameEnd();
        }

        private void ClearAllDropObjects()
        {
            for (int i = _activeDropObjects.Count - 1; i >= 0; i--)
            {
                if (_activeDropObjects[i] != null)
                {
                    poolManager.Push(_activeDropObjects[i]);
                }
            }
            _activeDropObjects.Clear();
        }

        private void UpdateText()
        {
            if (numberText != null)
                numberText.SetText($"남은 갯수 : {_numberCnt}");
        }

        public MemberType GetMainMemberType() => _mainMemberType;

        private void OnDrawGizmos()
        {
            float y = spawnArea != null ? spawnArea.position.y : 6f;
            float z = spawnArea != null ? spawnArea.position.z : 0f;

            Gizmos.color = new Color(1f, 0.9f, 0f, 0.15f);
            Gizmos.DrawCube(new Vector3((spawnXMin + spawnXMax) / 2f, y, z),
                            new Vector3(spawnXMax - spawnXMin, 0.5f, 0f));

            Gizmos.color = new Color(1f, 0.9f, 0f, 1f);
            Gizmos.DrawLine(new Vector3(spawnXMin, y, z), new Vector3(spawnXMax, y, z));
            Gizmos.DrawSphere(new Vector3(spawnXMin, y, z), 0.1f);
            Gizmos.DrawSphere(new Vector3(spawnXMax, y, z), 0.1f);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            for (int i = 0; i < 5; i++)
            {
                float x = Mathf.Lerp(spawnXMin, spawnXMax, i / 4f);
                Gizmos.DrawLine(new Vector3(x, y, z), new Vector3(x, y - 1.5f, z));
            }
        }
    }
}