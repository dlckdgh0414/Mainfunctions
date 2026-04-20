using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.Song.Director
{
    public class CommentObjectUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private TextMeshProUGUI commentContentText;
        
        [Header("Critic Only")]
        [SerializeField] private GameObject criticText;
        [SerializeField] private GameObject starParent;
        [SerializeField] private Image starBar;
        [SerializeField] private TextMeshProUGUI starText;
        
        private readonly string SLASH_FORMAT = "{0:F1} / {1:F1}";
        
        public void SetupData(CommentDataSO commentData)
        {
            userNameText.text = commentData.UserName;
            commentContentText.text = commentData.CommentText;
            
            // 비평가 데이터일 경우 별점 표시
            if (commentData.IsCritic)
            {
                starParent.SetActive(true);
                starBar.fillAmount = commentData.Star / 5f;
                criticText.SetActive(true);
                starText.text = string.Format(SLASH_FORMAT, commentData.Star, 5);
            }
            else
            {
                criticText.SetActive(false);
                starParent.SetActive(false);
            }
        }
    }
}