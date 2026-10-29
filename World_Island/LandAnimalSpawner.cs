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
		if (Singleton<GameTime>.Instance != null)
		{
			Singleton<GameTime>.Instance.HourTick += GameTime_HourTick;
		}
		if (_activeCreature.IsNullOrDestroyed())
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
		if (Singleton<GameTime>.Instance != null)
		{
			Singleton<GameTime>.Instance.HourTick -= GameTime_HourTick;
		}
	}

	private void GameTime_HourTick()
	{
		if (_activeCreature.IsNullOrDestroyed())
		{
			_respawningTimer++;
			PollSpawn();
		}
	}

	private void PollSpawn()
	{
		if (Game.Mode.IsClient())
		{
			return;
		}
		SetDifficultyParams();
		if (_removedWildlifeSetting && !_ignoreRemovedWildLifeSetting)
		{
			return;
		}
		IPlayer player = FindClosestPlayer(base.transform.position, includeDeadPlayers: true);
		if (player != null)
		{
			bool flag = Vector3Tools.DistanceOnAxis(base.transform.position, player.transform.position, Vector3Tools.xz) < 20f;
			if (((_canRespawn && !flag && _respawningTimer > _respawnHours) || _respawningTimer == -1) && !SpawnerInsideStructure())
			{
				_respawningTimer = 0;
				GameObject gameObject = MultiplayerMng.Instantiate<SaveablePrefab>(_landCreature.GetComponent<SaveablePrefab>().PrefabId, base.transform.position, Quaternion.identity, MiniGuid.New()).gameObject;
				_activeCreature = gameObject.GetComponent<LandAnimal>();
			}
		}
	}

	private bool SpawnerInsideStructure()
	{
		if (BeamPhysics.GetComponentsInSphere<Constructing>(base.transform.position, 1.5f, (1 << Layers.CONSTRUCTIONS) | (1 << Layers.CONSTRUCTIONS_SMALL)).Count > 0)
		{
			return true;
		}
		return false;
	}

	private IPlayer FindClosestPlayer(Vector3 position, bool includeDeadPlayers)
	{
		return PlayerUtilities.FindClosestPlayer(position, includeDeadPlayers);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize(base.transform);
		jObject.AddField("Transform", data);
		jObject.AddField("respawnCounter", JSerializer.Serialize(_respawningTimer));
		_lastPollTime = GameTime.Now;
		jObject.AddField("lastPollTime", JSerializer.Serialize(_lastPollTime));
		if (!_activeCreature.IsNullOrDestroyed())
		{
			InteractiveObject component = _activeCreature.GetComponent<InteractiveObject>();
			if (!component.IsNullOrDestroyed())
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
			JSerializer.DeserializeInto(data.GetField("Transform"), base.transform);
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
				LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
				{
					InteractiveObject interactiveObject = MultiplayerMng.FindObject<InteractiveObject>(activeAnimalData.GetValue<string>().ToMiniGuid());
					if (!interactiveObject.IsNullOrDestroyed())
					{
						LandAnimal component = interactiveObject.GetComponent<LandAnimal>();
						if (!component.IsNullOrDestroyed())
						{
							_activeCreature = component;
							_activeCreature.StartingPosition = base.transform.position;
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
