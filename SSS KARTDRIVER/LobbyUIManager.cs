using System;
using System.Collections;
using System.Collections.Generic;
using HN.Code.Core;
using HN.Code.Networking;
using HN.Code.References;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField] private GameObject makingPanel;
    [SerializeField] private HotkeyInputHandler hotkeyInputHandler;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TransitionManager transitionManager;
    [SerializeField] private StartFade fade;

    public static LobbyUIManager Instance;

    private Lobby currentLobby;

    private void Start()
    {
        Instance = this;

        lobbyPanel.SetActive(false);
        leaveButton.onClick.AddListener(OnLeaveLobbyClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);


        LobbyManager.Instance.OnJoinedLobby += HandleJoinedLobby;
        LobbyManager.Instance.OnLobbyUpdated += HandleLobbyUpdated;
        LobbyManager.Instance.OnLeftLobby += HandleLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += HandleKickedFromLobby;
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance == null) return;

        LobbyManager.Instance.OnJoinedLobby -= HandleJoinedLobby;
        LobbyManager.Instance.OnLobbyUpdated -= HandleLobbyUpdated;
        LobbyManager.Instance.OnLeftLobby -= HandleLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= HandleKickedFromLobby;
    }

    #region 이벤트 핸들러

    private void HandleJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        ShowLobby(e.lobby);
    }

    private void HandleLobbyUpdated(object sender, LobbyManager.LobbyEventArgs e)
    {
        RefreshLobby(e.lobby);

        if (e.lobby.Data != null && e.lobby.Data.ContainsKey("GameStarted"))
        {
            if (e.lobby.Data["GameStarted"].Value == "true")
            {
                Debug.Log("Game Start detected! Loading PlayScene...");

                bool isHost = LobbyManager.Instance.IsLobbyHost();
                int playerCnt = LobbyManager.Instance.GetJoinedLobby().Players.Count;
                ApplicationManager.Instance.SetData(playerCnt, isHost);

                if (isHost)
                {

                    LockLobby(e.lobby.Id);
                    TransitionAndFade(1, 0.3f, 0, () =>
                    {
                        if (HostSingleton.Instance.GameManager.StartHost())
                        {
                            NetworkManager.Singleton.SceneManager.LoadScene(SceneNames.GameScene, LoadSceneMode.Single);
                        }
                    });
                }
                else if(NetworkManager.Singleton.IsClient == false)
                {
                    TransitionAndFade(1, 0.3f, 0, () =>
                    {
                        string joinCode = e.lobby.Data["JoinCode"].Value;
                        _ = ClientSingleton.Instance.GameManager.StartClientWithJoinCode(joinCode);
                    });
                }
            }
        }
    }

    private async void LockLobby(string lobbyId)
    {
        try
        {
            var updateOptions = new UpdateLobbyOptions
            {
                IsLocked = true
            };

            Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
            Debug.Log("Lobby locked. No more players can join.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to lock lobby: {e}");
        }
    }

    private void TransitionAndFade(float endValue, float duration, float delay, Action onComplete = null)
    {
        transitionManager.Transition(() =>
        {
            fade.Fade(endValue, duration, delay, onComplete);
        });
    }

    private void HandleLeftLobby(object sender, System.EventArgs e)
    {
        HideLobby();
    }

    private void HandleKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Debug.Log("You were kicked from the lobby!");
        HideLobby();
    }

    #endregion

    #region UI 표시/갱신

    private void ShowLobby(Lobby lobby)
    {
        currentLobby = lobby;
        lobbyPanel.SetActive(true);
        hotkeyInputHandler.menuType = MenuType.gameLobby;
        makingPanel.SetActive(false);

        lobbyNameText.text = lobby.Name;

        RefreshPlayerList();
        UpdateHostUI();
    }

    private void RefreshLobby(Lobby lobby)
    {
        currentLobby = lobby;
        RefreshPlayerList();
        UpdateHostUI();
    }

    private void HideLobby()
    {
        currentLobby = null;
        lobbyPanel.SetActive(false);
        hotkeyInputHandler.menuType = MenuType.mainLobby;

        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);
    }

    #endregion

    #region 플레이어 리스트

    private void RefreshPlayerList()
    {
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        foreach (var player in currentLobby.Players)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);

            bool isHost = LobbyManager.Instance.IsLobbyHost();
            bool isLocalPlayer = player.Id == AuthenticationService.Instance.PlayerId;

            PlayerEntry playerEntry = entry.GetComponent<PlayerEntry>();
            if (playerEntry != null)
            {
                playerEntry.Setup(player, isHost, isLocalPlayer);
            }
            else
            {
                Debug.LogWarning("PlayerEntry 컴포넌트가 PlayerEntryPrefab에 없음!");
            }
        }
    }

    #endregion

    #region 버튼 동작

    private void OnLeaveLobbyClicked()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    private void OnStartGameClicked()
    {
        if (LobbyManager.Instance.IsLobbyHost())
        {
            LobbyManager.Instance.StartGame();
        }
    }

    #endregion

    #region 호스트 전용 UI

    private void UpdateHostUI()
    {
        if (LobbyManager.Instance.IsLobbyHost())
        {
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
    }

    #endregion
}