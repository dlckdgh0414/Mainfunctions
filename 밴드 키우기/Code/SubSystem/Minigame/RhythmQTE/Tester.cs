using Code.Core.Bus;
using Code.Core.Bus.GameEvents.MiniGameEvent.QTEEvents;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.MainSystem.Song;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    // 테스트용
    public class Tester : MonoBehaviour
    {
        #if UNITY_EDITOR
        
        private void Awake()
        {
            Button btn = gameObject.GetComponentInChildren<Button>();
            if(btn != null) btn.onClick.AddListener(BtnClick);
        }

        private void BtnClick()
        {
            Bus<QTEStartEvent>.Raise(new QTEStartEvent());
        }
        
        [ContextMenu("Test")]
        public void Excute()
        {
            
        }
        
        #endif
    }
}