using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.Core.Bus.GameEvents.DialogueEvents.Audio;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.SubSystem.BandFunds;
using Code.MainSystem.MusicProduction.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.MusicProduction
{
    public class MusicProductionPopupUI : MonoBehaviour
    {
        [SerializeField] private Button musicGenresBtn;
        [SerializeField] private Button musicDirectionBtn;
        [SerializeField] private Button musicProductionBtn;
        [SerializeField] private Button selectBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button previewMusicBtn;
        [SerializeField] private Button previewStopBtn;
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private MusicGenresUI musicGenresUI;
        [SerializeField] private MusicDirectionUI musicDirectionUI;
        [SerializeField] private TextMeshProUGUI musicGenresName;
        [SerializeField] private TextMeshProUGUI musicGenresDescription;
        [SerializeField] private GameObject musicProductionDetailsUI;
        [SerializeField] private TextMeshProUGUI musicGenresDesc;
        [SerializeField] private TextMeshProUGUI musicDirectionDesc;
        [SerializeField] private TextMeshProUGUI spendGoldText;
        [SerializeField] private AudioSource previewAudioSource;

        private const float PreviewDuration = 30f;

        private MusicDirectionType _currentMusicDirectionType;
        private MusicGenreType _currentMusicGenreType;
        private MusicDifficultyType _currentMusicDifficultyType;
        private string _currentGenreName;
        private string _currentDirectionName;
        private AudioClip _currentPreviewClip;
        private float _currentPreviewStartTime;

        private float _previewEndTime = -1f;

        private bool _genreSelected     = false;
        private bool _directionSelected = false;

        // 서브 UI(장르/방향성 패널)가 열려있는지 추적.
        // 닫히지 않은 채로 팝업이 숨겨졌다가 다시 표시될 때 버튼 잠금이
        // 풀리지 않는 문제를 방지하기 위해 사용한다.
        private bool _subUIOpen = false;

        private static readonly string[] GoodMessages =
        {
            "음악 조합이 매우 좋습니다.\n좋은 평가를 받을거 같습니다.",
            "환상의 궁합이다!\n팬들이 열광할 예감.",
            "장르와 방향성이 서로를 살려준다.",
            "이 조합은 대박 예감이 든다…",
            "완벽한 합. 명곡의 향기가 난다.",
        };

        private static readonly string[] BadMessages =
        {
            "음악 조합이 어딘가 어색합니다.\n평가가 좋지 않을 것 같습니다.",
            "물과 기름 같은 조합이다…",
            "이건 좀 위험한데.\n팬들이 고개를 갸웃할지도.",
            "장르와 방향성이 따로 논다.",
            "뭔가 삐걱거리는 느낌이다.",
        };

        private void Awake()
        {
            musicDirectionBtn.onClick.AddListener(HandleShowDrctionUI);
            musicGenresBtn.onClick.AddListener(HandleShowGenresUI);
            musicGenresUI.OnMusicGenresSelected     += SetMusicGenreType;
            musicDirectionUI.OnChangeMusicDirection += SetMusicDirectionType;
            musicProductionBtn.onClick.AddListener(HandleShowUI);
            selectBtn.onClick.AddListener(HandleSelectMusic);
            closeBtn.onClick.AddListener(HandleClose);
            previewMusicBtn.onClick.AddListener(HandlePreviewMusic);
            previewStopBtn.onClick.AddListener(HandleStopPreview);

            musicGenresName.SetText("없음");
            musicGenresDescription.SetText("없음");
            spendGoldText.SetText("제작비 : -");

            closeBtn.gameObject.SetActive(false);
            popupPanel.SetActive(false);
            selectBtn.gameObject.SetActive(false);
            previewMusicBtn.interactable = false;
            previewStopBtn.interactable  = false;

            musicGenresUI.OnHide    += OnSubUIHidden;
            musicDirectionUI.OnHide += OnSubUIHidden;

            Bus<MusicUploadEvent>.OnEvent += OnMusicUploaded;
        }

        private async void Start()
        {
            // 다른 매니저들이 Start 끝낸 후 복원 (한 프레임 대기)
            await UniTask.Yield();
            RestoreInProgressUI();
        }

        private void RestoreInProgressUI()
        {
            var mpm = MusicProductionManager.Instance;
            if (mpm == null || !mpm.HasMusicData)
            {
                Debug.Log($"[Popup] 복원 스킵. mpm={mpm!=null}, HasMusicData={mpm?.HasMusicData}");
                return;
            }

            musicProductionBtn.interactable = false;

            if (musicProductionDetailsUI != null)
                musicProductionDetailsUI.SetActive(false);

            var save = Code.SubSystem.Save.SaveManager.Instance;
            string genreName     = save?.Data?.musicGenreName     ?? mpm.GetCurrentMusicGenreType().ToString();
            string directionName = save?.Data?.musicDirectionName ?? mpm.GetCurrentMusicDirectionType().ToString();

            _currentGenreName     = genreName;
            _currentDirectionName = directionName;

            if (musicGenresDesc != null)
                musicGenresDesc.SetText("음악 장르 : " + genreName);
            if (musicDirectionDesc != null)
                musicDirectionDesc.SetText("음악 방향성 : " + directionName);

            Debug.Log($"[Popup] 기존 제작 중인 곡 UI 복원: {genreName}/{directionName}");
        }

        private void Update()
        {
            if (_previewEndTime > 0f && Time.unscaledTime >= _previewEndTime)
            {
                StopPreviewInternal();
            }
        }

        /// <summary>
        /// 팝업 패널이 활성화될 때 서브 UI가 닫힌 상태라면
        /// 버튼 잠금을 강제로 해제한다. 튜토리얼 등의 외부 흐름으로
        /// OnHide 콜백이 누락된 경우를 보완한다.
        /// </summary>
        private void OnEnable()
        {
            // popupPanel 자체가 아닌 이 컴포넌트 GameObject가 켜질 때 호출되므로,
            // 팝업이 열린 상태에서 다시 활성화된 경우만 처리한다.
            if (popupPanel != null && popupPanel.activeSelf)
                EnsureSubUIsClosed();
        }

        /// <summary>
        /// 서브 UI(장르/방향성 패널)가 실제로 모두 닫혀 있는지 확인하고,
        /// 열려 있다면 강제로 닫은 뒤 버튼 잠금을 해제한다.
        /// </summary>
        private void EnsureSubUIsClosed()
        {
            bool genresOpen    = musicGenresUI    != null && musicGenresUI.gameObject.activeSelf;
            bool directionOpen = musicDirectionUI != null && musicDirectionUI.gameObject.activeSelf;

            if (genresOpen)    musicGenresUI.gameObject.SetActive(false);
            if (directionOpen) musicDirectionUI.gameObject.SetActive(false);

            if (genresOpen || directionOpen || _subUIOpen)
            {
                Debug.Log("[Popup] 서브 UI 강제 닫기 → 버튼 잠금 해제");
                _subUIOpen = false;
                UnlockPopupButtons();
            }
        }

        private void LockPopupButtons()
        {
            _subUIOpen = true;
            musicGenresBtn.interactable    = false;
            musicDirectionBtn.interactable = false;
            selectBtn.interactable         = false;
            closeBtn.interactable          = false;
        }

        private void UnlockPopupButtons()
        {
            musicGenresBtn.interactable    = true;
            musicDirectionBtn.interactable = true;
            closeBtn.interactable          = true;
            selectBtn.interactable = _genreSelected && _directionSelected;
        }

        /// <summary>
        /// 장르/방향성 서브 UI가 닫혔을 때 호출되는 통합 콜백.
        /// _subUIOpen 플래그를 내리고 버튼 잠금을 해제한다.
        /// </summary>
        private void OnSubUIHidden()
        {
            _subUIOpen = false;
            UnlockPopupButtons();
        }

        private void OnMusicUploaded(MusicUploadEvent evt)
        {
            MusicProductionManager.Instance.ResetMusicData();
            musicProductionBtn.interactable = true;
            if (musicProductionDetailsUI != null) musicProductionDetailsUI.SetActive(true);
            if (musicGenresDesc    != null) musicGenresDesc.SetText("없음");
            if (musicDirectionDesc != null) musicDirectionDesc.SetText("없음");

            // 장르/방향성 이름도 클리어하고 즉시 저장
            var save = Code.SubSystem.Save.SaveManager.Instance;
            if (save != null)
            {
                save.Data.musicGenreName     = string.Empty;
                save.Data.musicDirectionName = string.Empty;
                save.ForceSaveNow();
            }
        }

        private void HandleShowUI()
        {
            if (MusicProductionManager.Instance.HasMusicData) return;
            popupPanel.SetActive(true);
            closeBtn.gameObject.SetActive(true);
            // 팝업을 새로 열 때 서브 UI 잠금 상태를 초기화한다
            _subUIOpen = false;
            UnlockPopupButtons();
            ResetSelections();
        }

        private void HandleSelectMusic()
        {
            if (!_genreSelected || !_directionSelected)
            {
                Debug.LogWarning("장르와 음악 방향을 모두 선택해주세요.");
                return;
            }

            var data = MusicProductionManager.Instance.GetData(_currentMusicGenreType, _currentMusicDirectionType);
            int cost = data != null ? data.spendGold : 0;

            if (cost > 0)
            {
                if (BandSupplyManager.Instance == null || !BandSupplyManager.Instance.CheckBandFunds(cost))
                {
                    Debug.LogWarning($"[MusicProductionPopupUI] 자금 부족 (필요: {cost} G)");
                    Bus<SystemMessageEvent>.Raise(new SystemMessageEvent(SystemMessageIconType.Warning, "자금이 부족합니다."));
                    return;
                }
                BandSupplyManager.Instance.SpendBandFunds(cost);
            }

            // 이름을 Setup 인자로 직접 전달 → Setup 내부에서 저장까지 처리
            MusicProductionManager.Instance.Setup(
                _currentMusicGenreType,
                _currentMusicDirectionType,
                _currentMusicDifficultyType,
                _currentGenreName,
                _currentDirectionName);

            RaiseUnionEvent(data);

            Bus<TutorialMusicConfiguredEvent>.Raise(new TutorialMusicConfiguredEvent(
                _currentMusicGenreType,
                _currentMusicDirectionType,
                _currentMusicDifficultyType));

            StopPreviewInternal();
            popupPanel.SetActive(false);
            closeBtn.gameObject.SetActive(false);
            musicProductionBtn.interactable = false;

            if (musicGenresDesc    != null) musicGenresDesc.SetText("음악 장르 : " + _currentGenreName);
            if (musicDirectionDesc != null) musicDirectionDesc.SetText("음악 방향성 : " + _currentDirectionName);
            if (musicProductionDetailsUI != null) musicProductionDetailsUI.SetActive(false);
        }

        private void RaiseUnionEvent(MusicProductionDataSO data)
        {
            if (data == null) return;

            switch (data.unionType)
            {
                case MusicProductionUnionType.Good:
                {
                    string msg = GoodMessages[UnityEngine.Random.Range(0, GoodMessages.Length)];
                    Bus<ShowTextEvent>.Raise(new ShowTextEvent(msg));
                    break;
                }
                case MusicProductionUnionType.Bad:
                {
                    string msg = BadMessages[UnityEngine.Random.Range(0, BadMessages.Length)];
                    Bus<ShowTextEvent>.Raise(new ShowTextEvent(msg));
                    break;
                }
            }
        }

        private void HandleClose()
        {
            StopPreviewInternal();
            popupPanel.SetActive(false);
            closeBtn.gameObject.SetActive(false);
            // 팝업 닫을 때 서브 UI 잠금 상태도 초기화
            _subUIOpen = false;
        }

        private void HandlePreviewMusic()
        {
            if (_currentPreviewClip == null) return;
            
            Bus<BGMStopEvnet>.Raise(new BGMStopEvnet());

            float startTime = Mathf.Clamp(_currentPreviewStartTime, 0f, Mathf.Max(0f, _currentPreviewClip.length - 0.01f));
            float duration  = Mathf.Min(PreviewDuration, _currentPreviewClip.length - startTime);

            previewAudioSource.Stop();
            previewAudioSource.clip = _currentPreviewClip;
            previewAudioSource.time = startTime;
            previewAudioSource.Play();

            _previewEndTime = Time.unscaledTime + duration;
            previewStopBtn.interactable = true;
        }

        private void HandleStopPreview()
        {
            StopPreviewInternal();
        }

        private void StopPreviewInternal()
        {
            previewAudioSource.Stop();
            previewStopBtn.interactable = false;
            Bus<PlayBGMEvent>.Raise(new PlayBGMEvent());
            _previewEndTime = -1f;
        }

        private void TryUpdateMusicData()
        {
            if (!_genreSelected || !_directionSelected) return;

            var data = MusicProductionManager.Instance.GetData(_currentMusicGenreType, _currentMusicDirectionType);
            if (data != null)
            {
                spendGoldText.SetText($"제작비 : {data.spendGold} G");
                _currentPreviewClip          = data.clip;
                _currentPreviewStartTime     = data.playTime;
                previewMusicBtn.interactable = data.clip != null;
            }
            else
            {
                spendGoldText.SetText("제작비 : -");
                _currentPreviewClip          = null;
                _currentPreviewStartTime     = 0f;
                previewMusicBtn.interactable = false;
            }

            StopPreviewInternal();
        }

        private void SetMusicDirectionType(MusicDirectionListData obj)
        {
            _currentMusicDirectionType = obj.musicDirectionType;
            _currentDirectionName      = obj.musicDirectionName;
            musicGenresDescription.SetText(obj.musicDirectionName);
            _directionSelected = true;
            UpdateSelectBtn();
            TryUpdateMusicData();
        }

        private void SetMusicGenreType(MusicGenreType arg1, MusicDifficultyType arg2, string arg3)
        {
            _currentMusicGenreType      = arg1;
            _currentMusicDifficultyType = arg2;
            _currentGenreName           = arg3;
            musicGenresName.SetText(arg3);
            _genreSelected = true;
            UpdateSelectBtn();
            TryUpdateMusicData();
        }

        private void UpdateSelectBtn()
        {
            selectBtn.gameObject.SetActive(true);
            selectBtn.interactable = _genreSelected && _directionSelected;
        }

        private void ResetSelections()
        {
            _genreSelected     = false;
            _directionSelected = false;
            selectBtn.gameObject.SetActive(false);
            musicGenresName.SetText("없음");
            musicGenresDescription.SetText("없음");
            spendGoldText.SetText("제작비 : -");
            _currentPreviewClip          = null;
            _currentPreviewStartTime     = 0f;
            previewMusicBtn.interactable = false;
            previewStopBtn.interactable  = false;
            StopPreviewInternal();
        }

        private void HandleShowGenresUI()
        {
            selectBtn.gameObject.SetActive(false);
            musicGenresUI.ShowUI();
            LockPopupButtons();
        }

        private void HandleShowDrctionUI()
        {
            selectBtn.gameObject.SetActive(false);
            musicDirectionUI.ShowUI();
            LockPopupButtons();
        }

        private void OnDestroy()
        {
            musicDirectionBtn.onClick.RemoveAllListeners();
            musicGenresBtn.onClick.RemoveAllListeners();
            musicDirectionUI.OnChangeMusicDirection -= SetMusicDirectionType;
            musicGenresUI.OnMusicGenresSelected     -= SetMusicGenreType;
            musicGenresUI.OnHide                    -= OnSubUIHidden;
            musicDirectionUI.OnHide                 -= OnSubUIHidden;
            musicProductionBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.RemoveAllListeners();
            previewMusicBtn.onClick.RemoveAllListeners();
            previewStopBtn.onClick.RemoveAllListeners();
            Bus<MusicUploadEvent>.OnEvent -= OnMusicUploaded;
        }
    }
}