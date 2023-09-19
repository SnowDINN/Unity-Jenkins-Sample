using System.IO;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
    [CustomEditor(typeof(Installer))]
    public class InstallerEidtor : Editor
    {
        private readonly float alignmentPosition = 12;
        private readonly float alignmentWidth = 10;
        private SerializedObject deepLinks;
        private bool foldoutAndroid;
        private bool foldoutIOS;

        private Installer installer;

        private void OnEnable()
        {
            installer = Resources.Load("Jenkins/Installer") as Installer;
            deepLinks = new SerializedObject(installer);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            addTitle("Jenkins Installer", 30);
            if (GUILayout.Button("SAVE", GUILayout.Width(50), GUILayout.Height(40)))
            {
                EditorUtility.SetDirty(installer);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();
            addLine(2);

            EditorGUILayout.Space(20);
            addTitle("Path Settings", 15);

            installer.BuildPathType = (PathType)EditorGUILayout.EnumPopup($"{name} Path", installer.BuildPathType);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
            addPath(ref installer.BuildPathType, ref installer.BuildExportPath, ref installer.BuildAddExportPath,
                DialogType.Folder, "Build Folder", "");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);
            addTitle("Build Settings", 15);

            installer.BuildVersion =
                EditorGUILayout.TextField("Version", installer.BuildVersion);
            installer.BuildNumber =
                EditorGUILayout.IntField("Number", installer.BuildNumber);


            EditorGUILayout.Space(20);
            foldoutAndroid = Foldout("Android Settings", foldoutAndroid);
            if (foldoutAndroid)
            {
                addTitle("Build File Name", 15);
                installer.BuildFileName =
                    EditorGUILayout.TextField("File Name", installer.BuildFileName);

                EditorGUILayout.Space(20);
                addTitle("KeyStore Settings", 15);
                installer.KeyStoreType =
                    (KeyStoreType)EditorGUILayout.EnumPopup($"{name} Path Type", installer.KeyStoreType);
                switch (installer.KeyStoreType)
                {
                    case KeyStoreType.Debug:
                        PlayerSettings.Android.useCustomKeystore = false;
                        break;
                    case KeyStoreType.Custom:
                        PlayerSettings.Android.useCustomKeystore = true;

                        installer.KeyStorePathType =
                            (PathType)EditorGUILayout.EnumPopup($"{name} Path", installer.KeyStorePathType);
                        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
                        addPath(ref installer.KeyStorePathType, ref installer.KeyStorePath,
                            ref installer.KeyStoreAddPath,
                            DialogType.File, "Keystore File", "keysotre");

                        EditorGUILayout.Space(10);
                        installer.KeyStorePassword =
                            EditorGUILayout.PasswordField("KeyStore Password", installer.KeyStorePassword);
                        installer.KeyAliasName =
                            EditorGUILayout.TextField("KeyAlias Name", installer.KeyAliasName);
                        installer.KeyAliasPassword =
                            EditorGUILayout.PasswordField("KeyAlias Password", installer.KeyAliasPassword);
                        EditorGUILayout.EndVertical();
                        break;
                }
            }

            EditorGUILayout.Space(10);
            foldoutIOS = Foldout("iOS Settings", foldoutIOS);
            if (foldoutIOS)
            {
                addTitle("Build Properties Settings", 15);
                installer.useSwiftLibraries =
                    (ActivateType)EditorGUILayout.EnumPopup("Use SwiftLibraries", installer.useSwiftLibraries);
                installer.useBitCode = (ActivateType)EditorGUILayout.EnumPopup("Use BitCode", installer.useBitCode);

                addTitle("Capabilities Settings", 15);
                installer.useCapabilities =
                    (iOSCapability)EditorGUILayout.EnumFlagsField("Use Capabilities", installer.useCapabilities);

                addTitle("Deep Links Settings", 15);
                deepLinks.Update();

                var deepLinksProperty = deepLinks.FindProperty("iOSDeepLink");
                EditorGUILayout.PropertyField(deepLinksProperty, true);
                deepLinks.ApplyModifiedProperties();
            }
        }

        private void addTitle(string message, int fontSize)
        {
            var originalFontSize = GUI.skin.label.fontSize;
            var rect = EditorGUILayout.GetControlRect(false);
            rect.x -= alignmentPosition;
            rect.width += alignmentWidth;
            rect.height += fontSize;

            GUI.skin.label.fontSize = fontSize;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.Label(rect, message);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.fontSize = originalFontSize;

            EditorGUILayout.Space(fontSize / 1.5f);
        }

        private void addLine(int height)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.x -= alignmentPosition;
            rect.width += alignmentWidth;
            rect.height = height;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private void addPath(ref PathType pathType, ref string path, ref string addPath, DialogType dialogType,
            string name, string extension)
        {
            switch (pathType)
            {
                case PathType.InProject:
                    path = projectPath();

                    addPath = EditorGUILayout.TextField("Add Path", addPath);
                    path += string.IsNullOrEmpty(addPath) ? "" : $"\\{addPath}";

                    GUILayout.Label($"{name} Path", EditorStyles.boldLabel);
                    GUI.enabled = false;
                    path = EditorGUILayout.TextArea(path);
                    GUI.enabled = true;
                    break;

                case PathType.External:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"{name} Path", EditorStyles.boldLabel);
                    if (GUILayout.Button("...", GUILayout.Width(25), GUILayout.Height(17.5f)))
                    {
                        var dialogpath = string.Empty;
                        switch (dialogType)
                        {
                            case DialogType.Folder:
                                dialogpath = EditorUtility.OpenFolderPanel($"Select {name}", "", "");
                                break;
                            case DialogType.File:
                                dialogpath = EditorUtility.OpenFilePanel($"Select {name}", "", extension);
                                break;
                        }

                        if (dialogpath.Length != 0)
                            path = dialogpath;
                    }

                    EditorGUILayout.EndHorizontal();
                    path = EditorGUILayout.TextArea(path);
                    break;
            }
        }

        private string projectPath()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, @".."));
            var path = $"{projectPath}";
            return path;
        }

        public static bool Foldout(string title, bool display)
        {
            var style = new GUIStyle("ShurikenModuleTitle")
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(5, 0, 0, 0),
                border = new RectOffset(7, 7, 4, 4),
                fixedHeight = 25,
                fontSize = 13
            };

            var rect = GUILayoutUtility.GetRect(25, 25, style);
            GUI.Box(rect, title, style);

            var evt = Event.current;
            if (evt.type == EventType.Repaint)
                EditorStyles.foldout.Draw(new Rect(rect.x + 5.5f, rect.y + 2.5f, 15, 15), false, false, display, false);

            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                display = !display;
                evt.Use();
            }

            return display;
        }
    }
}