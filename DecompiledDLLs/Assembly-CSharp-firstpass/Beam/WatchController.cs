using Beam.AccountServices;
using Beam.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class WatchController : MonoBehaviour, INeedPlayerComponent, IIdleUntilInitialized
{
	[SerializeField]
	private WatchViewAdapterBase _2dView;

	[SerializeField]
	private WatchScreenViewAdapterBase _3dView;

	private bool _delayed;

	private float _timer;

	private float _time;

	[SerializeField]
	private UnityEvent _onShowView;

	[SerializeField]
	private UnityEvent _onHideView;

	[SerializeField]
	private UnityEvent _onWatchModeChanged;

	[SerializeField]
	private UnityEvent _onLowHealthNotify;

	[SerializeField]
	private UnityEvent _onCriticalHealthNotify;

	[SerializeField]
	private UnityEvent _onDelayNotifications;

	[SerializeField]
	private UnityEvent _onPlayerEffectApplied;

	private IPlayer _player;

	private bool _initialize;

	public IPlayer Player => _player;

	public bool IsInitialized => _initialize;

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Character.ShowLeftArm += PlayerCharacter_ShowLeftArm;
		_player.Character.HideLeftArm += PlayerCharacter_HideLeftArm;
		_player.Statistics.PlayerEffectApplied += PlayerStatistics_PlayerEffectApplied;
	}

	public void Initialize()
	{
		if (!_initialize)
		{
			_initialize = true;
		}
	}

	public void Initialize(WatchViewAdapterBase watchView, WatchScreenViewAdapterBase watchScreenView)
	{
		_2dView = watchView;
		_3dView = watchScreenView;
		if ((Object)(object)_2dView == (Object)null)
		{
			_2dView = NullWatchViewAdapter.Instance;
			Debug.LogError((object)"Watch:: No 2D view found!");
		}
		if ((Object)(object)_3dView == (Object)null)
		{
			_3dView = NullWatchScreenViewAdapterBase.Instance;
			Debug.LogError((object)"Watch:: No 3D view found!");
		}
		_2dView.SetController(this);
		_2dView.SetWatchMode(0);
		_2dView.ShowView += WatchViewAdapterBase_ShowView;
		_2dView.HideView += WatchViewAdapterBase_HideView;
		_3dView.SetWatchTexture((Texture)(object)_2dView.Camera.targetTexture);
		_3dView.SetWatchMesh(_player.Gender);
		_3dView.SetBacklight(enabled: false);
		Singleton<Atmosphere>.Instance.SunsetEvent += Atmosphere_SunsetEvent;
		Atmosphere_SunsetEvent();
	}

	private void OnDestroy()
	{
		if ((Object)(object)_2dView != (Object)null)
		{
			_2dView.ShowView -= WatchViewAdapterBase_ShowView;
			_2dView.HideView -= WatchViewAdapterBase_HideView;
		}
		if (_player != null)
		{
			if ((Object)(object)_player.Character != (Object)null)
			{
				_player.Character.ShowLeftArm -= PlayerCharacter_ShowLeftArm;
				_player.Character.HideLeftArm -= PlayerCharacter_HideLeftArm;
			}
			if ((Object)(object)_player.Statistics != (Object)null)
			{
				_player.Statistics.PlayerEffectApplied -= PlayerStatistics_PlayerEffectApplied;
			}
		}
		if ((Object)(object)Singleton<Atmosphere>.Instance != (Object)null)
		{
			Singleton<Atmosphere>.Instance.SunsetEvent -= Atmosphere_SunsetEvent;
		}
	}

	private void Atmosphere_SunsetEvent()
	{
		if (ViewsLoaded())
		{
			bool backlight = !Singleton<Atmosphere>.Instance.DayTime;
			_3dView.SetBacklight(backlight);
		}
	}

	private void PlayerCharacter_ShowLeftArm()
	{
		Show();
	}

	private void PlayerCharacter_HideLeftArm()
	{
		Hide();
	}

	private void PlayerStatistics_PlayerEffectApplied(PlayerEffect effect)
	{
		UnityEvent onPlayerEffectApplied = _onPlayerEffectApplied;
		if (onPlayerEffectApplied != null)
		{
			onPlayerEffectApplied.Invoke();
		}
	}

	private void WatchViewAdapterBase_ShowView()
	{
		UnityEvent onShowView = _onShowView;
		if (onShowView != null)
		{
			onShowView.Invoke();
		}
	}

	private void WatchViewAdapterBase_HideView()
	{
		UnityEvent onHideView = _onHideView;
		if (onHideView != null)
		{
			onHideView.Invoke();
		}
	}

	private void Update()
	{
		if (_initialize)
		{
			CheckWarning();
		}
	}

	public void Show()
	{
		if (!ViewsLoaded())
		{
			return;
		}
		if (!_2dView.Visible)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 3);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
			_player.PlayerUI.Suspend(suspend: true);
			_2dView.SetTemperatureUnits(Options.GeneralSettings.Temperature);
			_2dView.Show();
			_3dView.Show();
			_onShowView.Invoke();
		}
		_delayed = true;
		Sunburn playerEffect = _player.Statistics.GetPlayerEffect<Sunburn>();
		string uVIndex = ((playerEffect.Protected || !playerEffect.IsDayTimeUV()) ? "PLAYER_WATCH_UV_INDEX_LABEL_NONE" : UVIndexUtility.GetFormattedUVIndex(Singleton<AtmosphereTemperature>.Instance.GetUVIndex()));
		_2dView.SetUVIndex(uVIndex);
		_2dView.SetClock(GameTime.GetFormattedTime());
		_2dView.SetDate(Singleton<GameCalendar>.Instance.GetDate());
		_2dView.SetDays(Singleton<GameCalendar>.Instance.DaysElapsed.ToString());
		_2dView.SetHealth(_player.Statistics.HealthPercent * 7f / 7f);
		_2dView.SetHunger(_player.Statistics.CaloriesPercent * 7f / 7f);
		_2dView.SetThirst(_player.Statistics.FluidsPercent * 7f / 7f);
		_2dView.SetSleep((1f - playerEffect.SunExposureTime) * 7f / 7f);
		_2dView.SetStatistics(GAccountService.Get().LocalUser.UserStats.SessionStats);
		_2dView.SetHunting(_player.PlayerSkills.HuntingDisplayedPercent * 7f / 7f);
		_2dView.SetCooking(_player.PlayerSkills.CookingDisplayedPercent * 7f / 7f);
		_2dView.SetHarvesting(_player.PlayerSkills.HarvestingDisplayedPercent * 7f / 7f);
		_2dView.SetPhysical(_player.PlayerSkills.PhysicalDisplayedPercent * 7f / 7f);
		_2dView.SetCraftsmanship(_player.PlayerSkills.CraftsmanshipDisplayedPercent * 7f / 7f);
		if (_player.Input.GetButtonDown(34))
		{
			_2dView.CycleWatchModeForward();
			UnityEvent onWatchModeChanged = _onWatchModeChanged;
			if (onWatchModeChanged != null)
			{
				onWatchModeChanged.Invoke();
			}
		}
		else if (_player.Input.GetButtonDown(40))
		{
			_2dView.CycleWatchModeBackward();
			UnityEvent onWatchModeChanged2 = _onWatchModeChanged;
			if (onWatchModeChanged2 != null)
			{
				onWatchModeChanged2.Invoke();
			}
		}
	}

	public void Hide()
	{
		if (ViewsLoaded() && _2dView.Visible)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 3);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
			_player.PlayerUI.Suspend(suspend: false);
			_2dView.Hide();
			_3dView.Hide();
			_onHideView.Invoke();
		}
	}

	private void CheckWarning()
	{
		if (_delayed)
		{
			UnityEvent onDelayNotifications = _onDelayNotifications;
			if (onDelayNotifications != null)
			{
				onDelayNotifications.Invoke();
			}
			_timer = 0f;
			_time = 90f;
			_delayed = false;
			return;
		}
		_timer += Time.deltaTime;
		if (_timer < _time)
		{
			return;
		}
		if (_player.Statistics.HealthPercent < 0.04f)
		{
			UnityEvent onCriticalHealthNotify = _onCriticalHealthNotify;
			if (onCriticalHealthNotify != null)
			{
				onCriticalHealthNotify.Invoke();
			}
			_timer = 0f;
			_time = 1f;
		}
		else if (_player.Statistics.HealthPercent < 0.1f)
		{
			UnityEvent onLowHealthNotify = _onLowHealthNotify;
			if (onLowHealthNotify != null)
			{
				onLowHealthNotify.Invoke();
			}
			_timer = 0f;
			_time = 10f;
		}
		else if (_player.Statistics.HealthPercent < 0.25f)
		{
			UnityEvent onLowHealthNotify2 = _onLowHealthNotify;
			if (onLowHealthNotify2 != null)
			{
				onLowHealthNotify2.Invoke();
			}
			_timer = 0f;
			_time = 30f;
		}
		else
		{
			_timer = 0f;
			_time = 30f;
		}
	}

	private bool ViewsLoaded()
	{
		if ((Object)(object)_2dView != (Object)null)
		{
			return (Object)(object)_3dView != (Object)null;
		}
		return false;
	}
}
