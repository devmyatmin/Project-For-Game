using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Language;

public class ScenePlatformLocalization : MonoBehaviour
{
	[SerializeField]
	private PlatformLocalizationHandler _platformLocalization;

	private List<Localizer> _localizers = new List<Localizer>();

	public static ScenePlatformLocalization Instance { get; private set; }

	public PlatformLocalizationHandler GetPlatformLocalization()
	{
		return _platformLocalization;
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
		ControllerGlyphs controllerGlyphs = Resources.Load<ControllerGlyphs>("Localization/ControllerGlyphs");
		if ((Object)(object)controllerGlyphs == (Object)null)
		{
			Debug.LogError((object)"ScenePlatformLocalization:: Failed to load glyphs asset.");
			return;
		}
		_platformLocalization = new PlatformLocalizationHandler(controllerGlyphs);
		EventManager.AddListener<RewiredControllerChangedEvent>(EventManager_RewiredControllerChanged);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<RewiredControllerChangedEvent>(EventManager_RewiredControllerChanged);
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	public void AddLocalizer(Localizer localizer)
	{
		if (!_localizers.Contains(localizer))
		{
			_localizers.Add(localizer);
		}
	}

	public void RemoveLocalizer(Localizer localizer)
	{
		_localizers.Remove(localizer);
	}

	private void EventManager_RewiredControllerChanged(RewiredControllerChangedEvent e)
	{
		Refresh();
	}

	private void Refresh()
	{
		Debug.Log((object)"ScenePlatformLocalization:: Refreshing widgets...");
		for (int i = 0; i < _localizers.Count; i++)
		{
			Localizer localizer = _localizers[i];
			if (localizer.RewiredAction != -1)
			{
				localizer.Refresh();
			}
		}
	}
}
