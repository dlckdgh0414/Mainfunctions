// using System;
// using System.Collections.Generic;
// using System.Text;
// using System.Threading.Tasks;
// using Code.Core;
// using Code.Core.Addressable;
// using Code.Core.Bus;
// using Code.Core.Bus.GameEvents.DialogueEvents.Actions;
// using Code.Core.Bus.GameEvents.DialogueEvents.Flow;
// using Code.MainSystem.Dialogue.Parser;
// using Code.MainSystem.Outing;
// using Code.MainSystem.StatSystem.BaseStats;
// using Code.MainSystem.StatSystem.Manager;
// using Cysharp.Threading.Tasks;
// using Member.LS.Code.Dialogue.Character;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Code.MainSystem.Cutscene.DialogCutscene
// {
//     /// <summary>
//     /// DialogCutscene에서 다이얼로그의 시작과 끝, 결과창 띄우기 등을 담당
//     /// </summary>
//     public class DialogCutsceneController : MonoBehaviour
//     {
//         [SerializeField] private DialogCutsceneSenderSO resultSender;
//         [SerializeField] private TextMeshProUGUI resultText;
//         [SerializeField] private Transform uiRoot;
//
//         [FormerlySerializedAs("unitLabel")]
//         [Header("DialogAddressable")]
//         [SerializeField] private string infoLabel = "DialogCharacter";
//         [SerializeField] private string backgroundLabel = "DialogBackground";
//         [SerializeField] private AddressableLoadUI loadingUI;
//
//         // 포메팅용. 스텟 이름은 일반 텍스트. 증가량은 녹색
//         private static readonly string RESULT_FORMAT = "{0} <color=#00FFAC>+{1}</color>";
//         private static readonly string SKILL_RESULT_FORMAT = "<color=#00FFAC>{0}</color> 획득";
//         private static readonly StatType[] MAIN_STAT_PRIORITY =
//         {
//             StatType.Composition,
//             StatType.InstrumentProficiency,
//         };
//         
//         private void Awake()
//         {
//             Bus<DialogueStatUpgradeEvent>.OnEvent += HandleDialogueStatUpgrade;
//             Bus<DialogueGetSkillEvent>.OnEvent += HandleDialogueSkillGet;
//             Bus<DialogueEndEvent>.OnEvent += HandleDialogueEnd;
//         }
//
//         private void OnDestroy()
//         {
//             Bus<DialogueStatUpgradeEvent>.OnEvent -= HandleDialogueStatUpgrade;
//             Bus<DialogueGetSkillEvent>.OnEvent -= HandleDialogueSkillGet;
//             Bus<DialogueEndEvent>.OnEvent -= HandleDialogueEnd;
//         }
//         
//         private async void Start()
//         {
//             resultSender.changeStats.Clear();
//             resultSender.addedTraits.Clear();
//
//             await LoadUnitsAsync();
//             await Awaitable.WaitForSecondsAsync(0.25f);
//
//             DialogueVariableContext variableContext = BuildDialogueVariableContext();
//             Bus<DialogueStartEvent>.Raise(
//                 new DialogueStartEvent(resultSender.selectedEvent, variableContext));
//         }
//
//         private async Task LoadUnitsAsync()
//         {
//             loadingUI.UpdateProgress("Loading Characters...");
//             await GameResourceManager.Instance.LoadAllAsync<CharacterInformationSO>(infoLabel);
//             await GameResourceManager.Instance.LoadAllAsync<Sprite>(backgroundLabel);
//         }
//             
//         public async UniTask PlayOutingSequence()
//         {
//             GameObject resultPrefab =
//                 await GameManager.Instance.LoadAddressableAsync<GameObject>("Outing/UI/Result");
//             GameObject resultInstance = Instantiate(resultPrefab, uiRoot);
//             //OutingResultUI resultUI = resultInstance.GetComponent<OutingResultUI>();
//             resultUI.ShowResultUI();
//             
//             StringBuilder resultBuilder = new StringBuilder();
//             
//             Dictionary<StatType, int> aggregatedStats = new Dictionary<StatType, int>();
//             
//             foreach (StatVariation stat in resultSender.changeStats)
//             {
//                 if (aggregatedStats.ContainsKey(stat.targetStat))
//                 {
//                     aggregatedStats[stat.targetStat] += stat.variation;
//                 }
//                 else
//                 {
//                     aggregatedStats[stat.targetStat] = stat.variation;
//                 }
//             }
//
//             foreach (KeyValuePair<StatType, int> entry in aggregatedStats)
//             {
//                 resultBuilder.Append(string.Format(RESULT_FORMAT, entry.Key.ToString(), entry.Value));
//                 resultBuilder.AppendLine();
//             }
//
//             foreach (TraitVariation skill in resultSender.addedTraits)
//             {
//                 resultBuilder.Append(string.Format(SKILL_RESULT_FORMAT, skill.targetTrait.TraitName));
//             }
//
//             resultText.SetText(resultBuilder.ToString());
//         }
//         
//         private void HandleDialogueStatUpgrade(DialogueStatUpgradeEvent evt)
//         {
//             resultSender.changeStats.Add
//                 (new StatVariation{targetStat = evt.Stat.targetStat, targetMember = evt.Stat.targetMember, variation = evt.Stat.variation});
//         }
//         
//         private void HandleDialogueSkillGet(DialogueGetSkillEvent evt)
//         {
//             resultSender.addedTraits.Add(new TraitVariation{ targetMember = evt.TraitType.targetMember, targetTrait = evt.TraitType.targetTrait});
//         }
//         
//         private void HandleDialogueEnd(DialogueEndEvent evt)
//         {
//             _ = PlayOutingSequence();
//         }
//
//         /// <summary>
//         /// 다이알로그 텍스트 치환에 사용할 변수 컨텍스트 생성
//         /// </summary>
//         /// <returns>세션 변수 컨텍스트</returns>
//         private DialogueVariableContext BuildDialogueVariableContext()
//         {
//             DialogueVariableContext context = new DialogueVariableContext();
//
//             DialoguePlaceholderRegistrar.RegisterCommon(context);
//
//             return context;
//         }
//
//         /// <summary>
//         /// 모든 멤버/스탯 조합의 이름/현재값/증감/미리보기 키 등록
//         /// </summary>
//         /// <param name="context">변수 컨텍스트</param>
//         private void RegisterAllMemberStatVariables(DialogueVariableContext context)
//         {
//             MemberType[] memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));
//             StatType[] statTypes = (StatType[])Enum.GetValues(typeof(StatType));
//
//             foreach (MemberType memberType in memberTypes)
//             {
//                 foreach (StatType statType in statTypes)
//                 {
//                     MemberType capturedMemberType = memberType;
//                     StatType capturedStatType = statType;
//                     string keyPrefix = $"stat.{capturedMemberType}.{capturedStatType}";
//
//                     context.SetGetter($"{keyPrefix}.name", () => GetStatName(capturedMemberType, capturedStatType));
//                     context.SetGetter($"{keyPrefix}.value", () => GetCurrentStatValue(capturedMemberType, capturedStatType));
//                     context.SetGetter($"{keyPrefix}.delta", () => GetAccumulatedDelta(capturedMemberType, capturedStatType));
//                     context.SetGetter($"{keyPrefix}.preview", () => GetCurrentStatValue(capturedMemberType, capturedStatType) + GetAccumulatedDelta(capturedMemberType, capturedStatType));
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 대표 스탯(main) 키 등록
//         /// </summary>
//         /// <param name="context">변수 컨텍스트</param>
//         private void RegisterMainStatVariables(DialogueVariableContext context)
//         {
//             context.SetGetter("stat.main.name", () => GetMainStatName());
//             context.SetGetter("stat.main.value", () => GetMainStatValue());
//             context.SetGetter("stat.main.delta", () => GetMainStatDelta());
//             context.SetGetter("stat.main.preview", () => GetMainStatPreview());
//         }
//
//         /// <summary>
//         /// 대표 스탯 이름 조회
//         /// </summary>
//         /// <returns>대표 스탯 이름</returns>
//         private string GetMainStatName()
//         {
//             if (!TrySelectMainStat(out MemberType memberType, out StatType statType, out _))
//             {
//                 return string.Empty;
//             }
//
//             return GetStatName(memberType, statType);
//         }
//
//         /// <summary>
//         /// 대표 스탯 현재 누적값 조회
//         /// </summary>
//         /// <returns>대표 스탯 현재값</returns>
//         private int GetMainStatValue()
//         {
//             if (!TrySelectMainStat(out MemberType memberType, out StatType statType, out _))
//             {
//                 return 0;
//             }
//
//             return GetCurrentStatValue(memberType, statType);
//         }
//
//         /// <summary>
//         /// 대표 스탯 이번 대화 누적 증감량 조회
//         /// </summary>
//         /// <returns>대표 스탯 누적 증감량</returns>
//         private int GetMainStatDelta()
//         {
//             if (!TrySelectMainStat(out _, out _, out int delta))
//             {
//                 return 0;
//             }
//
//             return delta;
//         }
//
//         /// <summary>
//         /// 대표 스탯 반영 예정값(현재값 + 누적 증감량) 조회
//         /// </summary>
//         /// <returns>대표 스탯 반영 예정값</returns>
//         private int GetMainStatPreview()
//         {
//             if (!TrySelectMainStat(out MemberType memberType, out StatType statType, out int delta))
//             {
//                 return 0;
//             }
//
//             return GetCurrentStatValue(memberType, statType) + delta;
//         }
//
//         /// <summary>
//         /// 대표 스탯 선택
//         /// abs(delta) 최대 기준으로 선택. 동률은 우선순위/enum 순서 기준으로 결정
//         /// </summary>
//         /// <param name="selectedMemberType">선택 멤버</param>
//         /// <param name="selectedStatType">선택 스탯</param>
//         /// <param name="selectedDelta">선택 스탯 누적 증감량</param>
//         /// <returns>대표 스탯 존재 여부</returns>
//         private bool TrySelectMainStat(out MemberType selectedMemberType, out StatType selectedStatType, out int selectedDelta)
//         {
//             selectedMemberType = default;
//             selectedStatType = default;
//             selectedDelta = 0;
//
//             if (resultSender == null || resultSender.changeStats == null || resultSender.changeStats.Count == 0)
//             {
//                 return false;
//             }
//
//             Dictionary<(MemberType memberType, StatType statType), int> aggregatedMap =
//                 new Dictionary<(MemberType memberType, StatType statType), int>();
//
//             foreach (StatVariation statVariation in resultSender.changeStats)
//             {
//                 (MemberType memberType, StatType statType) key = (statVariation.targetMember, statVariation.targetStat);
//
//                 if (aggregatedMap.TryGetValue(key, out int existingDelta))
//                 {
//                     aggregatedMap[key] = existingDelta + statVariation.variation;
//                 }
//                 else
//                 {
//                     aggregatedMap[key] = statVariation.variation;
//                 }
//             }
//
//             bool hasSelection = false;
//             int bestAbsDelta = -1;
//             int bestPriority = int.MaxValue;
//
//             foreach (KeyValuePair<(MemberType memberType, StatType statType), int> pair in aggregatedMap)
//             {
//                 int absDelta = Mathf.Abs(pair.Value);
//                 int priority = GetMainStatPriority(pair.Key.statType);
//
//                 if (!hasSelection
//                     || absDelta > bestAbsDelta
//                     || (absDelta == bestAbsDelta && priority < bestPriority)
//                     || (absDelta == bestAbsDelta && priority == bestPriority && (int)pair.Key.statType < (int)selectedStatType)
//                     || (absDelta == bestAbsDelta && priority == bestPriority && pair.Key.statType == selectedStatType && (int)pair.Key.memberType < (int)selectedMemberType))
//                 {
//                     hasSelection = true;
//                     selectedMemberType = pair.Key.memberType;
//                     selectedStatType = pair.Key.statType;
//                     selectedDelta = pair.Value;
//                     bestAbsDelta = absDelta;
//                     bestPriority = priority;
//                 }
//             }
//
//             return hasSelection;
//         }
//
//         /// <summary>
//         /// 스탯 우선순위 인덱스 조회
//         /// </summary>
//         /// <param name="statType">대상 스탯 타입</param>
//         /// <returns>우선순위 인덱스</returns>
//         private int GetMainStatPriority(StatType statType)
//         {
//             for (int i = 0; i < MAIN_STAT_PRIORITY.Length; i++)
//             {
//                 if (MAIN_STAT_PRIORITY[i] == statType)
//                 {
//                     return i;
//                 }
//             }
//
//             return int.MaxValue;
//         }
//
//         /// <summary>
//         /// 현재 누적 스탯 값 조회
//         /// </summary>
//         /// <param name="memberType">멤버 타입</param>
//         /// <param name="statType">스탯 타입</param>
//         /// <returns>현재 스탯 값</returns>
//         private int GetCurrentStatValue(MemberType memberType, StatType statType)
//         {
//             if (StatManager.Instance == null || !StatManager.Instance.IsInitialized)
//             {
//                 return 0;
//             }
//
//             BaseStat stat = StatManager.Instance.GetMemberStat(memberType, statType);
//             return stat != null ? stat.CurrentValue : 0;
//         }
//
//         /// <summary>
//         /// 스탯 표시명 조회
//         /// </summary>
//         /// <param name="memberType">멤버 타입</param>
//         /// <param name="statType">스탯 타입</param>
//         /// <returns>스탯 표시명</returns>
//         private string GetStatName(MemberType memberType, StatType statType)
//         {
//             if (StatManager.Instance == null || !StatManager.Instance.IsInitialized)
//             {
//                 return statType.ToString();
//             }
//
//             BaseStat stat = StatManager.Instance.GetMemberStat(memberType, statType);
//             if (stat == null)
//             {
//                 return statType.ToString();
//             }
//
//             return stat.StatName;
//         }
//
//         /// <summary>
//         /// 대화 중 누적된 스탯 증감량 조회
//         /// </summary>
//         /// <param name="memberType">멤버 타입</param>
//         /// <param name="statType">스탯 타입</param>
//         /// <returns>누적 증감량</returns>
//         private int GetAccumulatedDelta(MemberType memberType, StatType statType)
//         {
//             if (resultSender == null || resultSender.changeStats == null)
//             {
//                 return 0;
//             }
//
//             int sum = 0;
//             foreach (StatVariation statVariation in resultSender.changeStats)
//             {
//                 if (statVariation.targetMember == memberType && statVariation.targetStat == statType)
//                 {
//                     sum += statVariation.variation;
//                 }
//             }
//
//             return sum;
//         }
//     }
// }
