using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Tool
{
    [Serializable]
    public struct AwakeObject
    {
        public bool isActive;
        public GameObject gameObject;
    }
    
    public class ScriptAwaker : MonoBehaviour
    {
        [SerializeField] private List<AwakeObject> awakeObjects;

        private void Awake()
        {
            foreach (var awakeObject in awakeObjects)
            {
                awakeObject.gameObject.SetActive(true);
            }
            foreach (var awakeObject in awakeObjects)
            {
                if (!awakeObject.isActive)
                    awakeObject.gameObject.SetActive(false);
            }
        }
    }
}