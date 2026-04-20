using UnityEngine;
using System.Collections.Generic;
using Code.MainSystem.TraitSystem.Runtime;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitContainer : MonoBehaviour, IUIElement<ITraitHolder>
    {
        [SerializeField] private TraitBar traitBar;
        [SerializeField] private Transform traitBarRoot;

        private readonly List<TraitBar> _bars = new();

        public void EnableFor(ITraitHolder holder)
        {
            EnsureBarCount(holder.ActiveTraits.Count);

            for (int i = 0; i < _bars.Count; i++)
            {
                if (i < holder.ActiveTraits.Count)
                {
                    ActiveTrait trait = holder.ActiveTraits[i];
                    _bars[i].EnableFor(trait);
                }
                else
                {
                    _bars[i].Disable();
                }
            }
            
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            foreach (var bar in _bars)
                bar.Disable();

            gameObject.SetActive(false);
        }

        private void EnsureBarCount(int count)
        {
            while (_bars.Count < count)
            {
                TraitBar bar = Instantiate(traitBar, traitBarRoot);
                _bars.Add(bar);
            }
        }
    }
}