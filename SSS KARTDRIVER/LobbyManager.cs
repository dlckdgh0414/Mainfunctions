using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using HN.Code.Networking;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; } //싱글턴을 만들기 위해 선언

    public const string KEY_PLAYER_NAME = "PlayerName"; //플레이어 이름 KEY
    public const string KEY_PLAYER_CAR = "Car"; //플레이어 자동차 KEY

    private string _playerName; //플레이어 이름을 받아줄 변수
    private PlayerCartType _playerCartType; //자동차 타입을 받아줄함수

    [field: SerializeField] public CurrentPlayerData thisPlayerDataCompo;

    /// <summary>
    /// EventHandler는 반환값이 Void인 델리게이트를 사용가능하다. 하지만 사용하기 위해서는 이벤트의 매개변수에목록 맞춰서 사용해야함
    /// 인자값으로 두개를 받는데 object sender, EventArgs e를 받는데 sender는 이벤트를 실행주고싶은 객체,EventArgs 전달하고 싶은 정보 
    /// </summary>
    public event EventHandler OnLeftLobby; //방을 나갈때 나가는 함수를 실행시켜준다.

    public event EventHandler<LobbyEventArgs>
        OnJoinedLobby; //로비를 들어올때 발행하는 이벤트 LobbyEventArgs에 Lobby 정보를 보내줘야한다.

    public event EventHandler<LobbyEventArgs>
        OnLobbyUpdated; //로비가 업데이트될때 발행하는 이벤트 LobbyEventArgs에 Lobby 정보를 보내줘야한다.

    public event EventHandler<LobbyEventArgs>
        OnKickedFromLobby; //로비에서 강퇴할때 발행하는 이벤트 LobbyEventArgs에 Lobby 정보를 보내줘야한다.

    public event EventHandler<OnLobbyListChangedEventArgs>
        OnLobbyListChanged; //로비갯수가 바뀔때 발행하는 이벤트 OnLobbyListChangedEventArgs LobbyList정보를 보내줘야한다.

    public class LobbyEventArgs : EventArgs //로비 정보를 받아줄 클래스
    {
        public Lobby lobby; //현재 자기 로비를 보내줘야한다.
    }

    public class OnLobbyListChangedEventArgs : EventArgs //로비 리스트가 바뀔때 로비 리스트 정보를 받아줄 클래스
    {
        public List<Lobby> lobbyList = new List<Lobby>(); //로비 리스트를 보내줘야한다.
    }

    private float
        heartbeatTimer =
            2.5f; //Unity Lobby가 30초동안 아무활동이없으면 닫아버리기때매 주기적으로 활동을 갱신하기 위해 만들어주기위해 사용한 변수(호스트만 사용)

    private float lobbyPollTimer = 2.5f; //로비 정보를 업데이트 받기 위해서 사용한 변수(모든 플레이어)
    private float refreshLobbyListTimer = 5f; //로비 새로고침시간

    private Lobby joinedLobby; //자기가 소속한 로비를 받아줄 변수

    private void Awake()
    {
        //싱글턴
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        _playerName = "Player_" + UnityEngine.Random.Range(1000, 9999);
        Authenticate(_playerName); //플레이어 초기화랑 로그인을 해줌
    }

    private void Start()
    {
        InitializePlayerName();
    }

    private void Update()
    {
        HandleLobbyHeartbeat(); //로비 활동을 갱신하는 함수
        HandleLobbyPolling(); //로비 정보를 업데이트해주는 함수
        //HandleRefreshLobbyList();
    }

    #region Authentication

    public async void Authenticate(string playerName)
    {
        _playerName = playerName; //플레이어 이름을 넣어준다.

        var options = new InitializationOptions().SetProfile(playerName); //프로필설정

        await UnityServices.InitializeAsync(options); //어드레서블 초기화

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //익명 로그인
    }

    #endregion

    public async void InitializePlayerName()
    {
        await thisPlayerDataCompo.SetPlayerData(_playerName);
    }

    #region Lobby Heartbeat & Polling

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost()) //로비 호스트일때
        {
            heartbeatTimer -= Time.deltaTime; //로비 활동 갱신 타이머 시작
            if (heartbeatTimer < 0f) //타이머가 0보다 작을때
            {
                heartbeatTimer = 15f; //타이머 초기화

                try
                {
                    await LobbyService.Instance
                        .SendHeartbeatPingAsync(joinedLobby.Id); //로비에게 로비 활동중이라고 보내며 로비를 살려놓는다.
                    Debug.Log("Heartbeat sent");
                }
                catch (Exception e)
                {
                    Debug.LogError("Heartbeat failed: " + e);
                }
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null) //로비에 들어왔을때
        {
            lobbyPollTimer -= Time.deltaTime; //로비 업데이트를 위한 타이머 시작
            if (lobbyPollTimer < 0f) //타이머가 0보다작으면
            {
                lobbyPollTimer = 1.1f; //타이머 초기화

                try
                {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id); //로비 정보를 받아준다.

                    OnLobbyUpdated?.Invoke(this,
                        new LobbyEventArgs { lobby = joinedLobby }); //로비가 업데이트가 되었다는 정보를 보내준다.

                    if (!IsPlayerInLobby()) //플레이어가 로비에 들어가있지않다면(킥당한거임)
                    {
                        Debug.Log("Kicked from Lobby!");
                        OnKickedFromLobby?.Invoke(this,
                            new LobbyEventArgs { lobby = joinedLobby }); //내가 킥이되었다는 정보를 보내준다.
                        joinedLobby = null; //가입된 로비가없다고 처리한다.
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning("Lobby polling failed: " + e);
                    joinedLobby = null;
                }
            }
        }
    }

    #endregion

    #region Lobby Management

    public Lobby GetJoinedLobby() => joinedLobby; //다른 스크립트에서 로비를 가져올때 사용하는 함수

    public bool IsLobbyHost() //로비 호스트가 맞는지 판별하는 함수
    {
        return joinedLobby != null &&
               joinedLobby.HostId ==
               AuthenticationService.Instance
                   .PlayerId; //현재 로비에 호스트ID와 플레이어 ID가 같은지확인하고 아니면 false맞으면 true를 보내준다.
    }

    private bool IsPlayerInLobby() //플레이어가 로비에 들어와있는지 확인하는 변수
    {
        if (joinedLobby == null) return false; //로비가없으면 false를 보내준다.

        foreach (Player player in joinedLobby.Players) //로비에있는 플레이어를 반복하면서 돌면서
        {
            if (player.Id == AuthenticationService.Instance.PlayerId) //현재 플레이어ID와 같은지 확인하고
                return true; //맞으면 true를 보내준다.
        }

        return false; //없다면 false를 보내준다.
    }

    private Player GetPlayer(string playerName = null, string selectedCart = "") //플레이어를 가져올때 사용하는 함수
    {
        try
        {
            if (string.IsNullOrEmpty(playerName)) //플레이어 이름이 없다면
                playerName = _playerName; //현재 이름을 넣어준다.

            return new Player(
                AuthenticationService.Instance.PlayerId, //현재 로그인한 플레이어의 고유 ID를 넣어주고
                null, //연결정보 
                new Dictionary<string, PlayerDataObject> //커스텀 플레이어 데이터
                {
                    {
                        KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                    }, //플레이어 KEY스트링을 딕셔너리 KEY값으로 넣고 플레이어 데이터를 생성해주고 공개여부와 데이터를 넣어준다.(플레이어 이름은 공개해도 괜찮기 때매 공개해둠)
                    {
                        KEY_PLAYER_CAR, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, selectedCart)
                    } //플레이어 자동차 KEY스트링을 딕셔너리 KEY값으로 넣고 플레이어 데이터를 생성해주고 공개여부와 데이터를 넣어준다.
                }
            );
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get player data: " + e);
            return null;
        }
    }


    public async void
        CreateLobby(string lobbyName, int maxPlayers) //로비를 만들때 사용하는 함수 로비이름과 최대 플레이어를 인자값으로 받는다.
    {
        if (UnityServices.State !=
            ServicesInitializationState
                .Initialized) //유니티 서버가 초기화 되어있지않으면 로비를 생성할수없기에 오류를 띄우고 만들수없게만든다.
        {
            Debug.LogError("Unity Services not initialized. Cannot create lobby.");
            return;
        }

        if (!AuthenticationService.Instance
                .IsSignedIn) //익명계정으로 로그인안하면 안되기때문에 만약 로그인 되어있지않다면 오류를 띄우고 로비를 만들수없게 만든다.
        {
            Debug.LogError("Not signed in. Cannot create lobby.");
            return;
        }

        try
        {
            var options = new CreateLobbyOptions //로비를 만들때 추가옵션
            {
                Player = GetPlayer(thisPlayerDataCompo.GivePlayerData().UserName,
                    thisPlayerDataCompo.GivePlayerData().CarType), //방장을 등록
                IsPrivate = false, //비공개하지않는다.
            };

            joinedLobby =
                await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,
                    options); //로비를 만든 뒤 자신이 들어와있는 로비안에 넣어준다.

            OnJoinedLobby?.Invoke(this,
                new LobbyEventArgs { lobby = joinedLobby }); //로비에 들어왔다는 이벤트를 호출해주며 로비 정보를 넘겨준다.

            Debug.Log("Lobby Created: " + joinedLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("CreateLobby failed: " + e);
        }
        catch (Exception e)
        {
            Debug.LogError("CreateLobby failed: " + e);
        }
    }

    public async void RefreshLobbyList() //로비 새로고침
    {
        if (UnityServices.State !=
            ServicesInitializationState
                .Initialized) //유니티 서버가 초기화 되어있지않으면 로비를 생성할수없기에 오류를 띄우고 새로고침할수없게만든다.
        {
            Debug.LogWarning("Unity Services not initialized yet. Cannot refresh lobby list.");
            return;
        }

        if (!AuthenticationService.Instance
                .IsSignedIn) //익명계정으로 로그인안하면 안되기때문에 만약 로그인 되어있지않다면 오류를 띄우고 새로고침을할수없게 만든다.
        {
            Debug.LogWarning("Not signed in. Cannot refresh lobby list.");
            return;
        }

        try
        {
            var options = new QueryLobbiesOptions //Unity Lobby에서 로비를 검색할 때 조건을 지정하는 옵션 클래스.
            {
                Count = 25, //최대로비
                Filters = new List<QueryFilter>
                {
                    //자리가 있는 로비만 가져오기위한 필터
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    )
                },
                Order = new List<QueryOrder> //정렬
                {
                    //가장 먼저 생성된 로비대로 정렬
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created
                    )
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options); //조건에 맞는 로비를 가져오기

            var validLobbies = new List<Lobby>(); //들어갈수있는 로비리스트를 생성
            foreach (var lobby in response.Results) //조건에 결과를 반복문을 돌린다.
            {
                if (!lobby.IsLocked && !string.IsNullOrEmpty(lobby.HostId)) //로비가 안잠겨있거나 호스트정보가없는 로비는 제외한다.
                {
                    validLobbies.Add(lobby); //들어갈수있는 리스트안에 넣어준다.
                }
            }

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = validLobbies
            }); //로비 리스트 정보를 보내준다.

            Debug.Log($"Lobby List Refreshed: {validLobbies.Count}/{response.Results.Count} (after filtering)");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("RefreshLobbyList failed: " + e);
        }
        catch (Exception e)
        {
            Debug.LogError("RefreshLobbyList failed: " + e);
        }
    }


    public async void JoinLobby(Lobby lobby) //로비를 들어갈때 사용하는 함수
    {
        if (UnityServices.State !=
            ServicesInitializationState
                .Initialized) //유니티 서버가 초기화 되어있지않으면 로비를 생성할수없기에 오류를 띄우고 들어갈수없게할수없게만든다.
        {
            Debug.LogError("Unity Services not initialized. Cannot join lobby.");
            return;
        }

        if (!AuthenticationService.Instance
                .IsSignedIn) //익명계정으로 로그인안하면 안되기때문에 만약 로그인 되어있지않다면 오류를 띄우고 참가할수없게 만든다.
        {
            Debug.LogError("Not signed in. Cannot join lobby.");
            return;
        }

        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id,
                new JoinLobbyByIdOptions //로비에들어가기전에 자신의 정보를 서버에 전달해주기 위해 만듬
                {
                    Player = GetPlayer(thisPlayerDataCompo.GivePlayerData().UserName,
                        thisPlayerDataCompo.GivePlayerData().CarType)
                });

            OnJoinedLobby?.Invoke(this,
                new LobbyEventArgs { lobby = joinedLobby }); //로비에 들어갔다는 이벤트를 보내주고 로비정보를 보내준다.

            Debug.Log("Joined Lobby: " + joinedLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("JoinLobby failed: " + e);
        }
        catch (Exception e)
        {
            Debug.LogError("JoinLobby failed: " + e);
        }
    }

    public async void KickPlayer(string playerId) //플레이어 킥을할 때 사용하는 함수 
    {
        if (!IsLobbyHost()) return; //호스트가 아니라면 킥을 할수없게 반환
        if (joinedLobby == null) return; //로비가 없다면 킥을 당할수없기에 반환

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId); //로비에서 플레이어 제거
            Debug.Log($"Kicked player: {playerId}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("KickPlayer failed: " + e);
        }
    }

    public async void LeaveLobby() //로비를 나갈때 사용하는 함수
    {
        if (joinedLobby == null) return; //로비에 들어가있지않다면 반환

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id,
                AuthenticationService.Instance.PlayerId); //로비에서 자기자신의 고유번호를 보내 자기 자신 제거

            Debug.Log("Left Lobby: " + joinedLobby.Name);

            joinedLobby = null; //들어가있던 로비를 없던 로비로 변경
            OnLeftLobby?.Invoke(this, EventArgs.Empty); //로비를 나간 이벤트 발행
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("LeaveLobby failed: " + e);
        }
    }

    public async void StartGame()
    {
        if (!IsLobbyHost() || joinedLobby == null) return;

        try
        {
            bool result = await HostSingleton.Instance.GameManager.MakeJoinCode(GetJoinedLobby().Players.Count);
            
            if (result == false)
            {
                Debug.LogError("게임 코드 생성 실패");
                return;
            }
            print($"JoinCode: {HostSingleton.Instance.JoinCode}");

            var updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") },
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, HostSingleton.Instance.JoinCode) }
                }
            };

            joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updateOptions);

            Debug.Log("Game started! Signal sent to all players.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("StartGame failed: " + e);
        }
    }

    #endregion

    #region Helpers

    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f)
            {
                refreshLobbyListTimer = 5f;
                RefreshLobbyList();
            }
        }
    }

    public bool IsServiceReady()
    {
        try
        {
            return UnityServices.State == ServicesInitializationState.Initialized &&
                   AuthenticationService.Instance.IsSignedIn;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}