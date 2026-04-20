using Chuh007Lib.ObjectPool.RunTime;
using Code.MainSystem.RhythmQTE;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Work.CHUH.Chuh007Lib.ObjectPool.RunTime;

namespace Code.SubSystem.Minigame.RhythmQTE
{
    /// <summary>
    /// QTE에 나오는 오브젝트.
    /// 클릭하면 QTEController의 OnQTEPressSucceed을 작동시키고, 풀로 돌아간다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class QTEObject : MonoBehaviour, IQTEObject, IPoolable
    {
        [SerializeField] private Button button;
        private float _currentLifeTime;
        private float _lifeTime;
        
        private QTEController _controller;
        private RectTransform _rectTrm;
        
        public void Initailize(QTEController controller, float lifeTime)
        {
            _rectTrm = GetComponent<RectTransform>();
            _controller = controller;
            button.onClick.AddListener(EndMoving);
            _lifeTime = lifeTime;

            DoScaling(0.25f, 1f, 0.2f);
        }
        
        private void Update()
        {
            _currentLifeTime += Time.deltaTime;
            if (_currentLifeTime >= _lifeTime && button.interactable)
            {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
                DoScaling(1, 0.1f);
                DOVirtual.DelayedCall(0.25f, () => _myPool.Push(this));
            }
        }
        
        private void EndMoving()
        {
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            _rectTrm.DOAnchorPosY(-500, 1f).SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _rectTrm.DOMove(new Vector3(-500, 1750, 0), 0.5f)
                            .OnComplete(() =>
                            {
                                _controller.OnQTEPressSucceed();
                                _myPool.Push(this);
                            });
                    });
                });
        }
        
        private void DoScaling(float startScale, float endScale, float duration = 0.25f)
        {
            transform.localScale = Vector3.one * startScale;
            transform.DOScale(endScale, duration);
        }
        
        [field:SerializeField] public PoolItemSO PoolItem { get; private set; }
        
        private Pool _myPool;
        
        public void ResetItem()
        {
            _currentLifeTime = 0f;
            button.interactable = true;
        }
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
    }
}