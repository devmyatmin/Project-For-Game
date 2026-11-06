using System;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.UI;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

public class Constructing_TRAP_BIRD : Constructing, IInteractable, IBase, IHasCraftingType
{
	public class ReplicateTrap : MultiplayerMessage
	{
		[Replicate]
		public Constructing_TRAP_BIRD Trap;

		[Replicate]
		public Interactive_BIRD Trappable;

		public override void OnPeer()
		{
			Trap.TrapBird(Trappable);
		}
	}

	private class ReplicateKillNewBird : I_MultiplayerMessage
	{
		[Replicate]
		public Constructing_TRAP_BIRD Trap;

		[I_Replicate]
		public Interactive_BIRD Bird;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Trap.KillNewBird(Bird, Player);
		}
	}

	[Header("INTERACTING")]
	[SerializeField]
	private bool _canInteract;

	private const float CATCH_INTERVAL_HOURS = 0.05f;

	[SerializeField]
	private GameObject _proxyGull;

	[SerializeField]
	private Interactive_BIRD _prefabGull;

	[SerializeField]
	private LandingSpot _landingSpot;

	private bool _hasTrapped;

	private DateTime _lastCaughtTime;

	private Interactive_BIRD _reward;

	[HideInInspector]
	public UnityEvent OnTrapped;

	public bool CanInteract
	{
		get
		{
			return _canInteract;
		}
		set
		{
			_canInteract = value;
		}
	}

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (!_hasTrapped)
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_COLLECT";
	}

	protected override void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if (!MPExtensions.IsClient(Game.Mode))
		{
			_landingSpot.Landed += OnBirdLandedOnTrap;
			if (!LevelLoader.IsLoading(((Component)this).gameObject))
			{
				_lastCaughtTime = GameTime.Now;
				Initialize();
			}
		}
	}

	private void Initialize()
	{
		float num = Mathf.Min(1f, 0.05f) * GameTime.OneGameHour;
		((MonoBehaviour)this).InvokeRepeating("Poll_InvokeRepeating", 1f, num);
	}

	private void OnDestroy()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			return;
		}
		if (_hasTrapped && Object.op_Implicit((Object)(object)_reward))
		{
			Transform val = _reward.transform;
			LandingSpot landingSpot = _landingSpot;
			Nullable val2;
			if (landingSpot == null)
			{
				val2 = null;
			}
			else
			{
				Transform val3 = ((Component)landingSpot).transform;
				val2 = ((val3 != null) ? new Nullable(val3.position) : ((Nullable)null));
			}
			val.position = val2 ?? transform.position;
			_reward.rigidbody.isKinematic = false;
		}
		((MonoBehaviour)this).CancelInvoke("Poll_InvokeRepeating");
	}

	private void Poll_InvokeRepeating()
	{
		ValidateLandingspot();
	}

	private void ValidateLandingspot()
	{
		if (GameTime.GetHoursSince(_lastCaughtTime) < 0.05f)
		{
			_landingSpot.Occupied = true;
		}
		else if (!_hasTrapped)
		{
			_landingSpot.Occupied = false;
		}
	}

	private void OnBirdLandedOnTrap()
	{
		FlockChild flockChild = _landingSpot.FlockChild;
		if (!((Object)(object)flockChild == (Object)null))
		{
			Interactive_BIRD component = ((Component)flockChild).GetComponent<Interactive_BIRD>();
			ReplicatedTrap(component);
		}
	}

	public void TrapBird(Interactive_BIRD trappable)
	{
		if ((Object)(object)trappable != (Object)null)
		{
			trappable.Kill();
			MultiplayerMng.Destroy(trappable);
		}
		_lastCaughtTime = GameTime.Now;
		_hasTrapped = true;
		_landingSpot.Occupied = true;
		CreateProxy();
		UnityEvent onTrapped = OnTrapped;
		if (onTrapped != null)
		{
			onTrapped.Invoke();
		}
	}

	public void ReplicatedTrap(Interactive_BIRD trappable)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateTrap replicateTrap = new ReplicateTrap();
			replicateTrap.Trap = this;
			replicateTrap.Trappable = trappable;
			replicateTrap.Post();
		}
		TrapBird(trappable);
	}

	private void CreateProxy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		_proxyGull.SetActive(true);
		if (MPExtensions.IsMaster(Game.Mode))
		{
			Interactive_BIRD reward = MultiplayerMng.Instantiate<Interactive_BIRD>(_prefabGull.PrefabId, Vector3.zero, Quaternion.identity, MiniGuid.New());
			ReplicatedKillNewBird(reward, PlayerRegistry.LocalPlayer);
		}
	}

	public void KillNewBird(Interactive_BIRD reward, IPlayer player)
	{
		_reward = reward;
		_reward.Kill(player.Id);
		_reward.rigidbody.isKinematic = true;
	}

	public void ReplicatedKillNewBird(Interactive_BIRD reward, IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateKillNewBird replicateKillNewBird = new ReplicateKillNewBird();
			replicateKillNewBird.Trap = this;
			replicateKillNewBird.Bird = reward;
			replicateKillNewBird.Player = player;
			replicateKillNewBird.Post();
		}
		KillNewBird(reward, player);
	}

	private void ResetTrap()
	{
		_proxyGull.SetActive(false);
		_reward = null;
		_lastCaughtTime = GameTime.Now;
		_hasTrapped = false;
		ValidateLandingspot();
	}

	public bool Interact(IPlayer player)
	{
		if (CollectTrap(player))
		{
			return true;
		}
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private bool CollectTrap(IPlayer player)
	{
		if (!_hasTrapped)
		{
			return false;
		}
		if ((Object)(object)_reward == (Object)null)
		{
			return false;
		}
		if (player.Holder.CanPickup(_reward))
		{
			player.Holder.Pickup(_reward);
		}
		else
		{
			if (!player.Holder.Storage.CanPush(_reward))
			{
				return false;
			}
			player.Holder.Storage.Push(_reward);
		}
		LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "TRAP_BIRD_NTF_CAUGHT_BIRD_DESC", _prefabGull.DisplayName);
		EventManager.RaiseEvent(new ExperienceEvent(player.Id, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
		EventManager.RaiseEvent(new AddStatsEvent(player.Id, new Stats
		{
			AnimalsTrapped = 1
		}));
		ResetTrap();
		return true;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("LastCaughtTime", JSerializer.Serialize((object)_lastCaughtTime));
		jObject.AddField("HasTrapped", JSerializer.Serialize((object)_hasTrapped));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		DateTime lastCaughtTime = JSerializer.Deserialize<DateTime>(data.GetField("LastCaughtTime"));
		_lastCaughtTime = lastCaughtTime;
		_hasTrapped = data.GetField("HasTrapped").GetValue<bool>();
		if (_hasTrapped)
		{
			_landingSpot.Occupied = true;
			CreateProxy();
		}
		Initialize();
	}
}
