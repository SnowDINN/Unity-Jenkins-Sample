using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Anonymous.Jenkins
{
    public class Jenkins
    {
        public static void BuildAndroid()
        {
            var installer = Resources.Load("Jenkins/Installer") as Installer;
            
            Application.logMessageReceived += LogMessageReceived;
            BuildCommonSettings(installer);
            
            EditorUtility.SetDirty(installer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = FindEnabledEditorScenes(),
                locationPathName = $"{installer.BuildExportPath}/{installer.BuildFileName}",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                    break;
                case BuildResult.Failed:
                    Debug.Log("Build failed");
                    break;
            }

            Application.logMessageReceived -= LogMessageReceived;
        }

        public static void BuildIOS()
        {
            var installer = Resources.Load("Jenkins/Installer") as Installer;
            
            Application.logMessageReceived += LogMessageReceived;
            BuildCommonSettings(installer);
            
            EditorUtility.SetDirty(installer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = FindEnabledEditorScenes(),
                locationPathName = $"{installer.BuildExportPath}",
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                    break;
                case BuildResult.Failed:
                    Debug.Log("Build failed");
                    break;
            }

            Application.logMessageReceived -= LogMessageReceived;
        }

        private static void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Exception:
                    Debug.Log($"Exception : {condition}\n{stackTrace}");
                    break;

                case LogType.Error:
                    Debug.Log($"Error : {condition}\n{stackTrace}");
                    break;
            }
        }

        private static string GetArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (args[i] == name && args.Length > i + 1)
                    return args[i + 1];
            return null;
        }

        private static string[] FindEnabledEditorScenes()
        {
            return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        }

        private static void BuildCommonSettings(Installer installer)
        {
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.bundleVersion = installer.BuildVersion;

#if UNITY_ANDROID
            PlayerSettings.Android.bundleVersionCode = installer.BuildNumber;
#elif UNITY_IOS
            PlayerSettings.iOS.buildNumber = $"{installer.BuildNumber}";
#endif

#if UNITY_ANDROID
            if (!installer.useCusomKeystore)
                return;
            
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = installer.KeyStorePath;
            PlayerSettings.Android.keystorePass = installer.KeyStorePassword;
            PlayerSettings.Android.keyaliasName = installer.KeyAliasName;
            PlayerSettings.Android.keyaliasPass = installer.KeyAliasPassword;
#endif
        }
    }
}