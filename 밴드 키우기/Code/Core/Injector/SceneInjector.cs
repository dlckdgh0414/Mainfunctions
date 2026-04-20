using Reflex.Core;
using UnityEngine;

namespace Code.Core.Injector
{
    public class SceneInjector : MonoBehaviour, IInstaller
    {
        [SerializeField] private MonoBehaviour[] providers;
        
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            foreach (var provider in providers)
            {
                containerBuilder.AddSingleton(provider);
            }
        }
    }
}