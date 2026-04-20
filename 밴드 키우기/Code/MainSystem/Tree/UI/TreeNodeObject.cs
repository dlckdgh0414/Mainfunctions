using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.Core.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.MainSystem.Tree.UI
{
    /// <summary>
    /// TreeNodeDataSO를 담고, 보여준다.
    /// 선택하면 : 하얀 테두리로 주목시킴.
    /// 해금했으면 : 아이콘에 색 부여
    /// 해금할 수 있으면 : 아이콘이 흰 색임
    /// 해금 못하면 : 테두리, 아이콘 회색에 잠금 표시
    /// </summary>
    public class TreeNodeObject : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image outLineImage;
        [SerializeField] private Image lockImage;
        [SerializeField] private Color lockColor;
        [SerializeField] private Color openColor;
        
        private Button _button;

        public Transform lineTrm;
        
        public TreeNodeDataSO NodeData { get; private set; }
        
        private void Awake()
        {
            _button = GetComponentInChildren<Button>();
            Bus<UpdateNodeEvent>.OnEvent += UpdateUI;
            _button.onClick.AddListener(HandleNodeClick);
            iconImage.color = lockColor;
            outLineImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Bus<UpdateNodeEvent>.OnEvent -= UpdateUI;
        }

        private void HandleNodeClick()
        {
            Bus<NodeSelectEvent>.Raise(new NodeSelectEvent(NodeData));
        }
        
        public void SetupData(TreeNodeDataSO nodeData)
        {
            NodeData = nodeData;
            iconImage.sprite = nodeData.realIcon;
        }
        
        public void UpdateUI(UpdateNodeEvent evt)
        {
            if (TreeDataManager.Instance.IsNodeAvailable(NodeData.nodeID))
            {
                Activate();
            }
            else if (TreeDataManager.Instance.CanActivateNode(NodeData.nodeID))
            {
                UnLock();
            }
        }
        
        // 선택했으면 하얀 태두리만 해줌.
        public void Select()
        {
            outLineImage.gameObject.SetActive(true);
        }
        
        public void UnSelect()
        {
            outLineImage.gameObject.SetActive(false);
        }
        
        // 열 수 있어지면 비주얼 업데이트
        public void UnLock()
        {
            iconImage.color = Color.white;
            lockImage.gameObject.SetActive(false);
        }
        
        // 열기를 선택하면 아예 열린게 됨.
        public void Activate()
        {
            iconImage.color = openColor;
            lockImage.gameObject.SetActive(false);
        }
        
    }
}