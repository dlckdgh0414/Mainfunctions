using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.CutsceneEvents;
using Code.Core.Bus.GameEvents.EncounterEvents;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.Cutscene.DialogCutscene;
// using Code.MainSystem.Rhythm.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Code.MainSystem.Encounter
{
    [Flags]
    public enum FlagMember
    {
        Guitar = 1, Drums = 2, Bass = 4, Vocal = 8, Piano = 16
    }
    
    /// <summary>
    /// 인카운터를 모아서 가지고 있고, 인카운터의 발생을 컨트롤함
    /// 보니깐 나중에 외출쪽도 싱글턴으로 해야겄네
    /// </summary>
    public class EncounterManager : MonoBehaviour
    {
        [SerializeField] private CurrentEncounterListSO currentNormalEncounterList;
        [SerializeField] private TrainingEndEncounterListSO trainingEndEncounterList;
        [SerializeField] private TeamPracticeEncounterListSO teamPracticeEncounterList;
        
        [SerializeField] private DialogCutsceneSenderSO dialogSender;
        
        // [SerializeField] private RhythmGameDataSenderSO rhythmGameDataSender;
        
        public static EncounterManager Instance;
        
        /// <summary>
        /// 일반적인 인카운터(턴 렌덤, 버스킹과 공연 등)
        /// </summary>
        private Dictionary<EncounterConditionType, List<EncounterDataSO>> _encounterData;
        /// <summary>
        /// 훈련 종료시 인카운터들
        /// </summary>
        //private Dictionary<PersonalpracticeDataSO, List<EncounterDataSO>> _trainingEndData;
        /// <summary>
        /// 합주 종료시 특정 인원 인카운터들
        /// </summary>
        private Dictionary<int, List<EncounterDataSO>> _teamPracticeData;
        
        private void Awake()
        {
            //Bus<TrainingEndEncounterEvent>.OnEvent += HandleTrainingEndEncounter;
            Bus<EncounterCheckEvent>.OnEvent += HandleEncounterCheck;

            #region 딕셔너리 세팅

            _encounterData = new Dictionary<EncounterConditionType, List<EncounterDataSO>>();
            //_trainingEndData = new Dictionary<PersonalpracticeDataSO, List<EncounterDataSO>>();
            _teamPracticeData = new Dictionary<int, List<EncounterDataSO>>();
            
            foreach (EncounterConditionType type in Enum.GetValues(typeof(EncounterConditionType)))
            {
                _encounterData.Add(type, new List<EncounterDataSO>());
            }
            
            foreach (var encounter in currentNormalEncounterList.encounters)
            {
                _encounterData[encounter.type].Add(encounter);
            }

            foreach (var encounter in trainingEndEncounterList.list)
            {
                // if(!_trainingEndData.ContainsKey(encounter.trainingType)) 
                //     _trainingEndData.Add(encounter.trainingType, new List<EncounterDataSO>());
                // _trainingEndData[encounter.trainingType].Add(encounter.encounterData);
            }

            foreach (var encounter in teamPracticeEncounterList.list)
            {
                int sum = (int)encounter.members;
                if(!_teamPracticeData.ContainsKey(sum)) 
                    _teamPracticeData.Add(sum, new List<EncounterDataSO>());
                _teamPracticeData[sum].Add(encounter.encounterData);
            }
            
            #endregion
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                dialogSender.selectedEvent = null;
                dialogSender.addedTraits.Clear();
                dialogSender.changeStats.Clear();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            Bus<EncounterCheckEvent>.OnEvent -= HandleEncounterCheck;
            //Bus<TrainingEndEncounterEvent>.OnEvent -= HandleTrainingEndEncounter;
        }

        // public bool TryTeamPracticeEncounter(List<UnitDataSO> list)
        // {
        //     int sum = 0;
        //     foreach (var unit in list)
        //     {
        //         switch (unit.memberType)
        //         {
        //             case MemberType.Guitar:
        //                 sum += 1;
        //                 break;
        //             case MemberType.Drums:
        //                 sum += 2;
        //                 break;
        //             case MemberType.Bass:
        //                 sum += 4;
        //                 break;
        //             case MemberType.Vocal:
        //                 sum += 8;
        //                 break;
        //             case MemberType.Piano:
        //                 sum += 16;
        //                 break;
        //         }
        //     }
        //     if (!_teamPracticeData.ContainsKey(sum) || _teamPracticeData[sum].Count <= 0)
        //         return false;
        //     
        //     var data = _teamPracticeData[sum][0];
        //     _teamPracticeData[sum].RemoveAt(0);
        //     Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data.dialogue));
        //     
        //     return true;
        // }
        //
        // private void HandleTrainingEndEncounter(TrainingEndEncounterEvent evt)
        // {
        //     if (!_trainingEndData.ContainsKey(evt.TrainingData) || _trainingEndData[evt.TrainingData].Count <= 0) return;
        //     foreach (var data in _trainingEndData[evt.TrainingData])
        //     {
        //         if (Random.Range(0f, 1.0f) <= data.percent)
        //         {
        //             dialogSender.selectedEvent = data.dialogue;
        //             _encounterData[EncounterConditionType.TurnStart].Remove(data);
        //             Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data.dialogue));
        //         }
        //     }
        // }
        
        private void HandleEncounterCheck(EncounterCheckEvent evt)
        {
            switch (evt.Type)
            {
                case EncounterConditionType.BuskingCaseFall:
                {
                    var data = _encounterData[EncounterConditionType.BuskingCaseFall];
            
                    Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data[0].dialogue));
                    break;
                }
                case EncounterConditionType.StatCaseFall:
                {
                    var data = _encounterData[EncounterConditionType.StatCaseFall];
            
                    Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data[0].dialogue));
                    break;
                }
                case EncounterConditionType.TraitsGet:
                {
                    var data = _encounterData[EncounterConditionType.TraitsGet];
                
                    // Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data[0].dialogue));
                    break;
                }
                /*
                case EncounterConditionType.BuskingSuccess or EncounterConditionType.BuskingFall when rhythmGameDataSender.IsSuccess:
                {
                    var data = _encounterData[EncounterConditionType.BuskingSuccess];
                    dialogSender.selectedEvent = data[0].dialogue;
                    Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data[0].dialogue));
                    break;
                }
                case EncounterConditionType.BuskingSuccess or EncounterConditionType.BuskingFall:
                {
                    if(rhythmGameDataSender.IsFailed)
                    {
                        var data = _encounterData[EncounterConditionType.BuskingFall];
                        dialogSender.selectedEvent = data[0].dialogue;
                        Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data[0].dialogue));
                    }

                    break;
                }
                */
                case EncounterConditionType.TurnStart:
                {
                    foreach (var data in _encounterData[EncounterConditionType.TurnStart])
                    {
                        if (Random.Range(0f, 1.0f) <= data.percent)
                        {
                            dialogSender.selectedEvent = data.dialogue;
                            _encounterData[EncounterConditionType.TurnStart].Remove(data);
                            Bus<DialogCutscenePlayEvent>.Raise(new DialogCutscenePlayEvent(data.dialogue));
                            return;
                        }
                    }

                    break;
                }
            }
        }
    }
}