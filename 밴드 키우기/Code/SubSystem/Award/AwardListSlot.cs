using TMPro;
using UnityEngine;
using Code.SubSystem.Save;

namespace Code.SubSystem.Award
{
    public class AwardListSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI awardNameText;
        [SerializeField] private TextMeshProUGUI winnerInfoText;

        public void Setup(string awardName, string winnerName, string songTitle = "", bool isPlayer = false)
        {
            awardNameText.text = awardName;

            if (isPlayer)
            {
                string myBandName = SaveManager.Instance?.Data?.bandName;
                if (string.IsNullOrEmpty(myBandName))
                    myBandName = "우리 밴드"; 

                winnerInfoText.text = string.IsNullOrEmpty(songTitle)
                    ? myBandName
                    : $"{myBandName} : {songTitle}";
            }
            else
            {
                winnerInfoText.text = $"{winnerName} : {songTitle}";
            }
        }
    }
}