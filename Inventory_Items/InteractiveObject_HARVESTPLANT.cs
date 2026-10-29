using System;
using Beam;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_HARVESTPLANT : InteractiveObject
{
	[Serializable]
	public class HarvestResources
	{
		public GameObject HarvestPrefab;

		public Transform HarvestPosition;

		public int SpawnAmount = 1;
	}

	private bool _canRespawn;

	[SerializeField]
	private float HoursToRespawn = 48f;

	[SerializeField]
	private HarvestResources[] _resources;

	[SerializeField]
	private GameObject _fullModel;

	[SerializeField]
	private GameObject _harvestedModel;

	private DateTime _lastHarvestedTime;

	[HideInInspector]
	public BaseActionUnityEvent Harvested;

	[SerializeField]
	private InteractiveObject_FOOD _seedPrefab;

	[SerializeField]
	private Transform _seedPosition;

	private void CreateSeed()
	{
		if (!(_seedPrefab == null) && !Game.Mode.IsClient())
		{
			InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.Instantiate<InteractiveObject_FOOD>(_seedPrefab.PrefabId, MiniGuid.New(), base.transform.parent);
			interactiveObject_FOOD.transform.position = _seedPosition.position;
			interactiveObject_FOOD.transform.rotation = _seedPosition.rotation;
			interactiveObject_FOOD.Attach(new StaticAttachingAnchor(_seedPosition));
		}
	}

	protected override void Start()
	{
		base.Start();
		SetDifficultyParams();
		_canRespawn = HoursToRespawn > 0f;
		if (!LevelLoader.IsLoading())
		{
			CreateSeed();
			_lastHarvestedTime = GameTime.Now.RemoveHours(HoursToRespawn * 2f);
			Initialize();
		}
	}

	private void SetDifficultyParams()
	{
		if (PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard)
		{
			HoursToRespawn *= 1.5f;
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		_lastHarvestedTime = GameTime.Now.RemoveHours(HoursToRespawn * 2f);
		Initialize();
		CreateSeed();
	}

	private void Initialize()
	{
		if (_canRespawn)
		{
			InvokeRepeating("PollRespawn", 0f, 1f * GameTime.OneGameHour);
		}
		else
		{
			UpdateStage(respawned: true);
		}
	}

	public override void OnDestroy()
	{
		if (_canRespawn)
		{
			CancelInvoke("PollRespawn");
		}
		base.OnDestroy();
	}

	private void PollRespawn()
	{
		bool respawned = GameTime.GetHoursSince(_lastHarvestedTime) > HoursToRespawn;
		UpdateStage(respawned);
	}

	public override void Damage(float damage, IBase damager)
	{
		OnDamaged(damage, damager);
		if (!(GameTime.GetHoursSince(_lastHarvestedTime) > HoursToRespawn))
		{
			return;
		}
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
		}
		base.HealthPoints -= damage;
		if (base.HealthPoints <= 0f)
		{
			float originalHealthPoints = base.OriginalHealthPoints;
			if (!pickupable.IsNullOrDestroyed())
			{
				BaseUtilities.DoPlayerDamageExperience_Harvesting(originalHealthPoints, pickupable);
			}
			SpawnHarvest();
		}
	}

	private void SpawnHarvest()
	{
		base.HealthPoints = base.OriginalHealthPoints;
		if (Game.Mode.IsMaster())
		{
			for (int i = 0; i < _resources.Length; i++)
			{
				HarvestResources harvestResources = _resources[i];
				if (harvestResources.SpawnAmount <= 1)
				{
					MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, harvestResources.HarvestPosition.position, harvestResources.HarvestPosition.rotation, MiniGuid.New());
					continue;
				}
				float num = 0.5f;
				Vector3 axis = new Vector3(0f, 1f, 0f);
				Vector3 vector = new Vector3(1f - Mathf.Clamp01(axis.x), 1f - Mathf.Clamp01(axis.y), 1f - Mathf.Clamp01(axis.z));
				int num2 = 0;
				for (int j = 0; j < harvestResources.SpawnAmount; j++)
				{
					float angle = 360f / (float)harvestResources.SpawnAmount * (float)num2++;
					Vector3 vector2 = base.transform.rotation * (Quaternion.AngleAxis(angle, axis) * vector * num);
					Vector3 vector3 = harvestResources.HarvestPosition.position + vector2;
					MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, vector3, harvestResources.HarvestPosition.rotation, MiniGuid.New()).transform.forward = (base.transform.position - vector3).normalized;
				}
			}
		}
		base.transform.CheckForProjectilesInChildren();
		Harvested?.Invoke(this, BaseActionEventData.Empty);
		_ = _canRespawn;
		_lastHarvestedTime = GameTime.Now;
		UpdateStage(respawned: false);
	}

	private void UpdateStage(bool respawned)
	{
		_fullModel.SetActive(respawned);
		if (_harvestedModel != null)
		{
			_harvestedModel.SetActive(!respawned);
		}
		for (int i = 0; i < base.colliders.Length; i++)
		{
			base.colliders[i].enabled = respawned;
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("StartedTime", JSerializer.Serialize(_lastHarvestedTime));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("StartedTime");
		if ((bool)field)
		{
			_lastHarvestedTime = JSerializer.Deserialize<DateTime>(field);
		}
		Initialize();
	}
}
