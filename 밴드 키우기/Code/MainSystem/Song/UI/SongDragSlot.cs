using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.MainSystem.Song.UI
{
    public class SongDropSlot : BaseUIComponent, IDropHandler
    {
        [SerializeField] private DragItem dragItem;
        
        public event Action UploadSongReady;
        
        public override void Reset()
        {
            dragItem.Reset();
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            // 드래그 중인 오브젝트가 있는지 확인
            GameObject dropped = eventData.pointerDrag;
            DragItem delta = dropped.GetComponent<DragItem>();
            if (delta != null)
            {
                UploadSongReady?.Invoke();
            }
        }
    }
}