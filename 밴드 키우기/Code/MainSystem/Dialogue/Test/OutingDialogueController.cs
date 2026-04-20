// using System.Collections.Generic;
// using Code.Core.Bus;
// using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
// using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
// using Code.Core.Bus.GameEvents.SystemEvents;
// using Code.MainSystem.NewMainScreen.Data;
// using Code.MainSystem.Dialogue.Parser;
// using Code.MainSystem.StatSystem.BaseStats;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
//
// namespace Code.MainSystem.Dialogue
// {
//     /// <summary>
//     /// 외출 씬에서 멤버별 다이알로그를 순차적으로 실행하는 컨트롤러.
//     ///
//     /// 동작 흐름:
//     ///   1. OutingDialogueDataSO를 Addressables로 먼저 로드
//     ///   2. OutingMemberDataSO의 멤버 목록을 Queue에 적재
//     ///   3. 멤버별로 (MemberType + LocationType) 조합에 맞는 AssetReference를 랜덤 선택
//     ///   4. Addressables로 DialogueInformationSO 로딩 후 DialogueStartEvent 발행
//     ///   5. DialogueEndEvent 수신 시 로딩된 에셋 해제 후 다음 멤버로 이동
//     ///   6. 모든 멤버 완료 후 returnScene으로 페이드 전환
//     /// </summary>
//     public class OutingDialogueController : MonoBehaviour
//     {
//         [Header("외출 데이터")]
//         [SerializeField] private OutingMemberDataSO outingMemberDataSO;
//
//         [Header("다이알로그 데이터베이스")]
//         [SerializeField] private AssetReferenceT<OutingDialogueDataSO> outingDialogueDataRef;
//
//         [Header("복귀 씬 이름")]
//         [SerializeField] private string returnScene;
//
//         [Header("씬 전환 페이드 시간")]
//         [SerializeField] private float fadeDuration = 1f;
//
//         private Queue<OutingMemberData> _memberQueue = new();
//         private bool _isPlaying = false;
//
//         private AsyncOperationHandle<OutingDialogueDataSO> _databaseHandle;
//         private AsyncOperationHandle<DialogueInformationSO> _currentHandle;
//
//         private OutingDialogueDataSO _outingDialogueDataSO;
//         
//
//         private void OnEnable()
//         {
//             Bus<DialogueEndEvent>.OnEvent += OnDialogueEnd;
//         }
//
//         private void OnDisable()
//         {
//             Bus<DialogueEndEvent>.OnEvent -= OnDialogueEnd;
//             ReleaseCurrentHandle();
//             ReleaseDatabaseHandle();
//         }
//
//         private void Start()
//         {
//             if (outingMemberDataSO == null)
//             {
//                 Debug.LogError("[OutingDialogueController] OutingMemberDataSO가 연결되지 않았습니다.");
//                 return;
//             }
//
//             if (outingDialogueDataRef == null || !outingDialogueDataRef.RuntimeKeyIsValid())
//             {
//                 Debug.LogError("[OutingDialogueController] OutingDialogueDataSO AssetReference가 유효하지 않습니다.");
//                 return;
//             }
//
//             InitializeAsync().Forget();
//         }
//         
//
//         private async UniTaskVoid InitializeAsync()
//         {
//             _databaseHandle = outingDialogueDataRef.LoadAssetAsync<OutingDialogueDataSO>();
//             await _databaseHandle.Task;
//
//             if (_databaseHandle.Status != AsyncOperationStatus.Succeeded)
//             {
//                 Debug.LogError("[OutingDialogueController] OutingDialogueDataSO 로드 실패.");
//                 return;
//             }
//
//             _outingDialogueDataSO = _databaseHandle.Result;
//
//             if (outingMemberDataSO.outingMemberDatas == null || outingMemberDataSO.outingMemberDatas.Count == 0)
//             {
//                 Debug.LogWarning("[OutingDialogueController] 외출 멤버 데이터가 비어 있습니다. 바로 복귀합니다.");
//                 ReturnToOriginalScene();
//                 return;
//             }
//
//             _memberQueue.Clear();
//             foreach (OutingMemberData memberData in outingMemberDataSO.outingMemberDatas)
//                 _memberQueue.Enqueue(memberData);
//
//             await PlayNextMemberAsync();
//         }
//         
//
//         /// <summary>
//         /// 큐에서 다음 멤버를 꺼내 AssetReference를 로드한 뒤 다이알로그 실행.
//         /// 큐가 비어 있으면 원래 씬으로 복귀.
//         /// </summary>
//         private async UniTask PlayNextMemberAsync()
//         {
//             ReleaseCurrentHandle();
//
//             if (_memberQueue.Count == 0)
//             {
//                 ReturnToOriginalScene();
//                 return;
//             }
//
//             OutingMemberData memberData = _memberQueue.Dequeue();
//             AssetReference assetRef = _outingDialogueDataSO.GetRandom(memberData.type, memberData.outingType);
//
//             if (assetRef == null)
//             {
//                 Debug.LogWarning($"[OutingDialogueController] {memberData.type} / {memberData.outingType} 다이알로그 없음 → 건너뜀");
//                 await PlayNextMemberAsync();
//                 return;
//             }
//
//             _currentHandle = assetRef.LoadAssetAsync<DialogueInformationSO>();
//             await _currentHandle.Task;
//
//             if (_currentHandle.Status != AsyncOperationStatus.Succeeded)
//             {
//                 Debug.LogError($"[OutingDialogueController] DialogueInformationSO 로드 실패: {assetRef.RuntimeKey}");
//                 await PlayNextMemberAsync();
//                 return;
//             }
//
//             _isPlaying = true;
//             DialogueVariableContext variableContext = BuildDialogueVariableContext(memberData);
//             Bus<DialogueStartEvent>.Raise(new DialogueStartEvent(_currentHandle.Result, variableContext));
//         }
//
//         /// <summary>
//         /// 외출 다이알로그에서 사용할 세션 변수 컨텍스트 생성
//         /// </summary>
//         /// <param name="memberData">현재 외출 멤버 데이터</param>
//         /// <returns>치환용 변수 컨텍스트</returns>
//         private DialogueVariableContext BuildDialogueVariableContext(OutingMemberData memberData)
//         {
//             DialogueVariableContext context = new DialogueVariableContext();
//             context.SetValue("outing.memberType", memberData.type.ToString());
//             context.SetValue("outing.locationType", memberData.outingType.ToString());
//             
//             RegisterAllMemberStatVariables(context);
//             RegisterGoldVariables(context);
//             
//             return context;
//         }
//
//         private void RegisterAllMemberStatVariables(DialogueVariableContext context)
//         {
//             Code.MainSystem.StatSystem.Manager.MemberType[] memberTypes = (Code.MainSystem.StatSystem.Manager.MemberType[])System.Enum.GetValues(typeof(Code.MainSystem.StatSystem.Manager.MemberType));
//             Code.MainSystem.StatSystem.BaseStats.StatType[] statTypes = (Code.MainSystem.StatSystem.BaseStats.StatType[])System.Enum.GetValues(typeof(Code.MainSystem.StatSystem.BaseStats.StatType));
//
//             foreach (Code.MainSystem.StatSystem.Manager.MemberType memberType in memberTypes)
//             {
//                 foreach (StatType statType in statTypes)
//                 {
//                     Code.MainSystem.StatSystem.Manager.MemberType capturedMemberType = memberType;
//                     StatType capturedStatType = statType;
//                     string keyPrefix = $"stat.{capturedMemberType}.{capturedStatType}";
//
//                     context.SetGetter($"{keyPrefix}.name", () => GetStatName(capturedMemberType, capturedStatType));
//                     // TODO: 연출용 델타 추적 기능이 외출 씬 전체에서 필요하다면 누적 추적 로직 추가 필요.
//                     // 현재는 단순 변수 치환(이름)용도로만 기본 등록.
//                 }
//             }
//         }
//
//         private void RegisterGoldVariables(DialogueVariableContext context)
//         {
//             // 골드 증가량을 추적/표시하기 위한 변수
//             // Outing 쪽에도 누적 재화를 표시하려면 별도의 Tracker가 필요할 수 있으나,
//             // 일단 CSV에서 {gold.delta} 등으로 사용할 수 있게 틀만 마련해둡니다.
//             // context.SetGetter("gold.delta", () => GetAccumulatedGoldDelta());
//         }
//
//         private string GetStatName(Code.MainSystem.StatSystem.Manager.MemberType memberType, Code.MainSystem.StatSystem.BaseStats.StatType statType)
//         {
//             if (Code.MainSystem.StatSystem.Manager.StatManager.Instance == null || !Code.MainSystem.StatSystem.Manager.StatManager.Instance.IsInitialized)
//             {
//                 return statType.ToString();
//             }
//
//             BaseStat stat = Code.MainSystem.StatSystem.Manager.StatManager.Instance.GetMemberStat(memberType, statType);
//             return stat != null ? stat.StatName : statType.ToString();
//         }
//
//         /// <summary>
//         /// DialogueManager로부터 다이알로그 종료 이벤트 수신 시 호출.
//         /// </summary>
//         private void OnDialogueEnd(DialogueEndEvent evt)
//         {
//             if (!_isPlaying) return;
//             _isPlaying = false;
//
//             PlayNextMemberAsync().Forget();
//         }
//
//         /// <summary>
//         /// 모든 다이알로그가 끝난 뒤 원래 씬으로 페이드 전환.
//         /// </summary>
//         private void ReturnToOriginalScene()
//         {
//             if (string.IsNullOrEmpty(returnScene))
//             {
//                 Debug.LogError("[OutingDialogueController] returnScene이 설정되지 않았습니다.");
//                 return;
//             }
//
//             Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(returnScene, fadeDuration));
//         }
//         
//
//         private void ReleaseCurrentHandle()
//         {
//             if (!_currentHandle.IsValid()) return;
//
//             if (_currentHandle.IsDone)
//                 Addressables.Release(_currentHandle);
//             else
//             {
//                 AsyncOperationHandle<DialogueInformationSO> h = _currentHandle;
//                 h.Completed += handle => { if (handle.IsValid()) Addressables.Release(handle); };
//             }
//
//             _currentHandle = default;
//         }
//
//         private void ReleaseDatabaseHandle()
//         {
//             if (!_databaseHandle.IsValid()) return;
//
//             if (_databaseHandle.IsDone)
//                 Addressables.Release(_databaseHandle);
//             else
//             {
//                 AsyncOperationHandle<OutingDialogueDataSO> h = _databaseHandle;
//                 h.Completed += handle => { if (handle.IsValid()) Addressables.Release(handle); };
//             }
//
//             _databaseHandle = default;
//             _outingDialogueDataSO = null;
//         }
//     }
// }
