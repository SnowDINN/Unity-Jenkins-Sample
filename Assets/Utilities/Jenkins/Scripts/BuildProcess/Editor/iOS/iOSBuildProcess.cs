#if UNITY_IOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Anonymous.Jenkins.BuildProcess
{
	public class iOSBuildProcess : IPostprocessBuildWithReport
	{
		private Installer installer;

		private string PbxProjectPath;
		private string ProjectPath;

		public int callbackOrder => 999;

		public void OnPostprocessBuild(BuildReport report)
		{
			OnPostProcessBuild(report.summary.platform, report.summary.outputPath);
		}

		private void OnPostProcessBuild(BuildTarget target, string path)
		{
			if (target != BuildTarget.iOS)
				return;

			installer = Resources.Load("Jenkins/Installer") as Installer;

			ProjectPath = path;
			PbxProjectPath = PBXProject.GetPBXProjectPath(path);

			ModifyProject(AddLinkerFlag);
			ModifyProject(AddCapability);
			ModifyProject(SetSwiftLibraries);
			ModifyProject(SetBitcode);
		}

		private void ModifyProject(Action<PBXProject> modifier)
		{
			var project = new PBXProject();
			project.ReadFromFile(PbxProjectPath);

			modifier(project);

			File.WriteAllText(PbxProjectPath, project.WriteToString());
		}

		private void ModifyPlist(Action<PlistDocument> modifier)
		{
			var plistInfoFile = new PlistDocument();
			var infoPlistPath = Path.Combine(ProjectPath, "Info.plist");
			plistInfoFile.ReadFromString(File.ReadAllText(infoPlistPath));
			modifier(plistInfoFile);

			File.WriteAllText(infoPlistPath, plistInfoFile.WriteToString());
		}

		private void AddLinkerFlag(PBXProject project)
		{
			project.ReadFromString(File.ReadAllText(PbxProjectPath));
			
			var mainBuildTarget = project.GetUnityMainTargetGuid();
			project.AddBuildProperty(mainBuildTarget, "OTHER_LDFLAGS", "-all_load");
		}

		private void AddCapability(PBXProject project)
		{
			var mainBuildTarget = project.GetUnityMainTargetGuid();
			var entitlementsFileName =
				project.GetBuildPropertyForAnyConfig(mainBuildTarget, "CODE_SIGN_ENTITLEMENTS");
			if (entitlementsFileName == null)
			{
				var bundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
				entitlementsFileName = string.Format("{0}.entitlements",
					bundleIdentifier.Substring(bundleIdentifier.LastIndexOf(".") + 1));
			}

			var manager = new ProjectCapabilityManager(
				PbxProjectPath,
				entitlementsFileName,
				"Unity-iPhone",
				mainBuildTarget
			);

			if (installer.useCapabilities.HasFlag(iOSCapability.PushNotifications))
				manager.AddPushNotifications(true);

			if (installer.useCapabilities.HasFlag(iOSCapability.InAppPurchase))
				manager.AddInAppPurchase();

			if (installer.useCapabilities.HasFlag(iOSCapability.SignInWithApple))
				manager.AddSignInWithApple();

			if (installer.useCapabilities.HasFlag(iOSCapability.GameCenter))
				manager.AddGameCenter();

			manager.WriteToFile();
		}

		private void SetSwiftLibraries(PBXProject project)
		{
			var value = installer.useSwiftLibraries == ActivateType.Yes ? "YES" : "NO";
			project.ReadFromString(File.ReadAllText(PbxProjectPath));

			var mainBuildTarget = project.GetUnityMainTargetGuid();
			project.SetBuildProperty(mainBuildTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", value);
			project.SetBuildProperty(mainBuildTarget, "VALIDATE_WORKSPACE", value);

			var unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
			project.SetBuildProperty(unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
			project.SetBuildProperty(unityFrameworkTarget, "VALIDATE_WORKSPACE", "NO");
		}

		private void SetBitcode(PBXProject project)
		{
			var value = installer.useBitCode == ActivateType.Yes ? "YES" : "NO";

			var mainBuildTarget = project.GetUnityMainTargetGuid();
			project.SetBuildProperty(mainBuildTarget, "ENABLE_BITCODE", value);

			var testBuildTarget = project.TargetGuidByName(PBXProject.GetUnityTestTargetName());
			project.SetBuildProperty(testBuildTarget, "ENABLE_BITCODE", value);

			var unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
			project.SetBuildProperty(unityFrameworkTarget, "ENABLE_BITCODE", value);
		}
	}
}
#endif