using System.Collections.Generic;
using System.Linq;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.Core.Bus.GameEvents.TreeEvents;
using Code.MainSystem.Sound;
using Code.SubSystem.BandFunds;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.MainSystem.Tree.UI
{
    public class TreeBottomInfo : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private TextMeshProUGUI nodeDescriptionText;
        [SerializeField] private Button nodeActiveBtn;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Sprite activeButtonBack;
        [SerializeField] private Sprite unActiveButtonBack;
        [SerializeField] private TextMeshProUGUI buttonBackText;
        
        [SerializeField] private List<TextMeshProUGUI> buttonTexts;
        
        [Header("Sound")]
        [SerializeField] private SoundSO nodeActiveSound;
        
        private readonly string ACTIVE_TEXT = "활성화";
        private readonly string CANTACTIVE_TEXT = "이전 노드 활성화 필요";
        
        private TreeNodeDataSO _nodeData;

        
        private void Awake()
        {
            Bus<NodeSelectEvent>.OnEvent += HandleTreeInfoChange;
            nodeActiveBtn.onClick.AddListener(HandleActive);
        }
        
        private void OnDestroy()
        {
            Bus<NodeSelectEvent>.OnEvent -= HandleTreeInfoChange;
        }
        
        private void HandleTreeInfoChange(NodeSelectEvent evt)
        {
            
            _nodeData = evt.NodeData;
            iconImage.sprite = evt.NodeData.realIcon;
            nodeNameText.SetText(evt.NodeData.nodeName);
            costText.SetText(evt.NodeData.cost.ToString());
            nodeDescriptionText.SetText(evt.NodeData.description);
            if (TreeDataManager.Instance.IsNodeAvailable(_nodeData.nodeID))
            {
                nodeActiveBtn.gameObject.SetActive(false);
                buttonBackText.color = Color.yellow;
                buttonBackText.SetText(ACTIVE_TEXT);
                nodeActiveBtn.gameObject.SetActive(false);
            }
            else nodeActiveBtn.gameObject.SetActive(true);
            
            if (TreeDataManager.Instance.CanActivateNode(_nodeData.nodeID))
            {
                if (TreeDataManager.Instance.IsEnoughCost(_nodeData.nodeID))
                {
                    nodeActiveBtn.interactable = true;
                    foreach (var text in buttonTexts)
                    {
                        text.color = Color.black;
                    }
                    nodeActiveBtn.image.sprite = activeButtonBack;
                }
                else
                {
                    foreach (var text in buttonTexts)
                    {
                        text.color = Color.white;
                        nodeActiveBtn.interactable = false;
                        nodeActiveBtn.image.sprite = unActiveButtonBack;
                    }
                }
            }
            else
            {
                buttonBackText.color = Color.red;
                buttonBackText.SetText(CANTACTIVE_TEXT);
                nodeActiveBtn.interactable = false;
                nodeActiveBtn.gameObject.SetActive(false);
            }
        }
        
        private void HandleActive()
        {
            bool ans = TreeDataManager.Instance.TryActivateNode(_nodeData.nodeID);
            if (ans)
            {
                Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(nodeActiveSound));
                nodeActiveBtn.gameObject.SetActive(false);
                buttonBackText.color = Color.yellow;
                buttonBackText.SetText(ACTIVE_TEXT);
            }
        }
    }
}