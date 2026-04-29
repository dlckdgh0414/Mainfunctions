using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CarTypeSprite
{
    public PlayerCartType carType;
    public Sprite carSprite;
}

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLavelText;
    [SerializeField] private Image playerImage;
    [SerializeField] private Button kickButton;

    [Header("Car Type Sprites")]
    [SerializeField] private CarTypeSprite[] carTypeSprites;

    private string playerId;
    private Dictionary<PlayerCartType, Sprite> carSpriteDict;

    private void Awake()
    {
        carSpriteDict = new Dictionary<PlayerCartType, Sprite>();
        foreach (var entry in carTypeSprites)
        {
            if (!carSpriteDict.ContainsKey(entry.carType))
                carSpriteDict.Add(entry.carType, entry.carSprite);
        }
    }

    public void Setup(Player player, bool isHost, bool isLocalPlayer)
    {
        this.playerId = player.Id;

        string playerName = player.Data.ContainsKey(LobbyManager.KEY_PLAYER_NAME)
            ? player.Data[LobbyManager.KEY_PLAYER_NAME].Value
            : "Unknown";
        playerNameText.text = playerName;


        playerLavelText.text = "Lv " + LobbyManager.Instance.thisPlayerDataCompo.GivePlayerData().Level.ToString();

        string carString = player.Data.ContainsKey(LobbyManager.KEY_PLAYER_CAR)
     ? player.Data[LobbyManager.KEY_PLAYER_CAR].Value
     : PlayerCartType.PracticeCart.ToString();

        if (Enum.TryParse(carString, out PlayerCartType carType))
        {
            playerImage.sprite = CarSpriteManager.Instance.GetCarSprite(carType);
        }
        else
        {
            playerImage.sprite = CarSpriteManager.Instance.GetCarSprite(PlayerCartType.PracticeCart);
        }


        if (isLocalPlayer || !isHost)
            kickButton.gameObject.SetActive(false);
        else
        {
            kickButton.gameObject.SetActive(true);
            kickButton.onClick.RemoveAllListeners();
            kickButton.onClick.AddListener(() => KickPlayer());
        }
    }


    private void KickPlayer()
    {
        LobbyManager.Instance.KickPlayer(playerId);
    }
}
