using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	[CustomEditor(typeof(Installer))]
	public class InstallerEidtor : Editor
	{
		private const float alignmentPosition = 12;
		private const float alignmentWidth = 10;
		private bool foldoutAndroid;
		private bool foldoutIOS;

		private Installer installer;

		private void OnEnable()
		{
			installer = Resources.Load("Jenkins/Installer") as Installer;
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

			installer.Arguments.BuildVersion =
				EditorGUILayout.TextField("Version", installer.Arguments.BuildVersion);
			installer.Arguments.BuildNumber =
				EditorGUILayout.IntField("Number", installer.Arguments.BuildNumber);

			EditorGUILayout.Space(20);
			foldoutIOS = Foldout("iOS Settings", foldoutIOS);
			if (foldoutIOS)
			{
				EditorGUILayout.BeginVertical(GUI.skin.GetStyle("GroupBox"));
				
				AddTitle("Build Properties Settings", 15);
				installer.useSwiftLibraries =
					(ActivateType)EditorGUILayout.EnumPopup("Use SwiftLibraries", installer.useSwiftLibraries);
				installer.useBitCode = (ActivateType)EditorGUILayout.EnumPopup("Use BitCode", installer.useBitCode);

				EditorGUILayout.Space(10);
				
				AddTitle("Capabilities Settings", 15);
				installer.useCapabilities =
					(iOSCapability)EditorGUILayout.EnumFlagsField("Use Capabilities", installer.useCapabilities);
				
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