using UnityEditor;
using UnityEngine;

public static class FoldoutHeader
{
	public static bool ShowHeader(string title, bool display)
	{
		var style = new GUIStyle("ShurikenModuleTitle")
		{
			padding =
			{
				left = 5
			},
			border =
			{
				top = 10,
				bottom = 10,
				left = 5,
				right = 5
			},
			fixedHeight = 25,
			fontSize = 13
		};

		var rect = GUILayoutUtility.GetRect(25, 25, style);
		GUI.Box(rect, title, style);

		var evt = Event.current;
		switch (evt.type)
		{
			case EventType.Repaint:
				EditorStyles.foldout.Draw(new Rect(rect.x + 5.5f, rect.y + 2.5f, 15, 15), false, false, display, false);
				break;
			case EventType.MouseDown when rect.Contains(evt.mousePosition):
				display = !display;
				evt.Use();
				break;
		}

		return display;
	}
}