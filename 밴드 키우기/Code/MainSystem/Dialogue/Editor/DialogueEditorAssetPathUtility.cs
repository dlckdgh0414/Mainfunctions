using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Editor
{
    /// <summary>
    /// 다이얼로그 에디터의 에셋 경로 변환과 폴더 생성을 담당하는 유틸리티
    /// </summary>
    public static class DialogueEditorAssetPathUtility
    {
        public static string ConvertAbsoluteToAssetPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            string normalized = absolutePath.Replace("\\", "/");
            string assetsRoot = Application.dataPath.Replace("\\", "/");
            if (!normalized.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return "Assets" + normalized.Substring(assetsRoot.Length);
        }

        public static string ConvertAssetToAbsolutePath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            if (!assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string dataPath = Application.dataPath.Replace("\\", "/");
            string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
            return (projectRoot + assetPath).Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        public static bool EnsureAssetFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return false;
            }

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return true;
            }

            if (!folderPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] segments = folderPath.Split('/');
            if (segments.Length == 0 || !string.Equals(segments[0], "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string currentPath = "Assets";
            for (int i = 1; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (string.IsNullOrWhiteSpace(segment))
                {
                    continue;
                }

                string nextPath = $"{currentPath}/{segment}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segment);
                }

                currentPath = nextPath;
            }

            return AssetDatabase.IsValidFolder(folderPath);
        }
    }
}
