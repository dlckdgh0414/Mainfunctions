using System;
using System.Collections.Generic;
using Code.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Song.UI
{
    /// <summary>
    /// Upload UI의 상단 부분.
    /// 지금 단계의 아이콘이 커지고, 라인은 지나온 것들만 켜진다.
    /// 라인을 왼쪽으로 그리기, 0번은 안그림.
    /// </summary>
    public class UploadTopBar : MonoBehaviour
    {
        [SerializeField] private List<UILineRenderer> uiLines = new List<UILineRenderer>();

        [SerializeField] private List<RectTransform> partIcons = new List<RectTransform>();

        [Header("UI Settings")]
        [SerializeField] private float partIconBigSize;
        [SerializeField] private float partIconSmallSize;
        
        private void Start()
        {
            UpdateCurIndex(0);
        }
        
        public void UpdateCurIndex(int idx)
        {
            Canvas.ForceUpdateCanvases(); // UI 위치를 강제로 즉시 반영한다.
            partIcons[0].sizeDelta = idx == 0
                ? new Vector2(partIconBigSize, partIconBigSize)
                : new Vector2(partIconSmallSize, partIconSmallSize);
            
            for (int i = 1; i < partIcons.Count; i++)
            {
                partIcons[i].sizeDelta = i == idx
                    ? new Vector2(partIconBigSize, partIconBigSize)
                    : new Vector2(partIconSmallSize, partIconSmallSize);
                
                if (i > idx)
                {
                    uiLines[i].thickness = 5;
                    uiLines[i].lineColor = Color.gray;
                }
                else
                {
                    uiLines[i].thickness = 10;
                    uiLines[i].lineColor = Color.white;
                }
                float correction = idx == i - 1 ? 50 : 37;
                Vector2 start = partIcons[i].transform.InverseTransformPoint(partIcons[i].transform.position + Vector3.left * 25);
                Vector2 end = partIcons[i].transform.InverseTransformPoint(partIcons[i - 1].transform.position + Vector3.right * correction);
                
                uiLines[i].SetLinePositions(start, end);

            }
        }
    }
}