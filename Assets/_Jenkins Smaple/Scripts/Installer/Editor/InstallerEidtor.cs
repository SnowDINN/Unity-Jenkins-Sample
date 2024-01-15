using System;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	[CustomEditor(typeof(Installer))]
	public class InstallerEidtor : Editor
	{
		private const float alignmentPosition = 12;
		private const float alignmentWidth = 10;

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

		private Installer installer;
		private SerializedObject serializedObject;

		private void OnEnable()
		{
			installer = target as Installer;
			serializedObject = new SerializedObject(installer);;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			AddTitle("Jenkins Installer", 30);
			if (GUILayout.Button("SAVE", GUILayout.Width(50), GUILayout.Height(40)))
			{
				EditorUtility.SetDirty(installer);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			EditorGUILayout.EndHorizontal();
			AddLine(2);

			EditorGUILayout.Space(10);
			AddTitle("Build Settings", 20);

			installer.DefineType =
				(EnviromentType)EditorGUILayout.EnumPopup("Define Symbol", installer.DefineType);
			installer.Arguments.BuildVersion =
				EditorGUILayout.TextField("Version", installer.Arguments.BuildVersion);
			installer.Arguments.BuildNumber =
				EditorGUILayout.IntField("Number", installer.Arguments.BuildNumber);

			EditorGUILayout.Space(20);
			foldoutAndroid = CategoryHeader.ShowHeader("Android Settings", foldoutAndroid);
			if (foldoutAndroid)
			{
			}

			foldoutIOS = CategoryHeader.ShowHeader("iOS Settings", foldoutIOS);
			if (foldoutIOS)
			{
				EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));

				AddTitle("Build property settings", 15);
				installer.useSwiftLibraries =
					(ActivateType)EditorGUILayout.EnumPopup("Use SwiftLibraries", installer.useSwiftLibraries);
				installer.useBitCode = (ActivateType)EditorGUILayout.EnumPopup("Use BitCode", installer.useBitCode);

				EditorGUILayout.Space(10);

				AddTitle("Capability settings", 15);
				installer.useCapabilities =
					(iOSCapability)EditorGUILayout.EnumFlagsField("Use Capabilities", installer.useCapabilities);

				EditorGUILayout.EndVertical();
			}

			foldoutSymbols = CategoryHeader.ShowHeader("Symbol Settings", foldoutSymbols);
			if (foldoutSymbols)
			{
				EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));

				AddTitle("Symbol property settings", 15);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("Symbols"), true);
				serializedObject.ApplyModifiedProperties();

				EditorGUILayout.EndVertical();
			}
		}

		private void AddTitle(string message, int fontSize)
		{
			var originalFontSize = GUI.skin.label.fontSize;
			var rect = EditorGUILayout.GetControlRect(false);
			rect.height += fontSize;

			GUI.skin.label.fontSize = fontSize;
			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUI.Label(rect, message);
			GUI.skin.label.fontStyle = FontStyle.Normal;
			GUI.skin.label.fontSize = originalFontSize;

			EditorGUILayout.Space(fontSize / 1.5f);
		}

		private void AddLine(int height)
		{
			var rect = EditorGUILayout.GetControlRect(false, height);
			rect.x -= alignmentPosition;
			rect.width += alignmentWidth;
			rect.height = height;

			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}
	}
}