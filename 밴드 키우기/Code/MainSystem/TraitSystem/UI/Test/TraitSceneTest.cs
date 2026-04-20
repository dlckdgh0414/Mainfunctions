using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.MainSystem.TraitSystem.UI.Test
{
    public class TraitSceneTest : MonoBehaviour
    {
        public void NextScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}