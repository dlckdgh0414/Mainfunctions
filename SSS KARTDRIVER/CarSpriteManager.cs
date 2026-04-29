using UnityEngine;
using System.Collections.Generic;

public class CarSpriteManager : MonoBehaviour
{
    public static CarSpriteManager Instance { get; private set; }

    [System.Serializable]
    public struct CarSpriteEntry
    {
        public PlayerCartType carType;
        public Sprite sprite;
    }

    [SerializeField] private List<CarSpriteEntry> carSprites;
    private Dictionary<PlayerCartType, Sprite> carSpriteDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        carSpriteDict = new Dictionary<PlayerCartType, Sprite>();
        foreach (var entry in carSprites)
            carSpriteDict[entry.carType] = entry.sprite;
    }

    public Sprite GetCarSprite(PlayerCartType type)
    {
        if (carSpriteDict.TryGetValue(type, out var sprite))
            return sprite;
        return null;
    }
}
