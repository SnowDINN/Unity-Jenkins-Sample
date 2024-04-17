using System;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	[CustomEditor(typeof(Installer))]
	public class InstallerEditor : Editor
	{
		private Installer installer;
		private SerializedObject serializedObject;

		private bool foldoutAndroid
		{
			get => Convert.ToBoolean(PlayerPrefs.GetInt("UNITY_EDITOR_FOLDOUT_ANDROID"));
			set => PlayerPrefs.SetInt("UNITY_EDITOR_FOLDOUT_ANDROID", Convert.ToInt32(value));
		}

		private bool foldoutIOS
		{
			get => Convert.ToBoolean(PlayerPrefs.GetInt("UNITY_EDITOR_FOLDOUT_iOS"));
			set => PlayerPrefs.SetInt("UNITY_EDITOR_FOLDOUT_iOS", Convert.ToInt32(value));
		}

		private bool foldoutSymbols
		{
			get => Convert.ToBoolean(PlayerPrefs.GetInt("UNITY_EDITOR_FOLDOUT_SYMBOL"));
			set => PlayerPrefs.SetInt("UNITY_EDITOR_FOLDOUT_SYMBOL", Convert.ToInt32(value));
		}

		private void OnEnable()
		{
			installer = target as Installer;
			serializedObject = new SerializedObject(installer);
		}

		[MenuItem("Utilities/On Select/Build Setting", false, 101)]
		public static void OnSelectBuildSetting()
		{
			Selection.activeObject =
				AssetDatabase.LoadMainAssetAtPath(
					"Assets/Utilities/Jenkins/Scripts/Installer/Resources/Jenkins/Installer.asset");
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorHeader.Title("Jenkins Installer", 30);
			if (GUILayout.Button("SAVE", GUILayout.Width(50), GUILayout.Height(40)))
			{
				EditorUtility.SetDirty(installer);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorHeader.Line(2);

			EditorGUILayout.Space();
			
			EditorHeader.Title("Build Settings", 20);

			installer.DefineType =
				(EnvironmentType)EditorGUILayout.EnumPopup("Define Symbol", installer.DefineType);
			installer.Arguments.BuildVersion =
				EditorGUILayout.TextField("Version", installer.Arguments.BuildVersion);
			installer.Arguments.BuildNumber =
				EditorGUILayout.IntField("Number", installer.Arguments.BuildNumber);

			EditorGUILayout.Space();
			foldoutAndroid = CategoryHeader.ShowHeader("Android Settings", foldoutAndroid);
			if (foldoutAndroid)
			{
			}

			foldoutIOS = CategoryHeader.ShowHeader("iOS Settings", foldoutIOS);
			if (foldoutIOS)
			{
				EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));

				EditorHeader.Title("Build property settings", 15);
				installer.useSwiftLibraries =
					(ActivateType)EditorGUILayout.EnumPopup("Use SwiftLibraries", installer.useSwiftLibraries);
				installer.useBitCode = (ActivateType)EditorGUILayout.EnumPopup("Use BitCode", installer.useBitCode);

				EditorGUILayout.Space();

				EditorHeader.Title("Capability settings", 15);
				installer.useCapabilities =
					(iOSCapability)EditorGUILayout.EnumFlagsField("Use Capabilities", installer.useCapabilities);

				EditorGUILayout.EndVertical();
			}

			foldoutSymbols = CategoryHeader.ShowHeader("Symbol Settings", foldoutSymbols);
			if (foldoutSymbols)
			{
				EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));

				EditorHeader.Title("Symbol property settings", 15);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("Symbols"), true);
				serializedObject.ApplyModifiedProperties();

				EditorGUILayout.Space();
				if (GUILayout.Button("Apply Symbols", GUILayout.Height(25)))
					installer.SymbolBuildSettings(installer.DefineType);
				
				EditorGUILayout.EndVertical();
			}
		}
	}
}