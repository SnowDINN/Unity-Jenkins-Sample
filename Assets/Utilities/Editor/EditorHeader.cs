using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	public static class EditorHeader
	{
		private const float alignmentPosition = 12;
		private const float alignmentWidth = 10;
		
		public static void Title(string message, int fontSize)
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

		public static void Line(int height)
		{
			var rect = EditorGUILayout.GetControlRect(false, height);
			rect.x -= alignmentPosition;
			rect.width += alignmentWidth;
			rect.height = height;

			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}
	}
}