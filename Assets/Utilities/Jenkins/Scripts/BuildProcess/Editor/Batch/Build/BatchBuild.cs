using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	public class BatchBuild
	{
		public static void Build(Installer installer)
		{
			var args = installer.Arguments;
			var path = ExistPath(args.BuildEnviroment, args.BuildPlatform);
			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = FindEnabledEditorScenes(),
				target = args.BuildPlatform,
				options = BuildOptions.None
			};
			buildPlayerOptions.locationPathName =
				buildPlayerOptions.target == BuildTarget.Android
					? $"{path}/Build.{(EditorUserBuildSettings.buildAppBundle ? "aab" : "apk")}"
					: $"{path}";

			if (installer != null)
				installer.SymbolBuildSettings(args.BuildEnviroment);
			ProjectBuildSettings(args);

			BuildPipeline.BuildPlayer(buildPlayerOptions);
		}

		private static string ExistPath(EnviromentType enviroment, BuildTarget platform)
		{
			var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, @".."));
			var path = $"{projectPath}/bin/{enviroment}/{platform}";
			var info = new DirectoryInfo(path);
			if (!info.Exists)
				info.Create();

			return path;
		}

		private static string[] FindEnabledEditorScenes()
		{
			return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
		}

		private static void ProjectBuildSettings(BatchArguments args)
		{
			PlayerSettings.SplashScreen.show = false;
			PlayerSettings.SplashScreen.showUnityLogo = false;

			PlayerSettings.iOS.appleEnableAutomaticSigning = true;
			PlayerSettings.iOS.appleDeveloperTeamID = "";

			PlayerSettings.bundleVersion = args.BuildVersion;
			EditorUserBuildSettings.buildAppBundle = args.canABB;

			PlayerSettings.Android.useCustomKeystore = args.useKeystore;
			if (PlayerSettings.Android.useCustomKeystore)
			{
				PlayerSettings.Android.keystoreName = args.KeystorePath;
				PlayerSettings.Android.keystorePass = args.KeystorePassword;
				PlayerSettings.Android.keyaliasName = args.KeystoreAlias;
				PlayerSettings.Android.keyaliasPass = args.KeystorePassword;
			}

			PlayerSettings.Android.bundleVersionCode = args.BuildVersionCode;
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
		}
	}
}