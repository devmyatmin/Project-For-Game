using System;
using System.Collections.Generic;
using Beam;
using Beam.Rendering;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class Module : SaveablePrefab
{
	[Serializable]
	public class Spawn
	{
		public string Name;

		public Transform Position;

		public GameObject Obj;

		public float SpawnChance;
	}

	public string[] Tags;

	public GameObject FortLadder;

	private bool _seaLadderAccess;

	[SerializeField]
	private List<Spawn> _objectSpawn;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private LodController _lodController;

	private bool _customMap;

	public List<Spawn> ObjectSpawn => _objectSpawn;

	public ModuleConnector[] GetExits()
	{
		return ((Component)this).GetComponentsInChildren<ModuleConnector>();
	}

	public void Start()
	{
		if (!LevelLoader.IsLoading())
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (_objectSpawn.Count > 0)
		{
			SpawnObjects();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		_customMap = true;
		Initialize();
	}

	private void SetupSpawnedObject(SaveablePrefab saveable)
	{
		if (!BeamTeam.IsValid(saveable))
		{
			return;
		}
		InteractiveObject component = ((Component)saveable).GetComponent<InteractiveObject>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.transform.parent = ((Component)this).transform.parent;
			component.TransformParent = ((Component)this).transform.parent;
			if (_customMap)
			{
				component.OnCustomMapLoaded();
			}
		}
	}

	private void SpawnObject(GameObject objToSpawn, Vector3 pos, Quaternion rot)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ISaveablePrefab saveablePrefab = default;
		if (objToSpawn.TryGetComponent<ISaveablePrefab>(ref saveablePrefab))
		{
			uint prefabId = saveablePrefab.PrefabId;
			MiniGuid referenceId = MiniGuid.NewFrom(pos, prefabId, 48879);
			if (MPExtensions.IsMaster(Game.Mode) || !saveablePrefab.IsMultiplayerEntity)
			{
				SaveablePrefab saveable = MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, pos, rot, referenceId);
				SetupSpawnedObject(saveable);
			}
			else
			{
				MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.Module, referenceId, (MultiplayerZoneSync.Command cmd) =>
				{
					SetupSpawnedObject(cmd.Instance);
				});
			}
		}
		else
		{
			Object.Instantiate<GameObject>(objToSpawn, pos, rot);
		}
	}

	private void SpawnObjects()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Random random = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(((Component)this).transform.position);
		foreach (Spawn item in _objectSpawn)
		{
			if ((float)(random.Next(1000) / 10) <= item.SpawnChance)
			{
				SpawnObject(item.Obj, item.Position.position, item.Position.rotation);
			}
		}
	}

	public void SetUpSupport(bool seaLadderAccess = false)
	{
		if (!((Object)(object)FortLadder == (Object)null))
		{
			_seaLadderAccess = seaLadderAccess;
			FortLadder.SetActive(seaLadderAccess);
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)((Component)this).transform);
		jObject.AddField("Transform", data);
		jObject.AddField("hasLadder", JSerializer.Serialize((object)_seaLadderAccess));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (data != null && !data.IsNull())
		{
			JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
			CompleteTransformSerialization();
			bool value = data.GetField("hasLadder").GetValue<bool>();
			_seaLadderAccess = value;
		}
		SetUpSupport(_seaLadderAccess);
	}
}
