using System;
using System.Threading.Tasks;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.MiniGameEvent;
using TMPro;
using UnityEngine;

namespace Code.SubSystem.Minigame.Common.Management
{
    public class CountingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countUI;
        
        private void Awake()
        {
            gameObject.SetActive(true);
            Bus<MiniGameStartCountingEvent>.OnEvent += HandleStartCounting;
        }

        private void OnDestroy()
        {
            Bus<MiniGameStartCountingEvent>.OnEvent += HandleStartCounting;
        }

        private void HandleStartCounting(MiniGameStartCountingEvent evt)
        {
            Counting(evt.Counting);
        }

        public async Task Counting(int count)
        {
            for (int i = 0; i < count; i++)
            {
                countUI.SetText((count - i).ToString());
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
            gameObject.SetActive(false);
            Bus<StartCountingEndEvent>.Raise(new StartCountingEndEvent());
        }
        
    }
}