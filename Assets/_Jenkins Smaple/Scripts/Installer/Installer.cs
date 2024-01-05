using System;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
    public enum EnviromentType
    {
        Develop,
        Release
    }
    
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
    
    [Serializable]
    public class BatchArguments
    {
        public EnviromentType BuildEnviroment;
        public BuildTarget BuildPlatform;
        public string BuildVersion;
        public int BuildVersionCode;
        public int BuildNumber;

        public bool canABB;
        public bool useKeystore;
        public string KeystoreAlias;
        public string KeystorePassword;
        public string KeystorePath;
    }

    [CreateAssetMenu(fileName = "Installer", menuName = "Jenkins/Installer")]
    public class Installer : ScriptableObject
    {
        public BatchArguments Arguments;

        [Header("Xcode Settings")]
        public iOSCapability useCapabilities;
        public ActivateType useSwiftLibraries;
        public ActivateType useBitCode;
    }
}