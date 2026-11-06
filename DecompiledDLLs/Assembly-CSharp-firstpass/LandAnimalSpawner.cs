using System;
using Beam;
using Beam.Crafting;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class LandAnimalSpawner : SaveablePrefab
{
	[SerializeField]
	private GameObject _landCreature;

	private LandAnimal _activeCreature;

	[SerializeField]
	private bool _canRespawn;

	[SerializeField]
	private bool _ignoreRemovedWildLifeSetting;

	private bool _removedWildlifeSetting;

	[SerializeField]
	private int _respawnHours;

	private int _respawningTimer = -1;

	private DateTime _lastPollTime;

	private void Start()
	{
		if (!LevelLoader.IsLoading())
		{
			Initialize();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		Initialize();
	}

	private void Initialize()
	{
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.HourTick += GameTime_HourTick;
		}
		if (BeamTeam.IsNullOrDestroyed(_activeCreature))
		{
			PollSpawn();
		}
	}

	private void SetDifficultyParams()
	{
		_removedWildlifeSetting = PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Removed;
	}

	private void OnDestroy()
	{
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.HourTick -= GameTime_HourTick;
		}
	}

	private void GameTime_HourTick()
	{
		if (BeamTeam.IsNullOrDestroyed(_activeCreature))
		{
			_respawningTimer++;
			PollSpawn();
		}
	}

	private void PollSpawn()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			return;
		}
		SetDifficultyParams();
		if (_removedWildlifeSetting && !_ignoreRemovedWildLifeSetting)
		{
			return;
		}
		IPlayer player = FindClosestPlayer(((Component)this).transform.position, includeDeadPlayers: true);
		if (player != null)
		{
			bool flag = Vector3Tools.DistanceOnAxis(((Component)this).transform.position, player.transform.position, Vector3Tools.xz) < 20f;
			if (((_canRespawn && !flag && _respawningTimer > _respawnHours) || _respawningTimer == -1) && !SpawnerInsideStructure())
			{
				_respawningTimer = 0;
				GameObject gameObject = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(_landCreature.GetComponent<SaveablePrefab>().PrefabId, ((Component)this).transform.position, Quaternion.identity, MiniGuid.New())).gameObject;
				_activeCreature = gameObject.GetComponent<LandAnimal>();
			}
		}
	}

	private bool SpawnerInsideStructure()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (BeamPhysics.GetComponentsInSphere<Constructing>(((Component)this).transform.position, 1.5f, (1 << Layers.CONSTRUCTIONS) | (1 << Layers.CONSTRUCTIONS_SMALL)).Count > 0)
		{
			return true;
		}
		return false;
	}

	private IPlayer FindClosestPlayer(Vector3 position, bool includeDeadPlayers)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return PlayerUtilities.FindClosestPlayer(position, includeDeadPlayers);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)((Component)this).transform);
		jObject.AddField("Transform", data);
		jObject.AddField("respawnCounter", JSerializer.Serialize((object)_respawningTimer));
		_lastPollTime = GameTime.Now;
		jObject.AddField("lastPollTime", JSerializer.Serialize((object)_lastPollTime));
		if (!BeamTeam.IsNullOrDestroyed(_activeCreature))
		{
			InteractiveObject component = ((Component)_activeCreature).GetComponent<InteractiveObject>();
			if (!BeamTeam.IsNullOrDestroyed(component))
			{
				jObject.AddField("ActiveAnimal", component.ReferenceId.ToString());
			}
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (data != null && !data.IsNull())
		{
			JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
			CompleteTransformSerialization();
			int value = data.GetField("respawnCounter").GetValue<int>();
			_respawningTimer = value;
			JObject field = data.GetField("lastPollTime");
			if (field != null)
			{
				_lastPollTime = JSerializer.Deserialize<DateTime>(field);
				int num = (int)GameTime.GetHoursSince(_lastPollTime);
				if (_respawningTimer > 0)
				{
					_respawningTimer += num;
				}
			}
			JObject activeAnimalData = data.GetField("ActiveAnimal");
			if (activeAnimalData != null)
			{
				LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
				{
					//IL_0050: Unknown result type (might be due to invalid IL or missing references)
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					InteractiveObject interactiveObject = MultiplayerMng.FindObject<InteractiveObject>(Beam.Utilities.ExtensionMethods.ToMiniGuid(activeAnimalData.GetValue<string>()));
					if (!BeamTeam.IsNullOrDestroyed(interactiveObject))
					{
						LandAnimal component = ((Component)interactiveObject).GetComponent<LandAnimal>();
						if (!BeamTeam.IsNullOrDestroyed(component))
						{
							_activeCreature = component;
							_activeCreature.StartingPosition = ((Component)this).transform.position;
							Initialize();
						}
					}
				});
				return;
			}
		}
		Initialize();
	}
}
