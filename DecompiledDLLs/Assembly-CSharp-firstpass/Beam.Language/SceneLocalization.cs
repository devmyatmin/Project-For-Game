using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Language;

public class SceneLocalization : MonoBehaviour
{
	private LocalizationHandler _localization;

	public static SceneLocalization Instance { get; private set; }

	public LocalizationHandler GetLocalization()
	{
		return _localization;
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Options.Loaded += Options_Loaded;
		Initialize();
	}

	private void OnDestroy()
	{
		_localization?.Dispose();
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		Options.Loaded -= Options_Loaded;
	}

	private void Initialize()
	{
		string langPath = Options.GeneralSettings.LangPath;
		_localization = new LocalizationHandler(langPath);
	}

	private void Options_Loaded(JObject data)
	{
		string langPath = Options.GeneralSettings.LangPath;
		_localization.SetLanguagePath(langPath);
	}
}
