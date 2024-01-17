using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anonymous.Jenkins
{
	[Flags]
	public enum EnviromentType
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
		public EnviromentType Type;
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
		public static List<string> defines = new()
			{ "PROJECT_ENVIROMENT_QA", "PROJECT_ENVIROMENT_DEVELOP", "PROJECT_ENVIROMENT_RELEASE" };

		public BatchArguments Arguments;

		public iOSCapability useCapabilities;
		public ActivateType useSwiftLibraries;
		public ActivateType useBitCode;

		public List<Symbol> Symbols;

		public EnviromentType DefineType
		{
			get
			{
				var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
				var fullSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

				for (var i = 0; i < defines.Count; i++)
					if (fullSymbolString.Contains(defines[i]))
						return i == 0 ? (EnviromentType)1 : (EnviromentType)(i * 2);

				return EnviromentType.Develop;
			}
			set => SymbolBuildSettings(value);
		}

		public void SymbolBuildSettings(EnviromentType type)
		{
			var buildSymbol = type switch
			{
				EnviromentType.QA => defines[0],
				EnviromentType.Develop => defines[1],
				EnviromentType.Release => defines[2],
				_ => defines[0]
			};

			var symbols = new List<string> { buildSymbol };
			symbols.AddRange(from symbol in Symbols
				where symbol.Type.HasFlag(type)
				select symbol.SymbolCode);

			if (symbols.Count > 1)
				for (var i = 0; i < symbols.Count; i++)
				{
					if (i == symbols.Count)
						return;

					symbols[i] = $"{symbols[i]};";
				}

			var scriptingDefine = symbols.Aggregate(string.Empty, (current, symbol) => current + symbol);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
				$"{scriptingDefine}");
		}
	}
}