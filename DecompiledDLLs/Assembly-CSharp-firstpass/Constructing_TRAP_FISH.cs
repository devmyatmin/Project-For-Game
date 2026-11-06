using System;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Rendering;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

public class Constructing_TRAP_FISH : ConstructionObject
{
	public class ReplicateTrap : MultiplayerMessage
	{
		[Replicate]
		public Constructing_TRAP_FISH TrapFish;

		[Replicate]
		public short FishPrefabId;

		public override void OnPeer()
		{
			TrapFish.Trap(GetFishPrefab((uint)FishPrefabId));
		}
	}

	private const float CATCH_INTERVAL_HOURS = 3f;

	[SerializeField]
	private RenderableProxy _proxy;

	[SerializeField]
	private GameObject _trapAssembled;

	[SerializeField]
	private GameObject _trapDismantled;

	private bool _trapSet;

	private DateTime _lastCaughtTime;

	private IFishable _currentTrappable;

	[HideInInspector]
	[SerializeField]
	public UnityEvent OnTrapped;

	[HideInInspector]
	[SerializeField]
	public UnityEvent OnSetTrap;

	public bool HasTrapped => _currentTrappable != null;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_trapSet)
		{
			if (HasTrapped)
			{
				return "ITEM_INTERACTION_DESCRIPTION_COLLECT";
			}
			return "ITEM_INTERACTION_DESCRIPTION_DISMANTLE";
		}
		return "ITEM_INTERACTION_DESCRIPTION_SET";
	}

	protected override void Start()
	{
		base.Start();
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
		DismantleTrap();
		_lastCaughtTime = GameTime.Now;
		StartPollingOnOwner();
	}

	private void StartPollingOnOwner()
	{
		if (IsOwner)
		{
			float num = Mathf.Min(1f, 3f) * GameTime.OneGameHour;
			((MonoBehaviour)this).InvokeRepeating("Poll_InvokeRepeating", 1f, num);
		}
	}

	public override void OnDestroy()
	{
		((MonoBehaviour)this).CancelInvoke("Poll_InvokeRepeating");
		OnTrapped = null;
		OnSetTrap = null;
		base.OnDestroy();
	}

	private void SetTrap()
	{
		_trapSet = true;
		_trapAssembled.SetActive(_trapSet);
		_trapDismantled.SetActive(!_trapSet);
		_lastCaughtTime = GameTime.Now;
		DisablePhysics();
		UnityEvent onSetTrap = OnSetTrap;
		if (onSetTrap != null)
		{
			onSetTrap.Invoke();
		}
	}

	private void DismantleTrap()
	{
		_trapSet = false;
		_trapAssembled.SetActive(_trapSet);
		_trapDismantled.SetActive(!_trapSet);
		EnablePhysics();
		ResetTrap();
	}

	private void ResetTrap()
	{
		_proxy.MeshFilter.mesh = null;
		_proxy.Renderer.material = null;
		_currentTrappable = null;
	}

	private void Poll_InvokeRepeating()
	{
		if (_trapSet && ValidatePosition() && GameTime.GetHoursSince(_lastCaughtTime) > 3f && !HasTrapped)
		{
			Trap(SelectRandomFish());
		}
	}

	public void Trap(FishBase trappable)
	{
		if (!((Object)(object)trappable == (Object)null))
		{
			_currentTrappable = trappable;
			_lastCaughtTime = GameTime.Now;
			CreateProxy(trappable.FishTemplate.Renderable);
			UnityEvent onTrapped = OnTrapped;
			if (onTrapped != null)
			{
				onTrapped.Invoke();
			}
			if (IsOwner)
			{
				ReplicateTrap replicateTrap = new ReplicateTrap();
				replicateTrap.TrapFish = this;
				replicateTrap.FishPrefabId = (short)trappable.FishingData.FishPrefab.PrefabId;
				replicateTrap.Post();
			}
		}
	}

	private void CreateProxy(Renderable template)
	{
		_proxy.MeshFilter.sharedMesh = template.Mesh;
		_proxy.Renderer.sharedMaterial = template.Material;
	}

	private static FishBase GetFishPrefab(uint prefabID)
	{
		FishSpawner fishSpawner = Object.FindObjectOfType<PlayerDetailSpawnerManager>().Fish_Spawner as FishSpawner;
		if ((Object)(object)fishSpawner == (Object)null)
		{
			Debug.LogError((object)"Constructing_Trap_Fish :: Load : Error Finding FishSpawners");
			return null;
		}
		foreach (CellObjectSettings<FishBase> cellObjectsSetting in fishSpawner.CellObjectsSettings)
		{
			Interactive_FISH fishPrefab = cellObjectsSetting.Prefab.FishingData.FishPrefab;
			if ((Object)(object)fishPrefab != (Object)null && fishPrefab.PrefabId == prefabID)
			{
				return cellObjectsSetting.Prefab;
			}
		}
		return null;
	}

	private FishBase SelectRandomFish()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		FishSpawner fishSpawner = Object.FindObjectOfType<PlayerDetailSpawnerManager>().Fish_Spawner as FishSpawner;
		if ((Object)(object)fishSpawner == (Object)null)
		{
			Debug.LogError((object)"Constructing_Trap_Fish :: Load : Error Finding FishSpawners");
			return null;
		}
		_ = fishSpawner.CellObjectsSettings;
		int num = 0;
		foreach (CellObjectSettings<FishBase> cellObjectsSetting in fishSpawner.CellObjectsSettings)
		{
			if ((Object)(object)cellObjectsSetting.Prefab.FishingData.FishPrefab != (Object)null && transform.position.y > cellObjectsSetting.MinimumWorldHeight && transform.position.y < cellObjectsSetting.MaximumWorldHeight)
			{
				num += cellObjectsSetting.MaximumInstances;
			}
		}
		float num2 = Random.value * (float)num;
		foreach (CellObjectSettings<FishBase> cellObjectsSetting2 in fishSpawner.CellObjectsSettings)
		{
			if ((Object)(object)cellObjectsSetting2.Prefab.FishingData.FishPrefab != (Object)null && transform.position.y > cellObjectsSetting2.MinimumWorldHeight && transform.position.y < cellObjectsSetting2.MaximumWorldHeight)
			{
				if (num2 <= (float)cellObjectsSetting2.MaximumInstances)
				{
					return cellObjectsSetting2.Prefab;
				}
				num2 -= (float)cellObjectsSetting2.MaximumInstances;
			}
		}
		return null;
	}

	public override bool Interact(IPlayer player)
	{
		if (InteractTrap(player))
		{
			return true;
		}
		return false;
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private bool InteractTrap(IPlayer player)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (_trapSet)
		{
			if (HasTrapped)
			{
				if (MPExtensions.IsMaster(Game.Mode))
				{
					Interactive_FISH interactive_FISH = MultiplayerMng.Instantiate<Interactive_FISH>(_currentTrappable.FishingData.FishPrefab.PrefabId, MiniGuid.New());
					if (player.Holder.Storage.CanPush(interactive_FISH))
					{
						player.Holder.Storage.ReplicatedPush(interactive_FISH);
					}
					else
					{
						interactive_FISH.transform.position = player.transform.position;
					}
					LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "TRAP_FISH_NTF_CAUGHT_FISH_DESC", interactive_FISH.DisplayName);
				}
				EventManager.RaiseEvent(new ExperienceEvent(player.Id, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
				EventManager.RaiseEvent(new AddStatsEvent(player.Id, new Stats
				{
					AnimalsTrapped = 1
				}));
				ResetTrap();
				return true;
			}
			DismantleTrap();
			return true;
		}
		if (ValidatePosition())
		{
			SetTrap();
			return true;
		}
		LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "TRAP_FISH_NTF_CANT_SET_HERE_DESC");
		return false;
	}

	private bool ValidatePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (transform.position.y < 0f)
		{
			Vector3 velocity = rigidbody.velocity;
			if (velocity.magnitude < 0.5f && !IsDragging)
			{
				return true;
			}
		}
		return false;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("TrapSet", JSerializer.Serialize((object)_trapSet));
		jObject.AddField("LastCaughtTime", JSerializer.Serialize((object)_lastCaughtTime));
		if (_currentTrappable != null)
		{
			jObject.AddField("TrappableID", _currentTrappable.FishingData.FishPrefab.PrefabId);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("LastCaughtTime");
		if ((bool)field)
		{
			DateTime lastCaughtTime = JSerializer.Deserialize<DateTime>(field);
			_lastCaughtTime = lastCaughtTime;
		}
		JObject field2 = data.GetField("TrapSet");
		if (field2 != null)
		{
			if (field2.GetValue<bool>())
			{
				SetTrap();
			}
			else
			{
				DismantleTrap();
			}
		}
		else
		{
			DismantleTrap();
		}
		JObject field3 = data.GetField("TrappableID");
		if (field3 != null)
		{
			FishBase fishPrefab = GetFishPrefab(Prefabs.ConvertLegacyPrefabId(field3));
			if ((Object)(object)fishPrefab != (Object)null)
			{
				_currentTrappable = fishPrefab;
				CreateProxy(fishPrefab.FishTemplate.Renderable);
			}
		}
		StartPollingOnOwner();
	}
}
