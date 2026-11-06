using System;
using System.IO;
using Beam;
using Beam.Serialization;
using Beam.Serialization.Json;
using UnityEngine;

public static class Options
{
	private sealed class OptionsFinalizer
	{
		~OptionsFinalizer()
		{
			Unload();
		}
	}

	private static readonly OptionsFinalizer _finalizer = new OptionsFinalizer();

	private static GeneralSettings _generalSettings = new GeneralSettings();

	private static GraphicsSettings _graphicsSettings = GraphicsPreset.MEDIUM;

	private static AudioSettings _audioSettings = new AudioSettings();

	private static InputSettings _inputSettings = new InputSettings();

	private static GameSettings _customSettings = new GameSettings();

	private static Version _version = new Version();

	public static GeneralSettings GeneralSettings => _generalSettings;

	public static GraphicsSettings GraphicsSettings
	{
		get
		{
			return _graphicsSettings;
		}
		set
		{
			_graphicsSettings = value;
		}
	}

	public static AudioSettings AudioSettings => _audioSettings;

	public static InputSettings InputSettings
	{
		get
		{
			return _inputSettings;
		}
		set
		{
			_inputSettings = value;
		}
	}

	public static GameSettings CustomSettings
	{
		get
		{
			return _customSettings;
		}
		set
		{
			_customSettings = value;
		}
	}

	public static Version Version => _version;

	public static event Action PreSave;

	public static event Action Saved;

	public static event Action<JObject> Loaded;

	private static void Unload()
	{
		PreSave = null;
		Saved = null;
		Loaded = null;
	}

	public static void Save()
	{
		PreSave?.Invoke();
		JObject jObject = new JObject();
		jObject.AddField("Version", ExtensionMethods.ToPaddedString(BuildInformation.BuildVersion));
		jObject.AddField("General", SavingUtilities.Save(_generalSettings));
		jObject.AddField("Graphics", SavingUtilities.Save(_graphicsSettings));
		jObject.AddField("Input", SavingUtilities.Save(_inputSettings));
		jObject.AddField("Audio", SavingUtilities.Save(_audioSettings));
		bool flag = false;
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(FilePath.OPTIONS_FILE));
			using (FileStream stream = new FileStream(FilePath.OPTIONS_FILE, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				using StreamWriter streamWriter = new StreamWriter(stream);
				streamWriter.Write(jObject.ToString());
			}
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
			Debug.LogError((object)("Options::Save:: Could not save options to file.\n" + ex));
		}
		if (flag)
		{
			Saved?.Invoke();
		}
		else if (File.Exists(FilePath.OPTIONS_FILE))
		{
			File.Delete(FilePath.OPTIONS_FILE);
		}
	}

	public static void Load()
	{
		if (!File.Exists(FilePath.OPTIONS_FILE))
		{
			Debug.LogWarning((object)"Options::Load:: Could not load options data. No file was found. \nOptions settings have been reverted to defaults.");
			return;
		}
		JObject jObject;
		try
		{
			using StreamReader streamReader = new StreamReader(FilePath.OPTIONS_FILE);
			jObject = new JObject(streamReader.ReadToEnd());
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Options::Load:: Could not load options data. \nOptions settings have been reverted to defaults. \n\n" + ex));
			return;
		}
		if (jObject == null || jObject.IsNull())
		{
			Debug.LogError((object)"Options::Load:: Could not load options data. No data was found. \nOptions settings have been reverted to defaults.");
			return;
		}
		JObject field = jObject.GetField("Version");
		if (field != null)
		{
			try
			{
				_version = new Version(field.GetValue<string>());
			}
			catch (Exception ex2)
			{
				Debug.LogError((object)("Options::Load:: Could not load version data. No data was found. \n\n" + ex2));
			}
		}
		JObject field2 = jObject.GetField("General");
		if (field2 != null)
		{
			_generalSettings = new GeneralSettings();
			if (!SavingUtilities.Load(_generalSettings, field2))
			{
				Debug.LogError((object)"Options::Load:: Could not load [general] data. \nGeneral settings have been reverted to defaults.");
			}
		}
		else
		{
			Debug.LogError((object)"Options::Load:: No [general] data was found. \nGeneral settings have been reverted to defaults.");
		}
		JObject field3 = jObject.GetField("Graphics");
		if (field3 != null)
		{
			_graphicsSettings = new GraphicsSettings();
			if (!SavingUtilities.Load(_graphicsSettings, field3))
			{
				Debug.LogError((object)"Options::Load:: Could not load [graphics] data. \nGeneral settings have been reverted to defaults.");
			}
		}
		else
		{
			Debug.LogError((object)"Options::Load:: No [graphics] data was found. \nGraphics settings have been reverted to defaults.");
		}
		JObject field4 = jObject.GetField("Audio");
		if (field4 != null)
		{
			_audioSettings = new AudioSettings();
			if (!SavingUtilities.Load(_audioSettings, field4))
			{
				Debug.LogError((object)"Options::Load:: Could not load [audio] data. \nAudio settings have been reverted to defaults.");
			}
		}
		else
		{
			Debug.LogError((object)"Options::Load:: No [audio] data was found. \nAudio settings have been reverted to defaults.");
		}
		JObject field5 = jObject.GetField("Input");
		if (field5 != null)
		{
			_inputSettings = new InputSettings();
			if (!SavingUtilities.Load(_inputSettings, field5))
			{
				Debug.LogError((object)"Options::Load:: Could not load [input] data. \nInput settings have been reverted to defaults.");
			}
		}
		else
		{
			Debug.LogError((object)"Options::Load:: No [input] data was found. \nInput settings have been reverted to defaults.");
		}
		Loaded?.Invoke(jObject);
	}

	public static Version GetFileVersion()
	{
		if (!File.Exists(FilePath.OPTIONS_FILE))
		{
			Debug.Log((object)"Options::GetFileVersion:: Options file does not exist.");
			return null;
		}
		try
		{
			using FileStream stream = new FileStream(FilePath.OPTIONS_FILE, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader = new StreamReader(stream);
			streamReader.Read();
			bool flag = streamReader.Peek() == 13;
			if (flag)
			{
				streamReader.Read();
			}
			bool flag2 = streamReader.Peek() == 10;
			if (flag2)
			{
				streamReader.Read();
			}
			string version;
			if (flag | flag2)
			{
				version = streamReader.ReadLine().Trim().Substring(12, 7);
			}
			else
			{
				char[] array = new char[20];
				streamReader.Read(array, 0, array.Length);
				version = new string(array).Substring(11, 7);
			}
			return new Version(version);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Options::GetFileVersion:: Error loading version data from options file:\n\n" + ex.Message));
			return null;
		}
	}
}
