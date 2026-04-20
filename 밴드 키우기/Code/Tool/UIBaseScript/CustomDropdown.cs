using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Song.UI
{
    public class CustomDropdown : TMP_Dropdown
    {
        [Header("Item Colors")]
        [SerializeField] private Color _enabledColor = new Color(0.584f, 0.706f, 0.804f, 1f); // #95B4CD
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        private List<int> _disabledIndices = new List<int>();
        private int _currentItemIndex = 0;

        public void SetDisabledIndices(List<int> indices)
        {
            _disabledIndices = indices;
        }

        protected override GameObject CreateDropdownList(GameObject template)
        {
            _currentItemIndex = 0;
            return base.CreateDropdownList(template);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            DropdownItem newItem = base.CreateItem(itemTemplate);

            int index = _currentItemIndex++;

            if (newItem.toggle != null)
            {
                bool isDisabled = _disabledIndices.Contains(index);
                newItem.toggle.interactable = !isDisabled;

                if (newItem.text != null)
                {
                    newItem.text.color = isDisabled ? _disabledColor : _enabledColor;
                }
            }

            return newItem;
        }
    }
}