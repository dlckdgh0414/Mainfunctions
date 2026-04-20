using Code.Core;
using UnityEngine;

namespace Code.MainSystem.MusicProduction.Data
{
    [CreateAssetMenu(fileName = "ProductionData", menuName = "SO/Production/Data", order = 0)]
    public class MusicProductionDataSO : ScriptableObject
    {
        public AudioClip clip;
        public MusicGenreType genre;
        public MusicDirectionType direction;
        public MusicProductionUnionType unionType;
        public float playTime;
        public int spendGold = 100;

#if UNITY_EDITOR
        private void OnValidate()
        {
            string newName = $"{genre} - {direction}";
            if (name != newName)
            {
                name = newName;
                string path = UnityEditor.AssetDatabase.GetAssetPath(this);
                UnityEditor.AssetDatabase.RenameAsset(path, newName);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}