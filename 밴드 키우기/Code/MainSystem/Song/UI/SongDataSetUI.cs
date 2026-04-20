using System;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.SongEvents;
using Code.SubSystem.BandFunds;
using Code.Tool.UIBaseScript;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Song.UI
{
    [Serializable]
    public struct CostAndBuff
    {
        public int Cost;
        public int Buff;
    }
    
    public class SongDataSetUI : BaseUIComponent
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private CustomDropdown mvDropdown;
        [SerializeField] private CustomDropdown thumbnailDropdown;
        
        [Header("가격 및 버프 설정")]
        [SerializeField] private List<CostAndBuff> thumbnailList = new List<CostAndBuff>();
        [SerializeField] private List<CostAndBuff> mvList = new List<CostAndBuff>();
        
        [SerializeField] private Button uploadButton;
        
        public event Action UploadSongSucceeded;

        private int _currentThumbnailCost = 0;
        private int _currentMVCost = 0;
        
        private void OnEnable()
        {
            uploadButton.onClick.AddListener(HandleNextClick);
            
            // 값이 바뀔 때마다 상대 드롭다운의 활성 여부를 갱신
            mvDropdown.onValueChanged.AddListener(HandleMVChanged);
            thumbnailDropdown.onValueChanged.AddListener(HandleThumbnailChanged);
            
            // 이름 입력 여부도 버튼 활성화에 영향
            nameInputField.onValueChanged.AddListener(_ => HandleEnoughSupply());
        }
        
        private void Start()
        {
            Reset(); // 초기 상태 세팅
        }
        
        private void HandleMVChanged(int idx)
        {
            UpdateCosts();
            HandleEnoughSupply();
        }

        private void HandleThumbnailChanged(int idx)
        {
            UpdateCosts();
            HandleEnoughSupply();
        }

        private void UpdateCosts()
        {
            // 인덱스 안전망 체크 후 비용 업데이트
            if (idxInRange(mvDropdown.value, mvList))
                _currentMVCost = mvList[mvDropdown.value].Cost;
            
            if (idxInRange(thumbnailDropdown.value, thumbnailList))
                _currentThumbnailCost = thumbnailList[thumbnailDropdown.value].Cost;
        }

        private bool idxInRange<T>(int idx, List<T> list) => idx >= 0 && idx < list.Count;

        private void HandleNextClick()
        {
            if (string.IsNullOrEmpty(nameInputField.text)) return;

            int totalCost = _currentMVCost + _currentThumbnailCost;
            if (BandSupplyManager.Instance.BandFunds < totalCost) return;

            // 이벤트 발행 및 자금 차감
            Bus<SongUploadOptionEvent>.Raise(new SongUploadOptionEvent(
                nameInputField.text,
                (MarketingQuality)mvDropdown.value,
                (MarketingQuality)thumbnailDropdown.value));
            
            BandSupplyManager.Instance.SpendBandFunds(_currentMVCost);
            BandSupplyManager.Instance.SpendBandFunds(_currentThumbnailCost);
            
            UploadSongSucceeded?.Invoke();
        }
        
        public override void Reset()
        {
            nameInputField.text = "";
            mvDropdown.value = 0;
            thumbnailDropdown.value = 0;
            UpdateCosts();
            HandleEnoughSupply();
        }

        private void HandleEnoughSupply()
        {
            // 1. 현재 보유 자금 확인
            int currentFunds = BandSupplyManager.Instance.BandFunds;

            // 2. 현재 선택된 값들에 따른 비용 최신화
            UpdateCosts();

            // 3. MV 드롭다운 선택지 제한 로직
            // 가용 예산 = 전체 자금 - 현재 고정된 썸네일 비용
            int budgetForMV = currentFunds - _currentThumbnailCost;
            List<int> disabledMVs = new List<int>();

            for (int i = 0; i < mvList.Count; i++)
            {
                if (mvList[i].Cost > budgetForMV)
                {
                    disabledMVs.Add(i);
                }
            }
            mvDropdown.SetDisabledIndices(disabledMVs);

            // [중요] 현재 선택된 MV가 예산을 초과하게 되었다면 강제로 0번(기본)으로 변경
            if (mvList[mvDropdown.value].Cost > budgetForMV)
            {
                mvDropdown.SetValueWithoutNotify(0);
                UpdateCosts(); // MV가 바뀌었으므로 비용 다시 계산
            }

            // 4. 썸네일 드롭다운 선택지 제한 로직
            // 가용 예산 = 전체 자금 - 현재 고정된 MV 비용
            int budgetForThumbnail = currentFunds - _currentMVCost;
            List<int> disabledThumbnails = new List<int>();

            for (int i = 0; i < thumbnailList.Count; i++)
            {
                if (thumbnailList[i].Cost > budgetForThumbnail)
                {
                    disabledThumbnails.Add(i);
                }
            }
            thumbnailDropdown.SetDisabledIndices(disabledThumbnails);

            // [중요] 현재 선택된 썸네일이 예산을 초과하게 되었다면 강제로 0번(기본)으로 변경
            if (thumbnailList[thumbnailDropdown.value].Cost > budgetForThumbnail)
            {
                thumbnailDropdown.SetValueWithoutNotify(0);
                UpdateCosts(); // 다시 최종 비용 계산
            }

            // 5. 업로드 버튼 활성화 제어
            // 이름이 있고, 최종 합계 금액이 자산보다 적거나 같아야 함
            bool hasName = !string.IsNullOrEmpty(nameInputField.text);
            bool canAfford = currentFunds >= (_currentMVCost + _currentThumbnailCost);
    
            uploadButton.interactable = hasName && canAfford;
        }

        private void OnDisable()
        {
            uploadButton.onClick.RemoveAllListeners();
            
            // 값이 바뀔 때마다 상대 드롭다운의 활성 여부를 갱신
            mvDropdown.onValueChanged.RemoveAllListeners();
            thumbnailDropdown.onValueChanged.RemoveAllListeners();
            
            // 이름 입력 여부도 버튼 활성화에 영향
            nameInputField.onValueChanged.RemoveAllListeners();
        }
    }
}