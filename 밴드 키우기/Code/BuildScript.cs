using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

 public class BuildScript
    {
        // Windows Standalone 빌드
        public static void BuildWindows()
        {
            string outputPath = GetArg("-outputPath") ?? "Build/Windows/game.exe";

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Windows Build Failed!");
            }

            Console.WriteLine("Windows Build Succeeded: " + outputPath);
        }

        // Android 빌드
        public static void BuildAndroid()
        {
            string outputPath = GetArg("-outputPath") ?? "Build/Android/game.apk";

            // 키스토어 설정 (있을 경우)
            string keystoreName = GetArg("-keystoreName");
            string keystorePass = GetArg("-keystorePass");
            string keyaliasName = GetArg("-keyaliasName");
            string keyaliasPass = GetArg("-keyaliasPass");

            if (!string.IsNullOrEmpty(keystoreName))
            {
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Android Build Failed!");
            }

            Console.WriteLine("Android Build Succeeded: " + outputPath);
        }

        // 빌드 설정에서 활성화된 씬 목록 가져오기
        private static string[] GetScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        // 커맨드라인 인자 파싱
        private static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return null;
        }
    }