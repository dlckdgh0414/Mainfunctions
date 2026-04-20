using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SystemEvents;
using Code.SubSystem.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem
{
    public class StartScene : MonoBehaviour
    {
        [SerializeField] private Button startBtn;

        [Header("Scene Names")]
        [SerializeField] private string bandNameSceneName = "BandNameScene";
        [SerializeField] private string mainSceneName = "MainScene";

        private void Awake()
        {
            startBtn.onClick.AddListener(HandleStartGame);
        }

        private void HandleStartGame()
        {
            string nextScene = DecideNextScene();
            Debug.Log($"[StartScene] 다음 씬: {nextScene}");
            Bus<FadeSceneEvent>.Raise(new FadeSceneEvent(nextScene));
        }

        private string DecideNextScene()
        {
            if (SaveManager.Instance == null) return bandNameSceneName;
            
            if (SaveManager.Instance.IsTutorialCompleted)
                return mainSceneName;
            
            return bandNameSceneName;
        }

        private void OnDestroy()
        {
            startBtn.onClick.RemoveAllListeners();
        }
    }
}