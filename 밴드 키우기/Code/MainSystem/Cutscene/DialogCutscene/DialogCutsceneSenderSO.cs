using System;
using System.Collections.Generic;
using Code.Core;
using Code.MainSystem.Dialogue;
using Code.MainSystem.StatSystem.BaseStats;
using Code.MainSystem.TraitSystem.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.MainSystem.Cutscene.DialogCutscene
{
    [Serializable]
    public struct StatVariation
    {
        public MemberType targetMember;
        public StatType targetStat;
        public int variation;
    }
    
    [Serializable]
    public struct TraitVariation
    {
        public MemberType targetMember;
        public TraitDataSO targetTrait;
    }
    
    // 외출, 인카운터 등 모두 병합한 sender
    [CreateAssetMenu(fileName = "DialogCutsceneDataSender", menuName = "SO/CutScene/DataSender", order = 0)]
    public class DialogCutsceneSenderSO : ScriptableObject
    {
        [Header("Outing Input Data")]
        public DialogueInformationSO selectedEvent; // 기획상 턴 시작시에 어떤 이벤트 발생할지 정해놓고 한다 함.

        [Header("Game Result Data")]
        public List<StatVariation> changeStats; // 스텟 변경
        public List<TraitVariation> addedTraits; // 추가 특성
    }
}