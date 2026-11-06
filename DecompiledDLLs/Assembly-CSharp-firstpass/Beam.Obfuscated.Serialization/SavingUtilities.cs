using System;
using System.Text;
using Beam.Serialization;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam.Obfuscated.Serialization;

public static class SavingUtilities
{
	public static void SaveToPrefs(string key, JObject data)
	{
		if (data == null)
		{
			Debug.LogError((object)"SavingUtilities::SaveToPrefs:: Data was null.");
			return;
		}
		string text = Convert.ToBase64String(Encoding.UTF8.GetBytes(data.ToString()));
		PlayerPrefs.SetString(key, text);
	}

	public static void SaveToPrefs(string key, ISaveable saveable)
	{
		JObject data = Beam.Serialization.SavingUtilities.Save(saveable);
		SaveToPrefs(key, data);
	}

	public static void DeleteFromPrefs(string key)
	{
		if (PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.DeleteKey(key);
		}
	}

	public static JObject LoadFromPrefs(string key)
	{
		string text = PlayerPrefs.GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError((object)("SavingUtilities::LoadFromPrefs: Prefs data null or empty for key [" + key + "]."));
			return null;
		}
		JObject jObject;
		try
		{
			byte[] bytes = Convert.FromBase64String(text);
			jObject = new JObject(Encoding.UTF8.GetString(bytes));
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SavingUtilities::LoadFromPrefs: Failed to load from Prefs:\n\n" + ex));
			return null;
		}
		if (jObject.IsNull())
		{
			Debug.LogError((object)("SavingUtilities::LoadFromPrefs: Data was null or empty for key [" + key + "]."));
			return null;
		}
		return jObject;
	}

	public static bool LoadFromPrefs<T>(string key, T instance) where T : ISaveable, new()
	{
		JObject jObject = LoadFromPrefs(key);
		if (jObject == null)
		{
			return false;
		}
		try
		{
			instance.Load(jObject);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SavingUtilities::LoadFromPrefs: Error loading object [" + typeof(T).Name + "]:\n\n" + ex));
			return false;
		}
	}

	public static T LoadFromPrefs<T>(string key) where T : ISaveable, new()
	{
		if (LoadFromPrefs(key) == null)
		{
			return default;
		}
		T val = new T();
		LoadFromPrefs(key, val);
		return val;
	}
}
