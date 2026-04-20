using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SynergyEvents;
using Code.MainSystem.Synergy.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Synergy.UI
{
    public class SynergyUIController : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject displayPanel;
        [SerializeField] private Transform synergyStatusRoot;
        
        [Header("Templates")]
        [SerializeField] private SynergyStatusUI synergyStatusPrefab;
        
        [Header("Controls")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        private readonly List<SynergyStatusUI> _uiPool = new();
        private bool _isOpen;

        private const string TextOpen = "보유현황";
        private const string TextClose = "닫기";

        private void Awake()
        {
            displayPanel.SetActive(false);
            toggleButton.onClick.AddListener(ToggleStatusUI);
        }

        private void OnEnable()
        {
            Bus<SynergyUpdateEvent>.OnEvent += HandleSynergyUpdate;
        }

        private void OnDestroy()
        {
            Bus<SynergyUpdateEvent>.OnEvent -= HandleSynergyUpdate;
        }
        
        private void HandleSynergyUpdate(SynergyUpdateEvent evt)
        {
            if (_isOpen) 
                Refresh();
        }

        private void ToggleStatusUI()
        {
            _isOpen = !_isOpen;
            
            if (_isOpen) 
                Refresh();
            
            displayPanel.SetActive(_isOpen);
            buttonText.SetText(_isOpen ? TextClose : TextOpen);
        }

        private void Refresh()
        {
            var manager = SynergyEffectManager.Instance;
            if (manager is null || !manager.Initialize)
                return;

            var activeSynergies = manager.ActiveSynergies;
            
            for (int i = 0; i < activeSynergies.Count; i++)
            {
                var slot = GetOrCreateSlot(i);
                slot.gameObject.SetActive(true);
                slot.EnableFor(activeSynergies[i]);
            }
            
            for (int i = activeSynergies.Count; i < _uiPool.Count; i++)
            {
                _uiPool[i].Disable();
                _uiPool[i].gameObject.SetActive(false);
            }
        }

        private SynergyStatusUI GetOrCreateSlot(int index)
        {
            if (index < _uiPool.Count) 
                return _uiPool[index];
            
            var newSlot = Instantiate(synergyStatusPrefab, synergyStatusRoot);
            newSlot.transform.localScale = Vector3.one;
            _uiPool.Add(newSlot);
            
            return newSlot;
        }
    }
}