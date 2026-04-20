using UnityEngine;
using System.Collections.Generic;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitOverlayUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        private readonly Stack<GameObject> _panelStack = new Stack<GameObject>();
        private const float AlphaPerPanel = 0.3f;

        private void Awake()
        {
            overlayCanvasGroup.alpha = 0;
            overlayCanvasGroup.gameObject.SetActive(false);
        }

        public void OnPanelOpened(GameObject panel)
        {
            _panelStack.Push(panel);
            UpdateOverlayAlpha();
        }
    
        public void OnPanelClosed(GameObject panel)
        {
            if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
            {
                _panelStack.Pop();
            }
            else
            {
                var temp = new Stack<GameObject>();
                while (_panelStack.Count > 0)
                {
                    var p = _panelStack.Pop();
                    if (p != panel)
                        temp.Push(p);
                }
                while (temp.Count > 0)
                    _panelStack.Push(temp.Pop());
            }
    
            UpdateOverlayAlpha();
    
            if (_panelStack.Count == 0)
                overlayCanvasGroup.gameObject.SetActive(false);
        }
        
        public void OnOverlayClicked()
        {
            if(_panelStack.Count <= 0)
                return;
            
            GameObject topPanel = _panelStack.Pop();
            topPanel.SetActive(false);
            
            UpdateOverlayAlpha();
            
            if (_panelStack.Count == 0)
                overlayCanvasGroup.gameObject.SetActive(false);
        }
    
        private void UpdateOverlayAlpha()
        {
            float newAlpha = Mathf.Min(_panelStack.Count * AlphaPerPanel, 0.8f);
    
            overlayCanvasGroup.alpha = newAlpha;
            overlayCanvasGroup.gameObject.SetActive(true);
        }
    }
}