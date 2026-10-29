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
		if (!LevelLoader.IsLoading(base.gameObject))
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
		SpawnContainers();
		if (_shipwreckMarker != null)
		{
			_shipwreckMarker.transform.position = new Vector3(base.transform.position.x, 0.2f, base.transform.position.z);
		}
	}

	private void SpawnContainers()
	{
		if (_containersGenerated)
		{
			return;
		}
		WollieSpawner component = base.gameObject.GetComponent<WollieSpawner>();
		if (component != null)
		{
			Zone zone = StrandedWorld.GetZone(base.transform.position);
			if (zone != null)
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
				Debug.LogWarning("LootContainerSpawner :: Error finding zone for Wollie Spawner!");
			}
		}
		System.Random random = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(base.transform.position);
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
					ISaveablePrefab @interface = container.prefab.GetInterface<ISaveablePrefab>();
					if (!@interface.IsNullOrDestroyed())
					{
						uint prefabId = @interface.PrefabId;
						MiniGuid referenceId = MiniGuid.NewFrom(position, prefabId, 48879);
						bool flag = Prefabs.IsMultiplayerEntity(prefabId);
						if (Game.Mode.IsMaster() || !flag)
						{
							Setup(MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, position, rotation, referenceId));
						}
						else
						{
							MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.LootContainerSpawner, referenceId, delegate(MultiplayerZoneSync.Command command)
							{
								Setup(command.Instance);
							});
						}
					}
					else
					{
						UnityEngine.Object.Instantiate(container.prefab, position, rotation, base.transform.parent);
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
		if (interactiveObject != null)
		{
			interactiveObject.transform.parent = base.transform.parent;
			interactiveObject.TransformParent = base.transform.parent;
			if (_customMap)
			{
				interactiveObject.OnCustomMapLoaded();
			}
			Interactive_STORAGE interactive_STORAGE = interactiveObject as Interactive_STORAGE;
			if (interactive_STORAGE != null)
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
		if (!saveablePrefab.IsNullOrDestroyed())
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
		JObject data = JSerializer.Serialize(base.transform);
		jObject.AddField("Transform", data);
		jObject.AddField("containersGenerated", JSerializer.Serialize(_containersGenerated));
		jObject.AddField("Scavenged", JSerializer.Serialize(_scavenged));
		JObject jObject2 = new JObject();
		for (int i = 0; i < _containerIds.Count; i++)
		{
			jObject2.Add(JSerializer.Serialize(_containerIds[i].ToString()));
		}
		jObject.AddField("ContainerIDs", jObject2);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (data != null && !data.IsNull())
		{
			JSerializer.DeserializeInto(data.GetField("Transform"), base.transform);
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
					MiniGuid miniGuid = field2.Children[i].GetValue<string>().ToMiniGuid();
					if (!miniGuid.IsDefault())
					{
						Interactive_STORAGE interactive_STORAGE = MultiplayerMng.FindObject<Interactive_STORAGE>(miniGuid);
						if (interactive_STORAGE != null)
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
