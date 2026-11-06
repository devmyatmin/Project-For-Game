using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Beam.AccountServices;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Language;

public class LocalizationHandler : ILocalizationHandler, IDisposable
{
	public class LocalizationEntry
	{
		public string Term;

		public string Text;

		public string ReplacementTerm;

		public override bool Equals(object obj)
		{
			return obj?.GetHashCode().Equals(GetHashCode()) ?? false;
		}

		public override int GetHashCode()
		{
			return Term.GetHashCode();
		}
	}

	private static readonly string DEBUG_HEADER = "<color=cyan>LocalizationHandler::</color> ";

	public const string EN_DEFAULT_CODE = "en-DEFAULT";

	public const string FR_DEFAULT_CODE = "fr-DEFAULT";

	public const string IT_DEFAULT_CODE = "it-DEFAULT";

	public const string DE_DEFAULT_CODE = "de-DEFAULT";

	public const string ES_DEFAULT_CODE = "es-DEFAULT";

	public const string RU_DEFAULT_CODE = "ru-DEFAULT";

	public const string EN_DEFAULT_NAME = "English (Official)";

	public const string FR_DEFAULT_NAME = "French (Official)";

	public const string IT_DEFAULT_NAME = "Italian (Official)";

	public const string DE_DEFAULT_NAME = "German (Official)";

	public const string ES_DEFAULT_NAME = "Spanish (Official)";

	public const string RU_DEFAULT_NAME = "Russian (Official)";

	private Dictionary<string, string> _entries;

	private HashSet<Language> _languages = new HashSet<Language>
	{
		new Language("English (Official)", "en-DEFAULT", "en-DEFAULT"),
		new Language("French (Official)", "fr-DEFAULT", "fr-DEFAULT"),
		new Language("Italian (Official)", "it-DEFAULT", "it-DEFAULT"),
		new Language("German (Official)", "de-DEFAULT", "de-DEFAULT"),
		new Language("Spanish (Official)", "es-DEFAULT", "es-DEFAULT"),
		new Language("Russian (Official)", "ru-DEFAULT", "ru-DEFAULT")
	};

	private SystemLanguage currentLanguage;

	private bool _loadedUGC;

	private readonly bool _debug;

	private string _currentAssetPath;

	public IEnumerable<Language> GetLanguages()
	{
		return _languages;
	}

