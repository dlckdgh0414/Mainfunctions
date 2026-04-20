using System.Threading.Tasks;
using Code.Core.Addressable;
using UnityEngine;
using Reflex.Attributes;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Code.MainSystem.TraitSystem.UI
{
    public abstract class TraitPanelBase : MonoBehaviour
    {
        [Inject] private TraitOverlayUI _overlayUI;
        [SerializeField] protected GameObject panel;

        protected virtual void Show()
        {
            _overlayUI.OnPanelOpened(panel);
            panel.SetActive(true);
        }

        protected virtual void Hide()
        {
            _overlayUI.OnPanelClosed(panel);
            panel.SetActive(false);
        }
        
        /// <summary>
        /// AssetReference를 사용하여 Image 컴포넌트에 아이콘을 안전하게 로드하고 설정합니다.
        /// </summary>
        protected async Task SetIconSafeAsync(Image iconImage, AssetReferenceSprite iconRef)
        {
            if (iconImage is null)
                return;
            
            if (iconRef == null || !iconRef.RuntimeKeyIsValid())
            {
                iconImage.sprite = null;
                return;
            }

            string key = iconRef.RuntimeKey.ToString();
            GameResourceManager manager = GameResourceManager.Instance;
            
            Sprite cachedSprite = manager.Load<Sprite>(key);
            if (cachedSprite is not null)
            {
                iconImage.sprite = cachedSprite;
                return;
            }
            
            try
            {
                Sprite sprite = await manager.LoadAsync<Sprite>(key);
                if (sprite is not null)
                {
                    iconImage.sprite = sprite;
                }
            }
            catch (System.ArgumentException)
            {
                iconImage.sprite = manager.Load<Sprite>(key);
            }
        }

        public virtual void Cancel()
        {
            Hide();
        }
    }
}