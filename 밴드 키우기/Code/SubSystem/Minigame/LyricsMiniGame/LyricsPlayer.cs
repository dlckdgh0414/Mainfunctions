using Code.Core;
using Code.MainSystem.NewMainScreen.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    public class LyricsPlayer : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D   rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private float moveSpeed = 5f;

        private float _moveDir;
        private bool _isButtonHeld;
        private bool _isGameActive = true;

        private MemberType _memberType;
        private MemberDataSO memberData;

        public void Initialize(MemberDataSO data)
        {
            _memberType = data.memberType;
            memberData  = data;
            spriteRenderer.sprite = memberData.SDsprite;
            _isGameActive = true;
            
            if (playerCollider != null)
                playerCollider.enabled = true;
                
            Debug.Log($"[LyricsPlayer] 메인 멤버 세팅: {_memberType}");
        }

        private void Update()
        {
            if (!_isGameActive) return;

            float keyboardInput = 0f;

            if (Keyboard.current.aKey.wasPressedThisFrame)
                keyboardInput = -1f;
            else if (Keyboard.current.dKey.wasPressedThisFrame)
                keyboardInput = 1f;

            float finalDir = _isButtonHeld ? _moveDir : keyboardInput;
            rb.linearVelocity = new Vector2(finalDir * moveSpeed, rb.linearVelocity.y);
        }
        
        public void OnLeftDown()  
        { 
            if (!_isGameActive) return;
            _moveDir = -1f; 
            _isButtonHeld = true; 
        }
        
        public void OnRightDown() 
        { 
            if (!_isGameActive) return;
            _moveDir =  1f; 
            _isButtonHeld = true; 
        }
        
        public void OnButtonUp()  
        { 
            _moveDir =  0f; 
            _isButtonHeld = false; 
        }

        public void StopGame()
        {
            _isGameActive = false;
            rb.linearVelocity = Vector2.zero;
            
            if (playerCollider != null)
                playerCollider.enabled = false;
        }
    }
}