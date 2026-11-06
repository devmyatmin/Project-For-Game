using System;
using Beam;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

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
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_seedPrefab == (Object)null) && !MPExtensions.IsClient(Game.Mode))
		{
			InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.Instantiate<InteractiveObject_FOOD>(_seedPrefab.PrefabId, MiniGuid.New(), transform.parent);
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
			_lastHarvestedTime = DateTimeTools.RemoveHours(GameTime.Now, HoursToRespawn * 2f);
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
		_lastHarvestedTime = DateTimeTools.RemoveHours(GameTime.Now, HoursToRespawn * 2f);
		Initialize();
		CreateSeed();
	}

	private void Initialize()
	{
		if (_canRespawn)
		{
			((MonoBehaviour)this).InvokeRepeating("PollRespawn", 0f, 1f * GameTime.OneGameHour);
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
			((MonoBehaviour)this).CancelInvoke("PollRespawn");
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
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
		}
		HealthPoints -= damage;
		if (HealthPoints <= 0f)
		{
			float originalHealthPoints = OriginalHealthPoints;
			if (!BeamTeam.IsNullOrDestroyed(pickupable))
			{
				BaseUtilities.DoPlayerDamageExperience_Harvesting(originalHealthPoints, pickupable);
			}
			SpawnHarvest();
		}
	}

	private void SpawnHarvest()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		HealthPoints = OriginalHealthPoints;
		if (MPExtensions.IsMaster(Game.Mode))
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
				Vector3 val = new Vector3(0f, 1f, 0f);
				Vector3 val2 = new Vector3(1f - Mathf.Clamp01(val.x), 1f - Mathf.Clamp01(val.y), 1f - Mathf.Clamp01(val.z));
				int num2 = 0;
				for (int j = 0; j < harvestResources.SpawnAmount; j++)
				{
					float num3 = 360f / (float)harvestResources.SpawnAmount * (float)num2++;
					Vector3 val3 = transform.rotation * (Quaternion.AngleAxis(num3, val) * val2 * num);
					Vector3 val4 = harvestResources.HarvestPosition.position + val3;
					Transform val5 = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, val4, harvestResources.HarvestPosition.rotation, MiniGuid.New())).transform;
					Vector3 val6 = transform.position - val4;
					val5.forward = val6.normalized;
				}
			}
		}
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
		((UnityEvent<IBase, IBaseActionEventData>)Harvested)?.Invoke((IBase)this, (IBaseActionEventData)BaseActionEventData.Empty);
		_ = _canRespawn;
		_lastHarvestedTime = GameTime.Now;
		UpdateStage(respawned: false);
	}

	private void UpdateStage(bool respawned)
	{
		_fullModel.SetActive(respawned);
		if ((Object)(object)_harvestedModel != (Object)null)
		{
			_harvestedModel.SetActive(!respawned);
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = respawned;
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("StartedTime", JSerializer.Serialize((object)_lastHarvestedTime));
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
