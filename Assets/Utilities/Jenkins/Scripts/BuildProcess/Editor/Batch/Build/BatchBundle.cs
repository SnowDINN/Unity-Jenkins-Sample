using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Anonymous.Jenkins
{
	public class BatchBundle
	{
		private const string bundleDataAsset = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
		private const string bundleSettingsAsset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
		public const string profile = "Assets";

		private static string path => Path.GetFullPath(Path.Combine(Application.dataPath, @".."));

		public static void Build(Installer installer)
		{
			Bundle();
		}

		private static void Bundle()
		{
			var settings =
				AssetDatabase.LoadAssetAtPath<ScriptableObject>(bundleSettingsAsset) as AddressableAssetSettings;
			if (settings != null)
			{
				var id = settings.profileSettings.GetProfileId(profile);
				if (!string.IsNullOrEmpty(id))
					settings.activeProfileId = id;

				if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(bundleDataAsset) is not IDataBuilder builderScript)
					return;

				var index = settings.DataBuilders.IndexOf((ScriptableObject)builderScript);
				if (index > 0)
					settings.ActivePlayerDataBuilderIndex = index;
			}

			AddressableAssetSettings.BuildPlayerContent();
		}
	}
}