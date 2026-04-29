using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }

    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;

    private void Awake()
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.onClick.AddListener(HandleRefreshButtonClick);
        createLobbyButton.onClick.AddListener(HandleCreateButtonClick);
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += HandleLobbyChanged;
        LobbyManager.Instance.OnJoinedLobby += HandleJoinLobby;
        LobbyManager.Instance.OnLeftLobby += HandleLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += HandleKickLobby;

        Show();
    }

    private void HandleKickLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void HandleLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void HandleJoinLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void HandleLobbyChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in container)
        {
            if (child == lobbySingleTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);

            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void HandleRefreshButtonClick()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void HandleCreateButtonClick()
    {
        LobbyCreateUi_8s.Instance.Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
