using Code.Core;
using UnityEngine;

namespace Code.SubSystem.Minigame.LyricsMiniGame
{
    [CreateAssetMenu(fileName = "Item", menuName = "SO/DropItem/Data", order = 0)]
    public class LyricsDropObjDataSO : ScriptableObject
    {
        public MusicProductionUnionType ItemType;
        public Sprite ItemSprite;
        public Color ItemOutlineColor;
        public float ItemOutlineThickness = 8f;

        public bool IsGoodItem => ItemType is MusicProductionUnionType.Good;
    }
}