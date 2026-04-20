using System;
using Code.Core;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.TutorialEvents;
using Code.MainSystem.NewMainScreen.Alarm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainSystem.NewMainScreen.MVP.View
{
    public class ScheduleView : MonoBehaviour, IScheduleView
    {
        [Header("App Bar")]
        [SerializeField] private Button scheduleManageBtn;
        [SerializeField] private Button bandManageBtn;

        [Header("Activity Button Groups")]
        [SerializeField] private GameObject scheduleActivityGroup;
        [SerializeField] private GameObject bandActivityGroup;

        [Header("Activity Buttons - 일정관리")]
        [SerializeField] private Button concertBtn;
        [SerializeField] private Button songProductionBtn;
        [SerializeField] private Button partTimeBtn;

        [Header("Activity Buttons - 밴드관리")]
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button treeBtn;
        [SerializeField] private Button promotionBtn;

        [Header("음악 업로드")]
        [SerializeField] private Button musicUploadBtn;

        [Header("ScheduleRegistrationUI")]
        [SerializeField] private ScheduleRegistrationUI concertRegisteredScheduleBG;
        [SerializeField] private ScheduleRegistrationUI songRegisteredScheduleBG;
        [SerializeField] private GameObject scheduleUIBar;

        [Header("UI Panels")]
        [SerializeField] private GameObject appBar;
        [SerializeField] private GameObject startBar;
        [SerializeField] private GameObject memberListBar;
        [SerializeField] private GameObject cellPhoneTimeObj;
        [SerializeField] private GameObject treeObj;
        [SerializeField] private GameObject selectAppObj;
        [SerializeField] private GameObject participationInfoPanel;
        [SerializeField] private TextMeshProUGUI participationInfoText;

        [Header("Action Buttons")]
        [SerializeField] private Button registerBtn;
        [SerializeField] private Button startAllBtn;
        [SerializeField] private Button cancelBtn;

        [Header("Cell Phone Text")]
        [SerializeField] private TextMeshProUGUI cellPhoneTMP;

        [Header("Alarm")]
        [SerializeField] private AlarmNotificationUI alarmNotificationUI;

        public event Action<ManagementBtnType> OnActivityClicked;
        public event Action OnRegisterClicked;
        public event Action OnStartAllClicked;
        public event Action OnCancelClicked;
        public event Action OnScheduleGroupEntered;
        public event Action OnCancelFromSelectApp;

        private bool _isInGroup;
        private bool _lastGroupWasSchedule;

        private void Awake()
        {
            scheduleManageBtn.onClick.AddListener(OnScheduleManageClicked);
            bandManageBtn.onClick.AddListener(OnBandManageClicked);

            concertBtn.onClick.AddListener(() =>
            {
                HideGroups();
                OnActivityClicked?.Invoke(ManagementBtnType.Concert);
            });
            songProductionBtn.onClick.AddListener(() =>
            {
                HideGroups();
                OnActivityClicked?.Invoke(ManagementBtnType.Song);
            });
            if (partTimeBtn != null)
                partTimeBtn.onClick.AddListener(() =>
                {
                    HideGroups();
                    OnActivityClicked?.Invoke(ManagementBtnType.PartTime);
                });

            shopBtn.onClick.AddListener(() =>
            {
                HideGroups();
                OnActivityClicked?.Invoke(ManagementBtnType.Shop);
            });
            treeBtn.onClick.AddListener(() =>
            {
                HideGroups();
                OnActivityClicked?.Invoke(ManagementBtnType.Tree);
            });
            if (promotionBtn != null)
                promotionBtn.onClick.AddListener(() =>
                {
                    HideGroups();
                    OnActivityClicked?.Invoke(ManagementBtnType.Promotion);
                });

            if (musicUploadBtn != null)
            {
                musicUploadBtn.onClick.AddListener(() =>
                    OnActivityClicked?.Invoke(ManagementBtnType.MusicUpload));
            }

            registerBtn.onClick.AddListener(() => OnRegisterClicked?.Invoke());
            startAllBtn.onClick.AddListener(() => OnStartAllClicked?.Invoke());
            cancelBtn.onClick.AddListener(OnCancelBtnClicked);

            startBar.SetActive(false);
            participationInfoPanel.SetActive(false);
            memberListBar.SetActive(false);
            startAllBtn.gameObject.SetActive(false);
            scheduleActivityGroup.SetActive(false);
            bandActivityGroup.SetActive(false);
        }

        private void OnDestroy()
        {
            scheduleManageBtn.onClick.RemoveAllListeners();
            bandManageBtn.onClick.RemoveAllListeners();
            concertBtn.onClick.RemoveAllListeners();
            songProductionBtn.onClick.RemoveAllListeners();
            if (partTimeBtn != null)
                partTimeBtn.onClick.RemoveAllListeners();
            shopBtn.onClick.RemoveAllListeners();
            treeBtn.onClick.RemoveAllListeners();
            if (promotionBtn != null)
                promotionBtn.onClick.RemoveAllListeners();
            if (musicUploadBtn != null)
                musicUploadBtn.onClick.RemoveAllListeners();
            registerBtn.onClick.RemoveAllListeners();
            startAllBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.RemoveAllListeners();
        }

        private void HideGroups()
        {
            _isInGroup = false;
            scheduleActivityGroup.SetActive(false);
            bandActivityGroup.SetActive(false);
        }

        private void OnCancelBtnClicked()
        {
            if (selectAppObj.activeSelf)
            {
                OnCancelFromSelectApp?.Invoke();
                return;
            }

            if (_isInGroup)
            {
                Bus<TutorialReturnButtonClickedEvent>.Raise(new TutorialReturnButtonClickedEvent());
                ReturnToSelectApp();
            }
            else
            {
                ReturnToLastGroup();
                OnCancelClicked?.Invoke();
            }
        }

        private void OnScheduleManageClicked()
        {
            Bus<TutorialManageTabOpenedEvent>.Raise(new TutorialManageTabOpenedEvent(TutorialManageTabType.ScheduleManage));
            _isInGroup = true;
            _lastGroupWasSchedule = true;
            selectAppObj.SetActive(false);
            scheduleActivityGroup.SetActive(true);
            bandActivityGroup.SetActive(false);
            if (scheduleUIBar != null) scheduleUIBar.SetActive(true);
            if (alarmNotificationUI != null) alarmNotificationUI.SetVisible(false);
            OnScheduleGroupEntered?.Invoke();
        }

        private void OnBandManageClicked()
        {
            Bus<TutorialManageTabOpenedEvent>.Raise(new TutorialManageTabOpenedEvent(TutorialManageTabType.BandManage));
            _isInGroup = true;
            _lastGroupWasSchedule = false;
            selectAppObj.SetActive(false);
            bandActivityGroup.SetActive(true);
            scheduleActivityGroup.SetActive(false);
            if (scheduleUIBar   != null) scheduleUIBar.SetActive(false);
            if (alarmNotificationUI != null) alarmNotificationUI.SetVisible(false);
            startAllBtn.gameObject.SetActive(false);
        }

        public void ReturnToLastGroup()
        {
            _isInGroup = true;
            selectAppObj.SetActive(false);
            if (_lastGroupWasSchedule)
            {
                scheduleActivityGroup.SetActive(true);
                bandActivityGroup.SetActive(false);
                if (scheduleUIBar != null) scheduleUIBar.SetActive(true);
            }
            else
            {
                scheduleActivityGroup.SetActive(false);
                bandActivityGroup.SetActive(true);
                if (scheduleUIBar   != null) scheduleUIBar.SetActive(false);
                startAllBtn.gameObject.SetActive(false);
            }
        }

        public void ForceEnterScheduleGroup()
        {
            _isInGroup = true;
            _lastGroupWasSchedule = true;
            appBar.SetActive(true);
            selectAppObj.SetActive(false);
            scheduleActivityGroup.SetActive(true);
            bandActivityGroup.SetActive(false);
            if (scheduleUIBar != null) scheduleUIBar.SetActive(true);
            if (alarmNotificationUI != null) alarmNotificationUI.SetVisible(false);
            OnScheduleGroupEntered?.Invoke();
        }

        private void ReturnToSelectApp()
        {
            _isInGroup = false;
            scheduleActivityGroup.SetActive(false);
            bandActivityGroup.SetActive(false);
            selectAppObj.SetActive(true);
            if (scheduleUIBar != null) scheduleUIBar.SetActive(false);
            if (alarmNotificationUI != null) alarmNotificationUI.SetVisible(true);
            startAllBtn.gameObject.SetActive(false);
        }

        public void ReturnToSelectAppForTutorial()
        {
            appBar.SetActive(true);
            ReturnToSelectApp();
            SetCellPhoneTimeVisible(true);
        }

        public void SetAppBarVisible(bool visible)
        {
            appBar.SetActive(visible);
            if (alarmNotificationUI != null) alarmNotificationUI.SetVisible(false);

            if (!visible)
            {
                selectAppObj.SetActive(false);
                scheduleActivityGroup.SetActive(false);
                bandActivityGroup.SetActive(false);
                if (scheduleUIBar != null) scheduleUIBar.SetActive(false);
                startAllBtn.gameObject.SetActive(false);
            }
        }

        public bool IsInScheduleGroup => _isInGroup && _lastGroupWasSchedule;

        public void SetTreeBarVisible(bool visible)           => treeObj.SetActive(visible);
        public void SetStartBarVisible(bool visible)          => startBar.SetActive(visible);
        public void SetMemberListBarVisible(bool visible)     => memberListBar.SetActive(visible);
        public void SetParticipationInfoVisible(bool visible) => participationInfoPanel.SetActive(visible);
        public void SetCellPhoneTimeVisible(bool visible)     => cellPhoneTimeObj.SetActive(visible);
        public void SetStartAllButtonVisible(bool visible)    => startAllBtn.gameObject.SetActive(visible);
        public void SetMusicUploadButtonVisible(bool visible)
        {
            if (musicUploadBtn == null) return;
            musicUploadBtn.gameObject.SetActive(true);
            musicUploadBtn.interactable = visible;
        }
        
        public void SetActivityButtonsInteractable(bool interactable)
        {
            if (concertBtn != null) concertBtn.interactable = interactable;
            if (songProductionBtn != null) songProductionBtn.interactable = interactable;
        }

        public void SetRegisteredScheduleBarVisible(bool visible)
        {
            if (scheduleUIBar != null)
                scheduleUIBar.SetActive(visible);
        }

        public void SetRegisterButtonInteractable(bool interactable)
            => registerBtn.interactable = interactable;

        public void SetParticipationInfoText(string text)
        {
            if (participationInfoText != null)
                participationInfoText.SetText(text);
        }

        public void SetCellPhoneText(string text)
        {
            if (cellPhoneTMP != null)
                cellPhoneTMP.SetText(text);
        }
    }
}