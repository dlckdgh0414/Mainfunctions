using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Tree.Addon;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.MainSystem.Tree.Upgrade
{
    /// <summary>
    /// 트리 노드 효과 기반
    /// IAddon을 상속받은 스크립트가 이것들을 리스트로 가지고 있고, 값 계산 시 이것들을 호출한다.
    /// </summary>
    public abstract class BaseUpgradeSO : ScriptableObject
    {
        public TreeUpgradeType type;
        [TextArea] public string effectDescription;
        
        public abstract void Upgrade(IAddon addon);
    }
}