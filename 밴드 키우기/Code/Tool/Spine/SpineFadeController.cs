using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Code.Tool.Spine
{
    public enum SpineFaceType
    {
        Default,
        Smile,
        Frown,
    }

    [Serializable]
    public struct SpineFace
    {
        public SpineFaceType type; // 표정 타입
        public List<string> slotNames; // 표정 타입에 맞는 파츠들
    }

public class SpineFadeController : MonoBehaviour
    {
        [SerializeField] private List<SpineFace> spineFaces;
        [SerializeField] private List<string> allFaceSlot;
        
        private Skeleton _skeleton;

        private Dictionary<SpineFaceType, List<string>> _realSpineFaceData;
        
        private void Awake()
        {
            _skeleton = GetComponent<SkeletonAnimation>().Skeleton;
            foreach (var faceData in spineFaces)
            {
                _realSpineFaceData.Add(faceData.type, faceData.slotNames);
            }
        }

        public void ChangeFace(SpineFaceType faceType)
        {
            foreach (var slotName in allFaceSlot)
            {
                _skeleton.FindSlot(slotName).Attachment = null;
            }

            foreach (var slotName in _realSpineFaceData[faceType])
            {
                _skeleton.SetAttachment(slotName, slotName);
            }
        }
    }
}