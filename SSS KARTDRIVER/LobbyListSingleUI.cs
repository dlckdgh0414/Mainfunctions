using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text hostNameText;
    [SerializeField] private Button joinButton;

    private Lobby lobby;

    private void Awake()
    {
        joinButton.onClick.AddListener(HandleJoinClick);
    }

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        Player hostPlayer = lobby.Players.Find(p => p.Id == lobby.HostId);
        if (hostPlayer != null && hostPlayer.Data.ContainsKey(LobbyManager.KEY_PLAYER_NAME))
        {
            hostNameText.text = hostPlayer.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        }
    }

    private void HandleJoinClick()
    {
        if (lobby != null)
        {
            Debug.Log($"{lobby.Name} 로비 참가 시도");
            LobbyManager.Instance.JoinLobby(lobby);
        }
    }
}
