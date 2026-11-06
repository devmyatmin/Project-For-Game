using Beam;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

public class PiscusManager : SaveablePrefab
{
	[SerializeField]
	private GameObject _piscusCreature;

	private Piscus_Creature _activeCreature;

	[SerializeField]
	private float _spawnDistance = 60f;

	private float _territoryRadius;

	[SerializeField]
	private bool _canRespawn;

	[SerializeField]
	private int _respawnHours;

	private int _respawningTimer;

	private float _lastPollTime;

	public float SpawnDistance
	{
		get
		{
			return _spawnDistance;
		}
		set
		{
			_spawnDistance = value;
		}
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -1f, ((Component)this).transform.position.z);
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
			{
				Singleton<GameTime>.Instance.HourTick += GameTime_HourTick;
			}
			CheckInsideTerrain();
			_territoryRadius = _spawnDistance - 15f;
			float num = 5f;
			((MonoBehaviour)this).InvokeRepeating("PollActivation_InvokeRepeating", 0f, num);
			PollSpawn();
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.HourTick -= GameTime_HourTick;
		}
		if ((Object)(object)_activeCreature != (Object)null)
		{
			MultiplayerMng.DestroyMultiplayerEntity(((Component)_activeCreature).gameObject);
		}
	}

	private void GameTime_HourTick()
	{
		if ((Object)(object)_activeCreature == (Object)null)
		{
			_respawningTimer++;
			PollSpawn();
		}
	}

	private void PollActivation_InvokeRepeating()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsServerJoinInProgress)
		{
			IPlayer player = FindClosestPlayer(((Component)this).transform.position, includeDeadPlayers: true);
			if (player != null && (Object)(object)_activeCreature != (Object)null)
			{
				bool activate = Vector3Tools.DistanceOnAxis(((Component)this).transform.position, player.transform.position, Vector3Tools.xz) < _spawnDistance;
				Activate_Creature(activate);
			}
		}
	}

	private void PollSpawn()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		IPlayer player = FindClosestPlayer(((Component)this).transform.position, includeDeadPlayers: true);
		if (player != null)
		{
			bool flag = Vector3Tools.DistanceOnAxis(((Component)this).transform.position, player.transform.position, Vector3Tools.xz) < _spawnDistance;
			if ((_canRespawn && !flag && _respawningTimer > _respawnHours) || _respawningTimer == 0)
			{
				_respawningTimer = 0;
				GameObject gameObject = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(_piscusCreature.GetComponent<SaveablePrefab>().PrefabId, ((Component)this).transform.position, Quaternion.identity, MiniGuid.New())).gameObject;
				gameObject.transform.parent = ((Component)Singleton<PiscusInspectionManager>.Instance).transform;
				_activeCreature = gameObject.GetComponent<Piscus_Creature>();
				_activeCreature.BiomeSize = _territoryRadius;
				_activeCreature.EngagePlayerDistance = _activeCreature.BiomeSize * 0.5f;
				_activeCreature.Manager = this;
			}
		}
	}

	private void Activate_Creature(bool activate)
	{
		_lastPollTime = 0f;
		if (((Component)_activeCreature).gameObject.activeSelf != activate)
		{
			((Component)_activeCreature).gameObject.GetComponent<SaveablePrefab>().Active = activate;
			if (activate)
			{
				_activeCreature.CurrentState = Piscus_Creature.PiscusBehaviour.Explore;
			}
		}
	}

	private void CheckInsideTerrain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.CheckSphere(((Component)this).transform.position, 2f, 1 << Layers.TERRAIN) || Physics.Raycast(((Component)this).transform.position + Vector3.up * 5f, -Vector3.up, 7f, 1 << Layers.TERRAIN))
		{
			Debug.Log((object)"Manager :: Piscus Manager Destroyed :: Collision with terrain");
			MultiplayerMng.DestroyMultiplayerEntity(((Component)this).gameObject);
		}
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
		jObject.AddField("spawnDistance", JSerializer.Serialize((object)_spawnDistance));
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		if (data != null && !data.IsNull())
		{
			JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
			CompleteTransformSerialization();
			int value = data.GetField("respawnCounter").GetValue<int>();
			_respawningTimer = value;
			int value2 = data.GetField("spawnDistance").GetValue<int>();
			_spawnDistance = value2;
			float x = ((Component)this).transform.localPosition.x;
			float z = ((Component)this).transform.localPosition.z;
			float y = ((Component)this).transform.localPosition.y;
			if (Mathf.Abs(x) == 100f && Mathf.Abs(z) == 100f)
			{
				x = ((!(x > 0f)) ? (-88f) : 88f);
				z = ((!(z > 0f)) ? (-88f) : 88f);
				((Component)this).transform.localPosition = new Vector3(x, y, z);
			}
		}
		Initialize();
	}
}
