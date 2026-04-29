using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Lobbies.Models;

public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }

    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;
    [SerializeField] private Button createButton;
    [SerializeField] private Button closeButton;

    private bool isCreating = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnJoinedLobby -= OnLobbyCreated;
            LobbyManager.Instance.OnJoinedLobby += OnLobbyCreated;
        }

        createButton.onClick.AddListener(HandleCreateClick);
        closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnJoinedLobby -= OnLobbyCreated;
        }
    }

    private void HandleCreateClick()
    {
        if (isCreating) return; 
        isCreating = true;

        string lobbyName = lobbyNameInputField.text.Trim();
        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogError("로비 이름을 입력해주세요!");
            isCreating = false;
            return;
        }

        int maxPlayers = 4;
        if (int.TryParse(maxPlayersInputField.text, out int parsed))
        {
            maxPlayers = Mathf.Clamp(parsed, 2, 100);
        }

        if (LobbyManager.Instance == null)
        {
            Debug.LogError("LobbyManager가 준비되지 않았습니다!");
            isCreating = false;
            return;
        }

        if (!IsServiceReady())
        {
            Debug.Log("서비스 초기화 중... 인증을 시작합니다.");
            StartCoroutine(WaitAndCreateLobby(lobbyName, maxPlayers));
            return;
        }

        Debug.Log($"로비 생성 시도: {lobbyName} ({maxPlayers}명)");
        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers);
        Hide();
    }

    private System.Collections.IEnumerator WaitAndCreateLobby(string lobbyName, int maxPlayers)
    {
        float timeout = 10f;
        while (timeout > 0f && !IsServiceReady())
        {
            yield return new WaitForSeconds(0.5f);
            timeout -= 0.5f;
        }

        if (IsServiceReady())
        {
            Debug.Log($"인증 완료! 로비 생성: {lobbyName} ({maxPlayers}명)");
            LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers);
            Hide();
        }
        else
        {
            Debug.LogError("인증 시간 초과!");
            isCreating = false;
        }
    }

    private bool IsServiceReady()
    {
        try
        {
            return Unity.Services.Core.UnityServices.State == Unity.Services.Core.ServicesInitializationState.Initialized &&
                   Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn;
        }
        catch
        {
            return false;
        }
    }

    private void OnLobbyCreated(object sender, LobbyManager.LobbyEventArgs args)
    {
        Debug.Log("로비가 생성되었습니다!");
        isCreating = false; 
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isCreating = false;
        if (lobbyNameInputField != null)
            lobbyNameInputField.text = "";
        if (maxPlayersInputField != null)
            maxPlayersInputField.text = "4";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
