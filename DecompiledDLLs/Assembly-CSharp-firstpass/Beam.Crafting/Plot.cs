using System;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Beam.Crafting;

[RequireComponent(typeof(Farming_PLOT))]
public class Plot : MultiplayerBehaviour, IMultiplayerState<IPlotState>
{
	public class ReplicateKillCrop : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab Prefab;

		public override void OnPeer()
		{
			((Component)Prefab).GetComponent<Plot>().KillCrop();
		}
	}

	public class ReplicatePoll : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab Prefab;

		public override void OnPeer()
		{
			((Component)Prefab).GetComponent<Plot>().PollPlant_InvokeRepeating();
		}
	}

	private const float DEPLETE_INTERVAL_HOURS = 24f;

	[SerializeField]
	private int _maxWater = 4;

	[SerializeField]
	private Transform _plantPosition;

	private IPlantFactory _plantFactory;

	private Plant _plant;

	private bool _growing;

	private bool _isRaining;

	private int _water = 1;

	public bool Growing => _growing;

	public IPlotState MultiplayerState => base.GetMultiplayerState<IPlotState>();

	public event Action<PlantType> CropCreated;

	public bool HasPlants()
	{
		return (Object)(object)_plant != (Object)null;
	}

	public float GetNormalizedWater()
	{
		return (float)_water / (float)_maxWater;
	}

	protected virtual void OnCropCreated(PlantType type)
	{
		CropCreated?.Invoke(type);
	}

	private void Awake()
	{
		FarmingManager plantFactory = Object.FindObjectOfType<FarmingManager>();
		_plantFactory = plantFactory;
	}

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode) && (Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted += AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped += AtmosphereStorm_WeatherStopped;
			if ((Object)(object)Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent != (Object)null)
			{
				AtmosphereStorm_WeatherStarted(Singleton<AtmosphereStorm>.Instance.CurrentWeatherEvent);
			}
		}
	}

	private void OnDestroy()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CropCreated = null;
		if (!MPExtensions.IsClient(Game.Mode) && (Object)(object)Singleton<AtmosphereStorm>.Instance != (Object)null)
		{
			Singleton<AtmosphereStorm>.Instance.WeatherStarted -= AtmosphereStorm_WeatherStarted;
			Singleton<AtmosphereStorm>.Instance.WeatherStopped -= AtmosphereStorm_WeatherStopped;
		}
	}

	private void AtmosphereStorm_WeatherStarted(WeatherEvent weather)
	{
		if (Mathf.Approximately(weather.Humidity, 100f))
		{
			_isRaining = true;
			int num = 1;
			((MonoBehaviour)this).InvokeRepeating("PollRain_InvokeRepeating", GameTime.OneGameHour, (float)num * GameTime.OneGameHour);
		}
	}

	private void AtmosphereStorm_WeatherStopped()
	{
		_isRaining = false;
		((MonoBehaviour)this).CancelInvoke("PollRain_InvokeRepeating");
	}

	public void Place()
	{
	}

	public bool Plant(Plantable plantable)
	{
		if (_growing)
		{
			return false;
		}
		ClearCrop();
		if (_water < 1)
		{
			_water = 1;
		}
		CreateCrop(plantable.PlantType);
		Initialize();
		return true;
	}

	private void CreateCrop(PlantType plantType)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		_growing = true;
		_plant = _plantFactory.CreatePlant(plantType);
		((Component)_plant).transform.parent = ((Component)this).gameObject.transform;
		((Component)_plant).transform.position = _plantPosition.position;
		OnCropCreated(plantType);
	}

	private void Initialize()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			((MonoBehaviour)this).CancelInvoke("PollWater_InvokeRepeating");
			((MonoBehaviour)this).CancelInvoke("PollPlant_InvokeRepeating");
			((MonoBehaviour)this).InvokeRepeating("PollWater_InvokeRepeating", 24f * GameTime.OneGameHour, 24f * GameTime.OneGameHour);
			((MonoBehaviour)this).InvokeRepeating("PollPlant_InvokeRepeating", 3f, GameTime.OneGameHour);
		}
	}

	private void PollRain_InvokeRepeating()
	{
		if (!LevelLoader.IsLoading(((Component)this).gameObject) && !LevelLoader.IsServerJoinInProgress && _isRaining)
		{
			_water++;
			_water = Mathf.Clamp(_water, 0, _maxWater);
		}
	}

	private void PollPlant_InvokeRepeating()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsLoading() && !LevelLoader.IsServerJoinInProgress && _growing && !((Object)(object)_plant == (Object)null))
		{
			if (MPExtensions.IsServer(Game.Mode))
			{
				ReplicatePoll replicatePoll = new ReplicatePoll();
				replicatePoll.Prefab = ((Component)this).GetComponent<SaveablePrefab>();
				replicatePoll.Post();
			}
			_plant.Poll();
		}
	}

	private void PollWater_InvokeRepeating()
	{
		if (!LevelLoader.IsLoading() && !LevelLoader.IsServerJoinInProgress && !_isRaining)
		{
			_water--;
			_water = Mathf.Clamp(_water, 0, _maxWater);
			if (_water <= 0)
			{
				ReplicatedKillCrop();
			}
		}
	}

	public bool CanWaterCrop()
	{
		if (_growing)
		{
			return _water < _maxWater;
		}
		return false;
	}

	public bool WaterCrop(int amount)
	{
		if (_growing && _water < _maxWater)
		{
			_water += amount;
			_water = Mathf.Clamp(_water, 0, _maxWater);
			return true;
		}
		return false;
	}

	public bool ClearCrop()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_plant == (Object)null)
		{
			return false;
		}
		_plant.ClearFruit();
		_plantFactory.DestroyPlant(_plant);
		_plant = null;
		_growing = false;
		if (MPExtensions.IsMaster(Game.Mode))
		{
			((MonoBehaviour)this).CancelInvoke("PollWater_InvokeRepeating");
		}
		return true;
	}

	private void KillCrop()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (_growing)
		{
			_plant.Kill();
			_water = 0;
			_growing = false;
			if (MPExtensions.IsMaster(Game.Mode))
			{
				((MonoBehaviour)this).CancelInvoke("PollWater_InvokeRepeating");
			}
		}
	}

	protected void FixedUpdate()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			if (!((EntityBehaviour)this).entity.IsOwner)
			{
				_water = MultiplayerState.Water;
				_growing = MultiplayerState.Growing;
			}
			else
			{
				MultiplayerState.Water = _water;
				MultiplayerState.Growing = _growing;
			}
		}
	}

	private void ReplicatedKillCrop()
	{
		SaveablePrefab component = ((Component)this).GetComponent<SaveablePrefab>();
		if ((Object)(object)component != (Object)null && component.IsOwner)
		{
			ReplicateKillCrop replicateKillCrop = new ReplicateKillCrop();
			replicateKillCrop.Prefab = ((Component)this).GetComponent<SaveablePrefab>();
			replicateKillCrop.Post();
		}
		KillCrop();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Water", _water);
		PlantType plantType = (((Object)(object)_plant != (Object)null) ? _plant.PlantType : PlantType.NONE);
		jObject.AddField("PlantType", JSerializer.Serialize((object)plantType));
		if ((Object)(object)_plant == (Object)null)
		{
			return jObject;
		}
		JObject jObject2 = new JObject();
		jObject2.Add(SavingUtilities.Save(_plant));
		jObject.AddField("Plants", jObject2);
		jObject.AddField("LastPollTime", JSerializer.Serialize((object)GameTime.Now));
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Water");
		if ((bool)field)
		{
			_water = field.GetValue<int>();
		}
		PlantType plantType = JSerializer.Deserialize<PlantType>(data.GetField("PlantType"));
		if (plantType == PlantType.NONE)
		{
			return;
		}
		CreateCrop(plantType);
		if (_water <= 0)
		{
			ReplicatedKillCrop();
			return;
		}
		JObject field2 = data.GetField("Plants");
		JObject data2 = ((field2 != null) ? Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(field2.Children) : null) ?? data.GetField("[0]Plant");
		if (SavingUtilities.Load(_plant, data2))
		{
			JObject field3 = data.GetField("LastPollTime");
			if (field3 != null && !field3.IsNull())
			{
				int num = Mathf.RoundToInt((float)(int)GameTime.GetHoursSince(JSerializer.Deserialize<DateTime>(field3)) / 24f);
				_water -= num;
				if (_water <= 0)
				{
					ReplicatedKillCrop();
					return;
				}
			}
			Initialize();
		}
		else
		{
			ClearCrop();
		}
	}
}
