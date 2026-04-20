using UnityEngine;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TraitEvents;
using Code.MainSystem.TraitSystem.Interface;

namespace Code.MainSystem.TraitSystem.UI
{
    public class TraitControllerUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TraitContainer container;
        [SerializeField] private DetailTraitPanel detailTraitPanel;
        [SerializeField] private TraitPointGauge pointGauge;
        //[SerializeField] private TraitOverflowPanel overflowPanel;
        [SerializeField] private RemoveUI removeUI;
        
        private ITraitHolder _currentHolder;
        
        private void Awake()
        {
            RegisterEvents();
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #region Event Management

        private void RegisterEvents()
        {
            Bus<TraitShowResponded>.OnEvent += HandleTraitShowResponded;
            Bus<TraitShowItem>.OnEvent += HandleTraitShowItem;
            Bus<TraitOverflow>.OnEvent += HandleTraitOverflow;
            Bus<TraitAdjusted>.OnEvent += HandleTraitAdjusted;
            Bus<TraitRemoveRequestedUI>.OnEvent += HandleTraitRemoveRequestedUI;
        }

        private void UnregisterEvents()
        {
            Bus<TraitShowResponded>.OnEvent -= HandleTraitShowResponded;
            Bus<TraitShowItem>.OnEvent -= HandleTraitShowItem;
            Bus<TraitOverflow>.OnEvent -= HandleTraitOverflow;
            Bus<TraitAdjusted>.OnEvent -= HandleTraitAdjusted;
            Bus<TraitRemoveRequestedUI>.OnEvent -= HandleTraitRemoveRequestedUI;
        }

        #endregion

        private void InitializeUI()
        {
            detailTraitPanel?.Disable();
            //overflowPanel?.Disable();
            removeUI?.Disable();
        }

        /// <summary>
        /// 특성 목록 표시 응답 처리
        /// </summary>
        private void HandleTraitShowResponded(TraitShowResponded evt)
        {
            _currentHolder = evt.Holder;
            RefreshUI();
        }

        /// <summary>
        /// 특성 상세 정보 표시 요청 처리
        /// </summary>
        private void HandleTraitShowItem(TraitShowItem evt)
        {
            detailTraitPanel?.EnableFor(evt.Trait);
        }

        /// <summary>
        /// 특성 포인트 초과 처리
        /// </summary>
        private void HandleTraitOverflow(TraitOverflow evt)
        {
            //overflowPanel?.EnableFor(evt.CurrentPoint, evt.MaxPoint);
            pointGauge?.EnableFor(evt.CurrentPoint, evt.MaxPoint);
        }

        /// <summary>
        /// 특성 조정 완료 처리
        /// </summary>
        private void HandleTraitAdjusted(TraitAdjusted evt)
        {
            //overflowPanel?.Disable();
            RefreshUI();
        }

        /// <summary>
        /// 특성 제거 확인 UI 표시 요청 처리
        /// </summary>
        private void HandleTraitRemoveRequestedUI(TraitRemoveRequestedUI evt)
        {
            removeUI?.EnableFor(evt.Trait, evt.Holder);
        }

        /// <summary>
        /// 전체 UI 갱신
        /// </summary>
        public void RefreshUI()
        {
            if (_currentHolder == null)
                return;

            container?.EnableFor(_currentHolder);
            pointGauge?.EnableFor(_currentHolder.TotalPoint, _currentHolder.MaxPoints);
        }

        /// <summary>
        /// UI 초기화 (외부에서 호출 가능)
        /// </summary>
        public void ResetUI()
        {
            _currentHolder = null;
            container?.Disable();
            detailTraitPanel?.Disable();
            pointGauge?.Disable();
            //overflowPanel?.Disable();
            removeUI?.Disable();
        }
    }
}