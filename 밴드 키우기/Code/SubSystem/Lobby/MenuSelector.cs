using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.SubSystem.Lobby
{
    public enum MenuType
    {
        Upgrade = 0,
        Story = 1,
        Home = 2,
        Performance = 3,
        Gacha = 4
    }

    public class MenuSelector : MonoBehaviour
    {
        [SerializeField] private List<Menu> menuList;
        [SerializeField] private float transitionDuration = 0.3f;
        
        private RectTransform _canvasRect;
        private float _offScreenX;
        public int CurrentMenuIndex = 2;
        
        private bool _isTransitioning;
        private CancellationTokenSource _cts;
        
        private void Awake()
        {
            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            UpdateOffScreenDistance();
            InitMenus();
        }
        
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            DOTween.Kill(this);
        }
        
        private void OnRectTransformDimensionsChange()
        {
            if (_canvasRect == null) return;
            UpdateOffScreenDistance();
            
            if (!_isTransitioning)
            {
                ReorganizeStacks();
            }
        }
        
        /// <summary>
        /// 메뉴들 정리하기
        /// </summary>
        private void InitMenus()
        {
            menuList = menuList.OrderBy(menu => menu.type).ToList();
            
            for (int i = 0; i < menuList.Count; i++)
            {
                RectTransform rect = menuList[i].GetComponent<RectTransform>();
                if (i == CurrentMenuIndex)
                {
                    rect.anchoredPosition = Vector2.zero;
                    menuList[i].Activate();
                }
                else
                {
                    float targetX = (i < CurrentMenuIndex) ? -_offScreenX : _offScreenX;
                    rect.anchoredPosition = new Vector2(targetX, 0);
                    menuList[i].Deactivate();
                }
            }
        }
        
        /// <summary>
        /// MenuType int로 캐스팅해서 널기.
        /// 버튼 연결하려멱 직렬화 되야해서 이리 함
        /// </summary>
        /// <param name="menuTypeInt">int화한 메뉴 타입</param>
        public void SelectMenu(int menuTypeInt)
        {
            if (_isTransitioning) return;
            
            int targetIndex = menuList.FindIndex(m => m.type == (MenuType)menuTypeInt);
            if (targetIndex == -1 || targetIndex == CurrentMenuIndex) return;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            TransitionTaskAsync(targetIndex, _cts.Token).Forget();
        }
        
        /// <summary>
        /// 메뉴 전환 연출
        /// </summary>
        private async UniTaskVoid TransitionTaskAsync(int targetIndex, CancellationToken token)
        {
            _isTransitioning = true;
            
            try
            {
                Menu prevMenu = menuList[CurrentMenuIndex];
                Menu nextMenu = menuList[targetIndex];
                
                for (int i = 0; i < menuList.Count; i++)
                {
                    if (CurrentMenuIndex != i && targetIndex != i)
                        menuList[i].Deactivate();
                }
                
                UpdateOffScreenDistance();
                
                float nextStartPos = (targetIndex < CurrentMenuIndex) ? -_offScreenX : _offScreenX;
                float prevTargetPos = (targetIndex < CurrentMenuIndex) ? _offScreenX : -_offScreenX;
                
                RectTransform nextRect = nextMenu.GetComponent<RectTransform>();
                RectTransform prevRect = prevMenu.GetComponent<RectTransform>();
                
                nextRect.anchoredPosition = new Vector2(nextStartPos, 0);
                nextMenu.Activate();
                
                Sequence seq = DOTween.Sequence().SetTarget(this);
                seq.Join(prevRect.DOAnchorPos(new Vector2(prevTargetPos, 0), transitionDuration).SetEase(Ease.OutQuad));
                seq.Join(nextRect.DOAnchorPos(Vector2.zero, transitionDuration).SetEase(Ease.OutQuad));
                
                await UniTask.WaitWhile(() => seq.IsActive() && seq.IsPlaying(), PlayerLoopTiming.Update, token);
                
                prevMenu.Deactivate();
                CurrentMenuIndex = targetIndex;
                ReorganizeStacks();
            }
            catch (OperationCanceledException) { DOTween.Kill(this); }
            finally { _isTransitioning = false; }
        }
        
        /// <summary>
        /// 메뉴 위치 정렬하고 나머지 끄기
        /// </summary>
        private void ReorganizeStacks()
        {
            UpdateOffScreenDistance();
            
            for (int i = 0; i < menuList.Count; i++)
            {
                RectTransform rect = menuList[i].rectTrm;
                if (i != CurrentMenuIndex)
                {
                    float targetX = (i < CurrentMenuIndex) ? -_offScreenX : _offScreenX;
                    rect.anchoredPosition = new Vector2(targetX, 0);
                    menuList[i].Deactivate();
                }
            }
        }
        
        private void UpdateOffScreenDistance() => _offScreenX = _canvasRect.rect.width;

    }
}