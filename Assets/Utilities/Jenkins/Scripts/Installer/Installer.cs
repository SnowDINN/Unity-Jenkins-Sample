using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	[Flags]
	public enum EnvironmentType
	{
		QA = 1,
		Develop = 2,
		Release = 4
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
	public class Symbol
	{
		public string SymbolCode;
		public EnvironmentType Type;
	}

	[Serializable]
	public class BatchArguments
	{
		public EnvironmentType buildEnvironment;
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
		public static List<string> defines = new()
			{ "PROJECT_ENVIRONMENT_QA", "PROJECT_ENVIRONMENT_DEVELOP", "PROJECT_ENVIRONMENT_RELEASE" };

		public BatchArguments Arguments;

		public iOSCapability useCapabilities;
		public ActivateType useSwiftLibraries;
		public ActivateType useBitCode;

		public List<Symbol> Symbols;

		public EnvironmentType DefineType
		{
			get
			{
				var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
				var fullSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

				for (var i = 0; i < defines.Count; i++)
					if (fullSymbolString.Contains(defines[i]))
						return i == 0 ? (EnvironmentType)1 : (EnvironmentType)(i * 2);

				return EnvironmentType.Develop;
			}
			set => SymbolBuildSettings(value);
		}

		public void SymbolBuildSettings(EnvironmentType type)
		{
			var buildSymbol = type switch
			{
				EnvironmentType.QA => defines[0],
				EnvironmentType.Develop => defines[1],
				EnvironmentType.Release => defines[2],
				_ => defines[0]
			};

			var symbols = new List<string> { buildSymbol };
			symbols.AddRange(from symbol in Symbols
				where symbol.Type.HasFlag(type)
				select symbol.SymbolCode);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
				$"{string.Join(";", symbols)}");
		}
	}
}