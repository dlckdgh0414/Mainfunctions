using System.Collections;
using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.SoundEvents;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.Song;
using Code.MainSystem.Sound;
using Code.SubSystem.Save;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.SubSystem.Award
{
    public class AwardEnvelopeUI : MonoBehaviour
    {
        [Header("팝업 루트")]
        [SerializeField] private CanvasGroup dimBackground;
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private CanvasGroup popupCanvasGroup;

        [Header("봉투")]
        [SerializeField] private GameObject envelopeArea;
        [SerializeField] private RectTransform envelopeFlap;
        [SerializeField] private RectTransform arrowIcon;

        [Header("상장 (왼쪽 - 플레이어 전용)")]
        [SerializeField] private GameObject certificateArea;
        [SerializeField] private RectTransform certificateRect;
        [SerializeField] private TextMeshProUGUI yearText;
        [SerializeField] private TextMeshProUGUI awardNameText;
        [SerializeField] private TextMeshProUGUI awardMentText;
        [SerializeField] private TextMeshProUGUI prizeMoneyText;

        [Header("상 목록 (오른쪽 - 전체 명단)")]
        [SerializeField] private GameObject awardListPanel;
        [SerializeField] private RectTransform awardListRect;
        [SerializeField] private Transform awardListContent;
        [SerializeField] private AwardListSlot awardListSlotPrefab;

        [Header("닫기")]
        [SerializeField] private Button closeButton;

        [Header("설정")]
        [SerializeField] private AwardSystemSO awardSystemSO;

        [Header("기본값")]
        [SerializeField] private string defaultBandName = "우리 밴드";
        
        [Header("Sound")]
        [SerializeField] private SoundSO clapSound;
        
        private List<AwardResult> _results;
        private AwardResult _playerResult;
        private bool _isOpened = false;
        private float _certOriginY;
        private float _listOriginY;

        private IEnumerator Start()
        {
            certificateArea.SetActive(true);
            awardListPanel.SetActive(true);
            yield return null;
            _certOriginY = certificateRect.anchoredPosition.y;
            _listOriginY = awardListRect.anchoredPosition.y;
            certificateArea.SetActive(false);
            awardListPanel.SetActive(false);

            closeButton.onClick.AddListener(OnClose);

            var results = awardSystemSO.EvaluateAwards();
            Show(results);
        }

        public void Show(List<AwardResult> results)
        {
            _results      = results;
            _playerResult = results.Find(r => r.isPlayerWinner);
            _isOpened     = false;

            if (_playerResult == null || _playerResult.awardType == AwardType.None)
            {
                gameObject.SetActive(false);
                Bus<AwardDialogueEvent>.Raise(new AwardDialogueEvent(AwardType.None));
                return;
            }

            envelopeArea.SetActive(true);
            certificateArea.SetActive(false);
            awardListPanel.SetActive(false);
            closeButton.gameObject.SetActive(false);
            gameObject.SetActive(true);

            dimBackground.alpha    = 0f;
            dimBackground.DOFade(0.7f, 0.3f);
            popupCanvasGroup.alpha = 0f;
            popupRoot.localScale   = Vector3.one * 0.85f;
            popupCanvasGroup.DOFade(1f, 0.3f);
            popupRoot.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

            arrowIcon.gameObject.SetActive(true);
            arrowIcon.DOKill();
            arrowIcon.DOAnchorPosY(20f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        public void TryOpenEnvelope()
        {
            if (_isOpened) return;
            OpenEnvelope();
        }

        private void OpenEnvelope()
        {
            _isOpened = true;
            arrowIcon.DOKill();
            arrowIcon.gameObject.SetActive(false);

            envelopeFlap.DOScaleY(-1f, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    envelopeArea.SetActive(false);
                    ShowContents();
                });
        }

        private void ShowContents()
        {
            Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(clapSound));
            var config = awardSystemSO.GetConfig(_playerResult.awardType);
            yearText.text       = $"{TurnManager.Instance?.CurrentYear}년도 시상식";
            awardNameText.text  = config.awardName;
            awardMentText.text  = config.awardMent;
            prizeMoneyText.text = $"상금 {config.prizeMoney:N0}원";

            certificateArea.SetActive(true);
            awardListPanel.SetActive(true);
            PopulateAwardList();

            certificateRect.anchoredPosition = new Vector2(certificateRect.anchoredPosition.x, _certOriginY - 300f);
            certificateRect.DOAnchorPosY(_certOriginY, 0.6f).SetEase(Ease.OutCubic);

            awardListRect.anchoredPosition = new Vector2(awardListRect.anchoredPosition.x, _listOriginY - 300f);
            awardListRect.DOAnchorPosY(_listOriginY, 0.6f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => closeButton.gameObject.SetActive(true));
        }

        /// <summary>
        /// 세이브에 저장된 밴드 이름. 없으면 기본값.
        /// </summary>
        private string GetPlayerBandName()
        {
            var saved = SaveManager.Instance?.Data?.bandName;
            return string.IsNullOrEmpty(saved) ? defaultBandName : saved;
        }

        /// <summary>
        /// 수상 판단에 사용된 곡 제목. 없으면 빈 문자열.
        /// </summary>
        private string GetPlayerAwardedSongTitle()
        {
            var song = awardSystemSO?.PlayerAwardedSong;
            if (!song.HasValue) return "";
            return song.Value.songName ?? "";
        }

        private void PopulateAwardList()
        {
            foreach (Transform child in awardListContent)
            {
                if (child.CompareTag("Header")) continue;
                Destroy(child.gameObject);
            }

            string playerBandName  = GetPlayerBandName();
            string playerSongTitle = GetPlayerAwardedSongTitle();

            foreach (var result in _results)
            {
                var config = awardSystemSO.GetConfig(result.awardType);
                if (config == null) continue;

                var slot = Instantiate(awardListSlotPrefab, awardListContent);

                string winnerName = result.isPlayerWinner ? playerBandName : result.npcWinnerName;
                string songTitle  = result.isPlayerWinner ? playerSongTitle : result.npcSongTitle;

                slot.Setup(
                    config.awardName,
                    winnerName,
                    songTitle
                );
            }
        }

        private void OnClose()
        {
            popupCanvasGroup.DOFade(0f, 0.25f);
            dimBackground.DOFade(0f, 0.25f);
            popupRoot.DOScale(0.85f, 0.25f).SetEase(Ease.InBack).OnComplete(() => {
                gameObject.SetActive(false);
                var awardType = _playerResult != null ? _playerResult.awardType : AwardType.None;
                Bus<AwardDialogueEvent>.Raise(new AwardDialogueEvent(awardType));
            });
        }
    }
}