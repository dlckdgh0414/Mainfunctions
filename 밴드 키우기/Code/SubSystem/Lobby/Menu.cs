using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.SubSystem.Lobby
{
    public class Menu : MonoBehaviour
    {
        public MenuType type;
        [HideInInspector] public RectTransform rectTrm;

        [SerializeField] private List<GameObject> childrenObjects;
        [SerializeField] private List<GameObject> defaultButtonObjects;
        
        private void Awake()
        {
            rectTrm = GetComponent<RectTransform>();
            foreach (var child in childrenObjects)
            {
                child.SetActive(false);
            }

            foreach (var obj in defaultButtonObjects)
            {
                obj.SetActive(true);
            }
        }
        
        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}