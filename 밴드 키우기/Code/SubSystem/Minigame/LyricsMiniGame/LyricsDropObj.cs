using Chuh007Lib.ObjectPool.RunTime;
using Code.Core;
using UnityEngine;
using Work.CHUH.Chuh007Lib.ObjectPool.RunTime;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    public class LyricsDropObj : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D    rb;
        [SerializeField] private Collider2D     col;

        [Header("Pool")]
        [SerializeField] private PoolItemSO poolItem;

        private static readonly int OutlineColor     = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");

        private LyricsDropObjDataSO _data;
        private LyricsController    _controller;
        private Pool                _pool;
        private Material            _mat;

        #region IPoolable

        public PoolItemSO PoolItem => poolItem;

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
            _mat  = new Material(spriteRenderer.material);
            spriteRenderer.material = _mat;
        }

        public void ResetItem()
        {
            _data       = null;
            _controller = null;

            rb.linearVelocity = Vector2.zero;
            rb.gravityScale   = 0f;
            rb.bodyType       = RigidbodyType2D.Dynamic;
            col.isTrigger     = true;
        }

        #endregion

        public void Initialize(LyricsDropObjDataSO data, LyricsController controller, float fallSpeed)
        {
            _data       = data;
            _controller = controller;

            spriteRenderer.sprite = data.ItemSprite;

            _mat.SetColor(OutlineColor,     data.ItemOutlineColor);
            _mat.SetFloat(OutlineThickness, data.ItemOutlineThickness);

            rb.gravityScale   = 0f;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _controller.OnItemCollected(_data.IsGoodItem);
                ReturnToPool();
                return;
            }

            if (other.CompareTag("BottomBound"))
                ReturnToPool();
        }

        private void ReturnToPool()
        {
            // 컨트롤러에 제거 알림
            _controller?.OnDropObjectDestroyed(this);
            
            if (_pool != null)
                _pool.Push(this);
            else
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_mat != null)
                Destroy(_mat);
        }
    }
}