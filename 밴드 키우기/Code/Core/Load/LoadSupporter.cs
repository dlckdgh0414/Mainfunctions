using System;
using System.Linq;
using UnityEngine;

namespace Code.Core.Load
{
    /// <summary>
    /// 게임 시작할때 딱 한번만 실행해줌(껐다켜도 다시 실행 안되게끔)
    /// 초기화하려면 로비에서 데이터 날리기
    /// </summary>
    public class LoadSupporter : MonoBehaviour
    {
        public SaveLoadSO saveLoadSO;
        private void Awake()
        {
            if(saveLoadSO.isLoaded) return;
            var list = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IAwakeLoadComponent>()
                .ToList();
            foreach (var item in list)
            {
                item.FirstTimeAwake();
            }
            saveLoadSO.isLoaded = true;
        }
    }
}