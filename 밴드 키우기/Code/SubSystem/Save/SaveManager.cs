using System;
using System.IO;
using Code.Core;
using Code.MainSystem.MusicProduction;
using Code.MainSystem.MusicRelated;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.NewMainScreen.Data;
using Code.SubSystem.BandFunds;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.SubSystem.Save
{
    [DefaultExecutionOrder(-200)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string FileName = "gamesave.json";
        private const float DebounceSeconds = 0.5f;

        private string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        private string TempPath => SavePath + ".tmp";

        public GameSaveData Data { get; private set; }
        public bool HasSave => File.Exists(SavePath);

        /// <summary>
        /// 튜토리얼 완료 여부. 미완료 시 자동 저장이 차단됨.
        /// </summary>
        public bool IsTutorialCompleted => Data != null && Data.tutorialCompleted;

        private bool _isDirty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
            DebounceLoop().Forget();
        }

        public void Load()
        {
            if (!File.Exists(SavePath))
            {
                Data = new GameSaveData();
                Debug.Log("[SaveManager] 세이브 파일 없음. 새 데이터 생성.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
                Debug.Log($"[SaveManager] 로드 완료 ← {SavePath} | 튜토완료={Data.tutorialCompleted} | 밴드명={Data.bandName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load 실패: {e}");
                Data = new GameSaveData();
            }
        }

        /// <summary>
        /// 자동 저장 예약. 튜토리얼 미완료 시 무시됨.
        /// </summary>
        public void MarkDirty()
        {
            if (!IsTutorialCompleted)
            {
                // 튜토 중에는 자동 저장 안 함
                return;
            }
            _isDirty = true;
        }

        /// <summary>
        /// 튜토리얼 완료 여부와 상관없이 즉시 저장.
        /// 밴드 이름 저장, 튜토리얼 완료 순간 등 특별한 경우에만 사용.
        /// </summary>
        public void ForceSaveNow()
        {
            SaveNow();
        }

        public void SaveNow()
        {
            if (Data == null) return;

            try
            {
                Data.savedAtUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(Data, true);

                File.WriteAllText(TempPath, json);
                if (File.Exists(SavePath))
                    File.Replace(TempPath, SavePath, null);
                else
                    File.Move(TempPath, SavePath);

                _isDirty = false;
                Debug.Log($"[SaveManager] 저장 완료 → {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 저장 실패: {e}");
            }
        }

        /// <summary>
        /// 튜토리얼 완료 처리 + 즉시 저장. 
        /// 튜토 마지막 단계에서 호출.
        /// </summary>
        public void CompleteTutorial(int resetBandFunds = 2500, int resetBandExp = 0, int resetBandFans = 0, MemberConditionMode resetConditionMode = MemberConditionMode.Commonly)
        {
            if (Data == null) return;

            ResetRuntimeStateForTutorialCompletion(resetBandFunds, resetBandExp, resetBandFans, resetConditionMode);
            Data.tutorialCompleted = true;
            ForceSaveNow();
            Debug.Log("[SaveManager] 튜토리얼 완료 처리됨");
        }

        /// <summary>
        /// 튜토리얼 종료 시 런타임 상태 초기화.
        /// </summary>
        private static void ResetRuntimeStateForTutorialCompletion(
            int resetBandFunds,
            int resetBandExp,
            int resetBandFans,
            MemberConditionMode resetConditionMode)
        {
            if (MusicProductionManager.Instance != null)
            {
                MusicProductionManager.Instance.ResetMusicData();
            }

            if (GameStatManager.Instance != null)
            {
                GameStatManager.Instance.ResetAllForTutorial();
            }

            if (BandSupplyManager.Instance != null)
            {
                BandSupplyManager.Instance.ResetBandSupplyForTutorial(resetBandFunds, resetBandExp, resetBandFans);
            }

            if (TurnManager.Instance != null)
            {
                //TurnManager.Instance.ResetToStartDateForTutorial();
            }

            if (MemberConditionManager.Instance != null)
            {
                MemberConditionManager.Instance.ResetAllConditionsForTutorial(resetConditionMode);
            }

            MemberThrowDataSO[] memberThrowDataAssets = Resources.FindObjectsOfTypeAll<MemberThrowDataSO>();
            int memberThrowDataCount = memberThrowDataAssets.Length;
            for (int i = 0; i < memberThrowDataCount; i++)
            {
                MemberThrowDataSO memberThrowData = memberThrowDataAssets[i];
                if (memberThrowData == null)
                {
                    continue;
                }

                memberThrowData.ClearAll();
                memberThrowData.CleanupCompletedActivity();
                memberThrowData.ClearScheduleQueue();
            }
        }

        private async UniTaskVoid DebounceLoop()
        {
            while (this != null)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(DebounceSeconds));
                if (_isDirty) SaveNow();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && _isDirty && IsTutorialCompleted) SaveNow();
        }

        private void OnApplicationQuit()
        {
            if (_isDirty && IsTutorialCompleted) SaveNow();
        }

        [ContextMenu("세이브 삭제")]
        public void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Data = new GameSaveData();
            Debug.Log("[SaveManager] 세이브 삭제됨");
        }

        [ContextMenu("튜토리얼 완료 처리")]
        private void DebugCompleteTutorial() => CompleteTutorial();

        [ContextMenu("세이브 경로 출력")]
        private void PrintSavePath() => Debug.Log($"[SaveManager] {SavePath}");
    }
}
