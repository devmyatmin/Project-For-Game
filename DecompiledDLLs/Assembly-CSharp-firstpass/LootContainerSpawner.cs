using System;
using System.Collections.Generic;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class LootContainerSpawner : SaveablePrefab
{
	[Serializable]
	public class Spawn
	{
		public string Name;

		public Transform position;

		public Container[] containers;

		[HideInInspector]
		public bool occupied;
	}

	[Serializable]
	public class Container
	{
		[HideInInspector]
		public string Name;

		public GameObject prefab;

		public float rarity;
	}

	[HideInInspector]
	public bool debugGizmos;

	[SerializeField]
	private Transform _shipwreckMarker;

	[SerializeField]
	private bool IsOnShipwreck = true;

	[SerializeField]
	private List<Spawn> _containerSpawn;

	private List<MiniGuid> _containerIds = new List<MiniGuid>();

	private bool _containersGenerated;

	private bool _scavenged;

	private bool _customMap;

	public List<Spawn> containerSpawn => _containerSpawn;

	private void Start()
	{
		debugGizmos = false;
		if (!LevelLoader.IsLoading(((Component)this).gameObject))
		{
			Initialize();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		_customMap = true;
		Initialize();
	}

	private void Initialize()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		SpawnContainers();
		if ((Object)(object)_shipwreckMarker != (Object)null)
		{
			((Component)_shipwreckMarker).transform.position = new Vector3(((Component)this).transform.position.x, 0.2f, ((Component)this).transform.position.z);
		}
	}

	private void SpawnContainers()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		if (_containersGenerated)
		{
			return;
		}
		WollieSpawner component = ((Component)this).gameObject.GetComponent<WollieSpawner>();
		if ((Object)(object)component != (Object)null)
		{
			Zone zone = StrandedWorld.GetZone(((Component)this).transform.position);
			if ((Object)(object)zone != (Object)null)
			{
				if (zone.HasShelter && zone.HasWollie)
				{
					_containerSpawn.Add(new Spawn
					{
						position = component.WolliePosition,
						containers = new Container[1]
						{
							new Container
							{
								prefab = Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.WolliePrefab,
								rarity = 100f
							}
						}
					});
				}
			}
			else
			{
				Debug.LogWarning((object)"LootContainerSpawner :: Error finding zone for Wollie Spawner!");
			}
		}
		Random random = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(((Component)this).transform.position);
		foreach (Spawn item in containerSpawn)
		{
			float num = random.Next(1000) / 10;
			float num2 = 0f;
			Container[] containers = item.containers;
			foreach (Container container in containers)
			{
				if (num <= num2 + container.rarity && !item.occupied)
				{
					Vector3 position = item.position.position;
					Quaternion rotation = item.position.rotation;
					ISaveablePrefab saveablePrefab = BeamTeam.GetInterface<ISaveablePrefab>(container.prefab);
					if (!BeamTeam.IsNullOrDestroyed(saveablePrefab))
					{
						uint prefabId = saveablePrefab.PrefabId;
						MiniGuid referenceId = MiniGuid.NewFrom(position, prefabId, 48879);
						bool flag = Prefabs.IsMultiplayerEntity(prefabId);
						if (MPExtensions.IsMaster(Game.Mode) || !flag)
						{
							Setup(MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, position, rotation, referenceId));
						}
						else
						{
							MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.LootContainerSpawner, referenceId, (MultiplayerZoneSync.Command command) =>
							{
								Setup(command.Instance);
							});
						}
					}
					else
					{
						Object.Instantiate<GameObject>(container.prefab, position, rotation, ((Component)this).transform.parent);
					}
					item.occupied = true;
				}
				num2 += container.rarity;
			}
		}
		_containersGenerated = true;
	}

	private void Setup(SaveablePrefab saveablePrefab)
	{
		InteractiveObject interactiveObject = saveablePrefab as InteractiveObject;
		if ((Object)(object)interactiveObject != (Object)null)
		{
			interactiveObject.transform.parent = ((Component)this).transform.parent;
			interactiveObject.TransformParent = ((Component)this).transform.parent;
			if (_customMap)
			{
				interactiveObject.OnCustomMapLoaded();
			}
			Interactive_STORAGE interactive_STORAGE = interactiveObject as Interactive_STORAGE;
			if ((Object)(object)interactive_STORAGE != (Object)null)
			{
				interactive_STORAGE.PickedUp += Storage_PickedUp;
				interactive_STORAGE.BeginDragged += Storage_BeginDragged;
				interactive_STORAGE.Opened += Storage_Opened;
				_containerIds.Add(interactive_STORAGE.ReferenceId);
			}
		}
	}

	private void Storage_Opened(Interactive_STORAGE sender)
	{
		CheckScavenged(sender.ReferenceId);
	}

	private void Storage_PickedUp(IPickupable sender)
	{
		CheckScavenged(sender.ReferenceId);
	}

	private void Storage_BeginDragged(IDraggable sender)
	{
		ISaveablePrefab saveablePrefab = sender as ISaveablePrefab;
		if (!BeamTeam.IsNullOrDestroyed(saveablePrefab))
		{
			CheckScavenged(saveablePrefab.ReferenceId);
		}
	}

	private void CheckScavenged(MiniGuid refId)
	{
		if (!_containersGenerated || _scavenged)
		{
			return;
		}
		int num = _containerIds.FindIndex((MiniGuid id) => id.Equals(refId));
		if (num == -1)
		{
			return;
		}
		_containerIds.RemoveAt(num);
		if (_containerIds.Count == 0)
		{
			if (IsOnShipwreck)
			{
				EventManager.RaiseEvent(new AddStatsEvent(0, new Stats
				{
					ShipsScavenged = 1
				}));
			}
			else
			{
				EventManager.RaiseEvent(new ScavengedShelterEvent());
			}
			_scavenged = true;
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)((Component)this).transform);
		jObject.AddField("Transform", data);
		jObject.AddField("containersGenerated", JSerializer.Serialize((object)_containersGenerated));
		jObject.AddField("Scavenged", JSerializer.Serialize((object)_scavenged));
		JObject jObject2 = new JObject();
		for (int i = 0; i < _containerIds.Count; i++)
		{
			jObject2.Add(JSerializer.Serialize((object)_containerIds[i].ToString()));
		}
		jObject.AddField("ContainerIDs", jObject2);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (data != null && !data.IsNull())
		{
			JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
			CompleteTransformSerialization();
			bool value = data.GetField("containersGenerated").GetValue<bool>();
			_containersGenerated = value;
			JObject field = data.GetField("Scavenged");
			if (field != null)
			{
				_scavenged = field.GetValue<bool>();
			}
			JObject field2 = data.GetField("ContainerIDs");
			if (field2 != null && field2.Children != null)
			{
				for (int i = 0; i < field2.Children.Count; i++)
				{
					MiniGuid miniGuid = Beam.Utilities.ExtensionMethods.ToMiniGuid(field2.Children[i].GetValue<string>());
					if (!miniGuid.IsDefault())
					{
						Interactive_STORAGE interactive_STORAGE = MultiplayerMng.FindObject<Interactive_STORAGE>(miniGuid);
						if ((Object)(object)interactive_STORAGE != (Object)null)
						{
							interactive_STORAGE.PickedUp += Storage_PickedUp;
							interactive_STORAGE.BeginDragged += Storage_BeginDragged;
							interactive_STORAGE.Opened += Storage_Opened;
							_containerIds.Add(miniGuid);
						}
					}
				}
			}
		}
		Initialize();
	}
}
