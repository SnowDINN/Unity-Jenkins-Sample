using System;
using UnityEngine;

namespace Anonymous.Jenkins
{
    public enum PathType
    {
        InProject,
        External
    }

    public enum DialogType
    {
        Folder,
        File
    }

    public enum KeyStoreType
    {
        Debug,
        Custom
    }

    [Flags]
    public enum iOSCapability
    {
        InAppPurchase = 1,
        PushNotifications = 2,
        SignInWithApple = 4,
        GameCenter = 8
    }

    public enum ActivateType
    {
        No,
        Yes
    }

    [CreateAssetMenu(fileName = "Installer", menuName = "Jenkins/Installer")]
    public class Installer : ScriptableObject
    {
        public PathType BuildPathType;
        public string BuildExportPath;
        public string BuildAddExportPath;
        public string BuildFileName;
        public string BuildVersion;
        public int BuildNumber;

        public KeyStoreType KeyStoreType;
        public PathType KeyStorePathType;
        public string KeyStorePath;
        public string KeyStoreAddPath;
        public string KeyStorePassword;
        public string KeyAliasName;
        public string KeyAliasPassword;

        public string[] iOSDeepLink;
        public ActivateType useSwiftLibraries;
        public ActivateType useBitCode;
        public iOSCapability useCapabilities;
    }
}