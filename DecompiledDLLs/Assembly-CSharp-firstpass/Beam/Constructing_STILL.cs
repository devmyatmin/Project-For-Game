using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Constructing_STILL : Constructing, IInteractable, IBase, IHasCraftingType, IHaveAttribute
{
	public class ReplicateBoil : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab WaterStill;

		[Replicate]
		public int Units;

		public override void OnPeer()
		{
			SaveablePrefab waterStill = WaterStill;
			if (waterStill != null)
			{
				((Component)waterStill).GetComponent<Constructing_STILL>().Boil(Units);
			}
		}
	}

	private const float DECREMENT_INTERVAL_HOURS = 1f;

	private const int DECREMENT_UNIT = 1;

	private const int MAX_FIBRE = 4;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Constructing_STILL_COLLECTOR _collector;

	private CoroutineHandle _collectingSequence;

	private float _collectingTimer;

	private int _fibre = 1;

	private bool _isRaining;

	[SerializeField]
	private ParticleSystem _splashParticle;

	[SerializeField]
	private GameObject[] _fibreProxies;

	[HideInInspector]
	public UnityEvent StartedCollecting;

	[HideInInspector]
	public UnityEvent StoppedCollecting;

	[HideInInspector]
	public FloatUnityEvent Collected;

	[HideInInspector]
	public UnityEvent Refilled;

	public IconType Type => IconType.Fuel;

	public bool CanInteract { get; set; } = true;

	public Constructing_STILL_COLLECTOR Collector => _collector;

	public float GetValue()
	{
		return (float)_fibre / 4f;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_fibre == 4 || BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (IsRefillable(obj))
		{
			return "ITEM_INTERACTION_DESCRIPTION_ADD_FIBRE";
		}
		return string.Empty;
	}

	public override void Place()
	{
		base.Place();
		if (_fibre > 0)
		{
			StartCollecting();
		}
		Reposition_Fibres();
	}

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted += AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped += AtmosphereStorm_WeatherStopped;
			if ((Object)(object)Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent != (Object)null)
			{
				AtmosphereStorm_WeatherStarted(Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent);
			}
		}
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
		}
	}

	private void GameTime_HourChanged(float hours)
	{
		Still_Collected(hours);
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_collectingSequence);
		((UnityEventBase)StartedCollecting).RemoveAllListeners();
		((UnityEventBase)StoppedCollecting).RemoveAllListeners();
		((UnityEventBase)Collected).RemoveAllListeners();
		((UnityEventBase)Refilled).RemoveAllListeners();
		StartedCollecting = null;
		StoppedCollecting = null;
		Collected = null;
		Refilled = null;
		if ((Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted -= AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped -= AtmosphereStorm_WeatherStopped;
		}
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
	}

	private void AtmosphereStorm_WeatherStarted(WeatherEvent weather)
	{
		if (Mathf.Approximately(weather.Humidity, 100f))
		{
			_isRaining = true;
			StartCollecting();
		}
	}

	private void AtmosphereStorm_WeatherStopped()
	{
		_isRaining = false;
	}

	private void StartCollecting()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((_fibre != 0 || _isRaining) && !_collectingSequence.IsRunning)
		{
			if ((Object)(object)_splashParticle != (Object)null)
			{
				_splashParticle.Play();
			}
			_collectingSequence = Timing.RunCoroutine(Boil_Sequence());
			UnityEvent startedCollecting = StartedCollecting;
			if (startedCollecting != null)
			{
				startedCollecting.Invoke();
			}
		}
	}

	private void StopCollecting()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_collectingSequence.IsRunning)
		{
			if ((Object)(object)_splashParticle != (Object)null)
			{
				_splashParticle.Stop();
			}
			Timing.KillCoroutines(_collectingSequence);
			UnityEvent stoppedCollecting = StoppedCollecting;
			if (stoppedCollecting != null)
			{
				stoppedCollecting.Invoke();
			}
			_collectingTimer = 0f;
		}
	}

	private IEnumerator<float> Boil_Sequence()
	{
		while (true)
		{
			_collectingTimer += GameTime.DeltaTimeHour;
			if (_collectingTimer > 1f)
			{
				_collectingTimer = 0f;
				ReplicatedBoil(1);
			}
			yield return 0f;
		}
	}

	private void Boil(int elapsedIntervals)
	{
		if (_collector.Water >= _collector.MaxWater)
		{
			return;
		}
		if (_isRaining)
		{
			((UnityEvent<float>)Collected)?.Invoke(1f);
			return;
		}
		int fibre = _fibre;
		int num = _collector.MaxWater - _collector.Water;
		int num2 = Mathf.Min(new int[3] { fibre, num, elapsedIntervals });
		if (num2 > 0)
		{
			_fibre -= num2;
			_fibre = Mathf.Clamp(_fibre, 0, 4);
			if (_fibre == 0)
			{
				StopCollecting();
			}
			((UnityEvent<float>)Collected)?.Invoke((float)num2);
		}
		Reposition_Fibres();
	}

	public void ForceRefill(int units)
	{
		((UnityEvent<float>)Collected).Invoke((float)units);
	}

	private void Refill()
	{
		if (_fibre != 4)
		{
			_fibre++;
			_fibre = Mathf.Clamp(_fibre, 0, 4);
			UnityEvent refilled = Refilled;
			if (refilled != null)
			{
				refilled.Invoke();
			}
			StartCollecting();
			Reposition_Fibres();
		}
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (_fibre == 4)
		{
			return false;
		}
		if (IsRefillable(obj))
		{
			player.Holder.ReleaseAndDestroyCurrent();
			Refill();
			return true;
		}
		return Interact(player);
	}

	private bool IsRefillable(IBase obj)
	{
		if (obj.CraftingType.InteractiveType != InteractiveType.CRAFTING_LEAVES)
		{
			return obj.CraftingType.InteractiveType == InteractiveType.CRAFTING_PALM_FROND;
		}
		return true;
	}

	private void Reposition_Fibres()
	{
		for (int i = 0; i < 4; i++)
		{
			bool active = false;
			if (i < _fibre)
			{
				active = true;
			}
			_fibreProxies[i].SetActive(active);
		}
	}

	private void Still_Collected(float hours)
	{
		float num = hours / 1f;
		int num2 = Mathf.FloorToInt(num);
		ReplicatedBoil(num2);
		if (_fibre > 0)
		{
			float num3 = num - (float)num2;
			_collectingTimer = num3 * 1f;
			StartCollecting();
		}
	}

	public void ReplicatedBoil(int units)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateBoil replicateBoil = new ReplicateBoil();
			replicateBoil.WaterStill = ((Component)this).GetComponent<SaveablePrefab>();
			replicateBoil.Units = units;
			replicateBoil.Post();
		}
		Boil(units);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Fibre", _fibre);
		jObject.AddField("TimeLeft", JSerializer.Serialize((object)GameTime.Now));
		JObject jObject2 = SavingUtilities.Save(_collector);
		if (jObject2 != null)
		{
			jObject.AddField("Collector", jObject2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Fibre");
		if (field != null)
		{
			_fibre = field.GetValue<int>();
		}
		JObject field2 = data.GetField("Collector");
		if (field2 != null)
		{
			SavingUtilities.Load(_collector, field2);
		}
		LevelLoader.CurrentLoader.PostLoading.Add(() =>
		{
			JObject field3 = data.GetField("TimeLeft");
			if (field3 != null)
			{
				float hoursSince = GameTime.GetHoursSince(JSerializer.Deserialize<DateTime>(field3));
				Still_Collected(hoursSince);
			}
		});
	}
}
