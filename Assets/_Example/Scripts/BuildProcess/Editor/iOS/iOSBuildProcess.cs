#if !UNITY_IOS
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Anonymous.Jenkins.BuildProcess
{
    public static class iOSBuildProcess
    {
        private const string DefaultAccessLevel = "Default";
        private const string AuthenticationServicesFramework = "AuthenticationServices.framework";
        private const BindingFlags NonPublicInstanceBinding = BindingFlags.NonPublic | BindingFlags.Instance;
        private static string ProjectPath = string.Empty;
        private static string PbxProjectPath = string.Empty;

        private static Installer installer;

        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
                return;

            installer = Resources.Load("Jenkins/Installer") as Installer;

            ProjectPath = path;
            PbxProjectPath = PBXProject.GetPBXProjectPath(path);

            ModifyProject(SetBuildPhase);
            ModifyProject(AddLinkerFlag);
            ModifyProject(AddCapability);
            ModifyProject(SetSwiftLibraries);
            ModifyProject(SetBitcode);

            ModifyPlist(AddUrlScheme);
        }

        private static void ModifyProject(Action<PBXProject> modifier)
        {
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(PbxProjectPath));
            modifier(project);

            File.WriteAllText(PbxProjectPath, project.WriteToString());
        }

        private static void ModifyPlist(Action<PlistDocument> modifier)
        {
            var plistInfoFile = new PlistDocument();
            var infoPlistPath = Path.Combine(ProjectPath, "Info.plist");
            plistInfoFile.ReadFromString(File.ReadAllText(infoPlistPath));
            modifier(plistInfoFile);

            File.WriteAllText(infoPlistPath, plistInfoFile.WriteToString());
        }

        private static void AddUrlScheme(PlistDocument plist)
        {
            var CFBundleURLTypes = "CFBundleURLTypes";
            var CFBundleURLSchemes = "CFBundleURLSchemes";

            if (!plist.root.values.ContainsKey(CFBundleURLTypes))
                plist.root.CreateArray(CFBundleURLTypes);

            var cFBundleURLTypesElem = plist.root.values[CFBundleURLTypes] as PlistElementArray;
            var getSocialUrlSchemesArray = new PlistElementArray();
            var getSocialSchemeElem = cFBundleURLTypesElem.AddDict();
            getSocialSchemeElem.values[CFBundleURLSchemes] = getSocialUrlSchemesArray;
        }

        private static void SetBuildPhase(PBXProject project)
        {
            project.AddHeadersBuildPhase(project.GetUnityFrameworkTargetGuid());

            var header = project.FindFileGuidByProjectPath("UnityFramework/UnityFramework.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);

            header = project.FindFileGuidByProjectPath("Classes/UnityAppController.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);

            header = project.FindFileGuidByProjectPath("Classes/RedefinePlatforms.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);

            header = project.FindFileGuidByProjectPath("Classes/UndefinePlatforms.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);

            header = project.FindFileGuidByProjectPath("Classes/PluginBase/RenderPluginDelegate.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);

            header = project.FindFileGuidByProjectPath("Classes/PluginBase/LifeCycleListener.h");
            project.AddPublicHeaderToBuild(project.GetUnityFrameworkTargetGuid(), header);
        }

        private static void AddLinkerFlag(PBXProject project)
        {
            project.ReadFromString(File.ReadAllText(PbxProjectPath));
            var buildTarget = project.GetUnityMainTargetGuid();

            project.AddBuildProperty(buildTarget, "OTHER_LDFLAGS", "-all_load");
        }

        private static void AddCapability(PBXProject project)
        {
            var mainTarget = project.GetUnityMainTargetGuid();
            var manager =
                new ProjectCapabilityManager(PbxProjectPath, "Unity-iPhone.entitlements", "Unity-iPhone", mainTarget);

            if (installer.iOSDeepLink.Length > 0)
                manager.AddAssociatedDomains(installer.iOSDeepLink);

            if (installer.useCapabilities.HasFlag(iOSCapability.InAppPurchase))
                manager.AddInAppPurchase();

            if (installer.useCapabilities.HasFlag(iOSCapability.PushNotifications))
                manager.AddPushNotifications(true);

            if (installer.useCapabilities.HasFlag(iOSCapability.SignInWithApple))
                manager.AddSignInWithApple(project.GetUnityFrameworkTargetGuid());

            if (installer.useCapabilities.HasFlag(iOSCapability.GameCenter))
                manager.AddGameCenter();

            manager.WriteToFile();
        }

        private static void SetSwiftLibraries(PBXProject project)
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

        private static void SetBitcode(PBXProject project)
        {
            var value = installer.useBitCode == ActivateType.Yes ? "YES" : "NO";

            var mainBuildTarget = project.GetUnityMainTargetGuid();
            project.SetBuildProperty(mainBuildTarget, "ENABLE_BITCODE", value);

            var testBuildTarget = project.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            project.SetBuildProperty(testBuildTarget, "ENABLE_BITCODE", value);

            var unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
            project.SetBuildProperty(unityFrameworkTarget, "ENABLE_BITCODE", value);
        }

        public static void AddSignInWithApple(this ProjectCapabilityManager manager,
            string unityFrameworkTargetGuid = null)
        {
            var managerType = typeof(ProjectCapabilityManager);
            var capabilityTypeType = typeof(PBXCapabilityType);

            var projectField = managerType.GetField("project", NonPublicInstanceBinding);
            var targetGuidField = managerType.GetField("m_TargetGuid", NonPublicInstanceBinding);
            var entitlementFilePathField = managerType.GetField("m_EntitlementFilePath", NonPublicInstanceBinding);
            var getOrCreateEntitlementDocMethod =
                managerType.GetMethod("GetOrCreateEntitlementDoc", NonPublicInstanceBinding);
            var constructorInfo = capabilityTypeType.GetConstructor(
                NonPublicInstanceBinding,
                null,
                new[] { typeof(string), typeof(bool), typeof(string), typeof(bool) },
                null);

            if (projectField == null || targetGuidField == null || entitlementFilePathField == null ||
                getOrCreateEntitlementDocMethod == null || constructorInfo == null)
                throw new Exception("Can't Add Sign In With Apple programatically in this Unity version");

            var entitlementFilePath = entitlementFilePathField.GetValue(manager) as string;
            if (getOrCreateEntitlementDocMethod.Invoke(manager, new object[] { }) is PlistDocument entitlementDoc)
            {
                var plistArray = new PlistElementArray();
                plistArray.AddString(DefaultAccessLevel);
                entitlementDoc.root["com.apple.developer.applesignin"] = plistArray;
            }

            if (projectField.GetValue(manager) is PBXProject project)
            {
                var mainTargetGuid = targetGuidField.GetValue(manager) as string;
                var capabilityType = constructorInfo.Invoke(new object[]
                    { "com.apple.developer.applesignin.custom", true, string.Empty, true }) as PBXCapabilityType;

                var targetGuidToAddFramework = unityFrameworkTargetGuid;
                if (targetGuidToAddFramework == null)
                    targetGuidToAddFramework = mainTargetGuid;

                project.AddFrameworkToProject(targetGuidToAddFramework, AuthenticationServicesFramework, true);
                project.AddCapability(mainTargetGuid, capabilityType, entitlementFilePath);
            }
        }

        public static void AddGameCenter(this ProjectCapabilityManager manager)
        {
            var managerType = typeof(ProjectCapabilityManager);
            var capabilityTypeType = typeof(PBXCapabilityType);

            var projectField = managerType.GetField("project", NonPublicInstanceBinding);
            var targetGuidField = managerType.GetField("m_TargetGuid", NonPublicInstanceBinding);
            var entitlementFilePathField = managerType.GetField("m_EntitlementFilePath", NonPublicInstanceBinding);
            var getOrCreateEntitlementDocMethod =
                managerType.GetMethod("GetOrCreateEntitlementDoc", NonPublicInstanceBinding);
            var constructorInfo = capabilityTypeType.GetConstructor(
                NonPublicInstanceBinding,
                null,
                new[] { typeof(string), typeof(bool), typeof(string), typeof(bool) },
                null);

            if (projectField == null || targetGuidField == null || entitlementFilePathField == null ||
                getOrCreateEntitlementDocMethod == null || constructorInfo == null)
                throw new Exception("Can't Add Game Center programatically in this Unity version");

            var entitlementFilePath = entitlementFilePathField.GetValue(manager) as string;
            if (getOrCreateEntitlementDocMethod.Invoke(manager, new object[] { }) is PlistDocument entitlementDoc)
                entitlementDoc.root.SetBoolean("com.apple.developer.game-center", true);

            if (projectField.GetValue(manager) is PBXProject project)
            {
                var mainTargetGuid = targetGuidField.GetValue(manager) as string;
                var capabilityType =
                    constructorInfo.Invoke(new object[]
                        { "com.apple.developer.game-center", true, string.Empty, true }) as PBXCapabilityType;

                project.AddCapability(mainTargetGuid, capabilityType, entitlementFilePath);
            }
        }
    }
}
#endif