	public SystemLanguage GetCurrentLanguage()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return currentLanguage;
	}

	public LocalizationHandler(string path, bool debug = false)
	{
		_debug = debug;
		SetLanguagePath(path);
		GUserContentProvider.Get().LoadedUserContent += UserContentProvider_LoadedUserContent;
		GUserContentProvider.Get().RequestUserContent();
	}

	public void Dispose()
	{
		GUserContentProvider.Get().LoadedUserContent -= UserContentProvider_LoadedUserContent;
	}

	public void SetLanguagePath(string assetPath)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		string text = assetPath;
		if (string.IsNullOrWhiteSpace(text))
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if ((int)systemLanguage <= 15)
			{
				if ((int)systemLanguage != 14)
				{
					if ((int)systemLanguage != 15)
					{
						goto IL_0085;
					}
					text = "de-DEFAULT";
					currentLanguage = (SystemLanguage)15;
				}
				else
				{
					text = "fr-DEFAULT";
					currentLanguage = (SystemLanguage)14;
				}
			}
			else if ((int)systemLanguage != 21)
			{
				if ((int)systemLanguage != 30)
				{
					if ((int)systemLanguage != 34)
					{
						goto IL_0085;
					}
					text = "es-DEFAULT";
					currentLanguage = (SystemLanguage)34;
				}
				else
				{
					text = "ru-DEFAULT";
					currentLanguage = (SystemLanguage)30;
				}
			}
			else
			{
				text = "it-DEFAULT";
				currentLanguage = (SystemLanguage)21;
			}
		}
		goto IL_0093;
		IL_0093:
		if (_currentAssetPath != null && _currentAssetPath.Equals(text))
		{
			return;
		}
		_currentAssetPath = text;
		Dictionary<string, string> dictionary = LoadAssetEntries("en-DEFAULT");
		if (_currentAssetPath.Equals("en-DEFAULT"))
		{
			_entries = dictionary;
			return;
		}
		Dictionary<string, string> dictionary2 = LoadAssetEntries(_currentAssetPath);
		if (dictionary2.Count > 0)
		{
			MergeEntries(dictionary2, dictionary);
		}
		_entries = dictionary;
		return;
		IL_0085:
		text = "en-DEFAULT";
		currentLanguage = (SystemLanguage)10;
		goto IL_0093;
	}

	private Dictionary<string, string> LoadAssetEntries(string assetPath)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = LoadAsset(assetPath);
		if (array != null && array.Length != 0)
		{
			foreach (LocalizationEntry item in Parse(array, _debug))
			{
				dictionary.Add(item.Term, item.Text);
			}
		}
		return dictionary;
	}

	private void MergeEntries(Dictionary<string, string> entriesFrom, Dictionary<string, string> entriesTo)
	{
		StringBuilder builder = new StringBuilder();
		entriesTo.ToList().ForEach((KeyValuePair<string, string> entry) =>
		{
			string key = entry.Key;
			if (!entriesFrom.TryGetValue(key, out var value))
			{
				builder.Append("<color=red>" + key + "</color>\n");
			}
			else
			{
				entriesTo[key] = value;
			}
		});
		if (builder.Length > 0)
		{
			Debug.Log((object)(DEBUG_HEADER + "<color=red>Missing terms:</color>\n\n" + builder));
		}
	}

	private bool IsDefaultAssetPath(string path)
	{
		switch (path)
		{
		case "en-DEFAULT":
		case "fr-DEFAULT":
		case "it-DEFAULT":
		case "de-DEFAULT":
		case "es-DEFAULT":
		case "ru-DEFAULT":
			return true;
		default:
			return false;
		}
	}

	private string[] LoadAsset(string path)
	{
		if (!IsDefaultAssetPath(path))
		{
			return LoadExternalAsset(path);
		}
		return LoadDefaultAsset(path);
	}

	private string[] LoadDefaultAsset(string path)
	{
		TextAsset val = Resources.Load<TextAsset>("Localization/" + path);
		string[] result = val.text.Split('\n');
		Debug.Log((object)(DEBUG_HEADER + "Loaded default '" + path + "' asset."));
		Resources.UnloadAsset((Object)(object)val);
		return result;
	}

	private string[] LoadExternalAsset(string path)
	{
		if (!File.Exists(path))
		{
			Debug.LogError((object)(DEBUG_HEADER + "No asset found: " + path));
			return null;
		}
		try
		{
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			using StreamReader streamReader = new StreamReader(stream);
			List<string> list = new List<string>();
			string item;
			while ((item = streamReader.ReadLine()) != null)
			{
				list.Add(item);
			}
			Debug.Log((object)(DEBUG_HEADER + "Loaded '" + Path.GetFileNameWithoutExtension(path) + "' asset."));
			return list.ToArray();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return null;
		}
	}

	public static IEnumerable<LocalizationEntry> Parse(string[] lines, bool debug = false)
	{
		HashSet<LocalizationEntry> hashSet = new HashSet<LocalizationEntry>();
		for (int i = 1; i < lines.Length; i++)
		{
			string text = lines[i].Trim(new char[4] { ' ', '\t', '\r', '\n' });
			if (text.Length == 0 || (text.Length > 0 && text[0] == '#'))
			{
				continue;
			}
			string[] array = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))").Split(text);
			if (array.Length != 4)
			{
				Debug.LogError((object)(DEBUG_HEADER + $"Invalid formatting on line {i}: " + text));
				continue;
			}
			string text2 = array[0];
			string text3 = (debug ? "***" : array[1].Replace("\\n", "\n").Replace("\"", ""));
			if (text2.Length != 0)
			{
				LocalizationEntry localizationEntry = new LocalizationEntry();
				localizationEntry.Term = text2;
				localizationEntry.Text = text3;
				localizationEntry.ReplacementTerm = array[2];
				if (!hashSet.Add(localizationEntry))
				{
					Debug.LogError((object)(DEBUG_HEADER + "Duplicate term: " + text2));
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (LocalizationEntry item in hashSet)
		{
			if (!string.IsNullOrEmpty(item.ReplacementTerm))
			{
				LocalizationEntry localizationEntry2 = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(hashSet, (LocalizationEntry e, string replacementTerm) => e.Term.Equals(replacementTerm, StringComparison.InvariantCultureIgnoreCase), item.ReplacementTerm);
				if (localizationEntry2 != null)
				{
					item.Text = localizationEntry2.Text;
					stringBuilder.Append("Substituted term <color=cyan>" + item.Term + "</color> value for term " + localizationEntry2.Term + " value: \"" + localizationEntry2.Text + "\"\n");
				}
			}
		}
		return hashSet;
	}

	private void UserContentProvider_LoadedUserContent(LoadedContentArgs args)
	{
		if (_loadedUGC)
		{
			return;
		}
		Debug.Log((object)(DEBUG_HEADER + "Loading localization UGC.."));
		foreach (LoadedContent item in args.LoadedContent)
		{
			if (Beam.Utilities.ExtensionMethods.Any_NonAlloc(item.Tags, (string tag) => tag.Equals(UgcTags.LOCALIZATION)))
			{
				string[] files = Directory.GetFiles(item.Path, "*.csv");
				foreach (string path in files)
				{
					AddLanguageAsset(path);
				}
			}
		}
		_loadedUGC = true;
	}

	private void AddLanguageAsset(string path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string languageName = GetLanguageName(fileNameWithoutExtension);
		_languages.Add(new Language(languageName, fileNameWithoutExtension, path));
	}

	public IEnumerable<string> GetTerms()
	{
		return _entries.Keys;
	}

	public string Localize(string term, params string[] args)
	{
		if (string.IsNullOrWhiteSpace(term))
		{
			return string.Empty;
		}
		string text = term;
		if (_entries.ContainsKey(term))
		{
			text = _entries[term];
			if (HasArgs(args))
			{
				args = LocalizeArgs(args);
				string str = text;
				object[] args2 = args;
				text = string_safemethods.Format(str, args2);
			}
		}
		return text;
	}

	private string[] LocalizeArgs(params string[] args)
	{
		if (HasArgs(args))
		{
			for (int i = 0; i < args.Length; i++)
			{
				string terms = args[i];
				args[i] = this.LocalizeMany(terms, ' ');
			}
		}
		return args;
	}

	private bool HasArgs(params string[] args)
	{
		if (args != null)
		{
			return args.Length != 0;
		}
		return false;
	}

	private string GetLanguageName(string code)
	{
		try
		{
			return CultureInfo.GetCultureInfo(code).DisplayName;
		}
		catch (CultureNotFoundException)
		{
			return code;
		}
	}
}
