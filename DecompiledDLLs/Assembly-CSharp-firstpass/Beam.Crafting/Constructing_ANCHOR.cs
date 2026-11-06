using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Ceto;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

public class Constructing_ANCHOR : Constructing, IInteractable, IBase, IHasCraftingType
{
	[SerializeField]
	private Transform _packedAnchorTransform;

	[SerializeField]
	private GameObject _deployedAnchorGameObject;

	[SerializeField]
	private Transform _deployedAnchorTransform;

	[SerializeField]
	private VerletLine _deployedAnchorRope;

	private Construction_STRUCTURE_RAFT _raftStructure;

	private bool _serverJoinInProgressAttemptDeploy;

	private bool _deployed;

	[HideInInspector]
	public UnityEvent OnDeploy;

	[HideInInspector]
	public UnityEvent OnRetrieve;

	public bool CanInteract { get; set; } = true;

	protected Construction_STRUCTURE_RAFT RaftStructure
	{
		get
		{
			if ((Object)(object)_raftStructure == (Object)null)
			{
				_raftStructure = Structure as Construction_STRUCTURE_RAFT;
			}
			return _raftStructure;
		}
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!_deployed)
		{
			return "ITEM_INTERACTION_DESCRIPTION_DROP";
		}
		return "ITEM_INTERACTION_DESCRIPTION_RETRIEVE";
	}

	protected override void Start()
	{
		base.Start();
		EventManager.AddListener<ServerJoinInProgressStarted>(OnServerJoinInProgressStarted);
		EventManager.AddListener<ServerJoinInProgressEnded>(OnServerJoinInProgressEnded);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<ServerJoinInProgressStarted>(OnServerJoinInProgressStarted);
		EventManager.RemoveListener<ServerJoinInProgressEnded>(OnServerJoinInProgressEnded);
		if (_deployed)
		{
			Retrieve();
		}
		if ((Object)(object)_deployedAnchorTransform != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)_deployedAnchorTransform).gameObject);
		}
	}

	private void OnServerJoinInProgressStarted(ServerJoinInProgressStarted @event)
	{
		_serverJoinInProgressAttemptDeploy = false;
		if (!_deployed && !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer) && PlayerRegistry.LocalPlayer.IsInitialized && !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer.Movement) && !BeamTeam.IsNullOrDestroyed(Structure) && !((Object)(object)Structure.transform != (Object)(object)PlayerRegistry.LocalPlayer.Movement.ActivePlatform))
		{
			_serverJoinInProgressAttemptDeploy = true;
			Interact(PlayerRegistry.LocalPlayer);
		}
	}

	private void OnServerJoinInProgressEnded(ServerJoinInProgressEnded @event)
	{
		if (_deployed && _serverJoinInProgressAttemptDeploy)
		{
			LocalizedNotification.Post(PlayerRegistry.LocalPlayer, NotificationPriority.Low, 4f, "ANCHOR_NTF_SERVER_JIP_DEPLOYED");
			_serverJoinInProgressAttemptDeploy = false;
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		base.Damage(damage, damager);
	}

	public bool Interact(IPlayer player)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!_deployed)
		{
			if (!CanDeploy())
			{
				LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "ANCHOR_NTF_CANT_DROP_HERE_DESC");
				return false;
			}
			Deploy(GetAnchorPlacement(transform.position));
			return true;
		}
		if (_deployed)
		{
			Retrieve();
			return true;
		}
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private void Deploy(Placement placement)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_deployedAnchorGameObject.SetActive(true);
		_deployedAnchorTransform.position = placement.Position + new Vector3(0f, 0.9f, 0f);
		_deployedAnchorTransform.rotation = placement.Rotation;
		_deployedAnchorTransform.parent = transform.parent.parent;
		((Component)_deployedAnchorRope).gameObject.SetActive(true);
		((Component)_packedAnchorTransform).gameObject.SetActive(false);
		RaftStructure.StructureBuoyancy.EnableWind(enable: false);
		RaftStructure.StructureBuoyancy.SetDampening(0.7f);
		_deployed = true;
		_raftStructure.AnchorsDeployed++;
		UnityEvent onDeploy = OnDeploy;
		if (onDeploy != null)
		{
			onDeploy.Invoke();
		}
	}

	public void Retrieve()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		_deployedAnchorGameObject.SetActive(false);
		_deployedAnchorTransform.parent = _deployedAnchorGameObject.transform;
		_deployedAnchorTransform.localPosition = Vector3.zero;
		((Component)_deployedAnchorRope).gameObject.SetActive(false);
		((Component)_packedAnchorTransform).gameObject.SetActive(true);
		_deployed = false;
		_raftStructure.AnchorsDeployed--;
		if (_raftStructure.AnchorsDeployed <= 0)
		{
			RaftStructure.StructureBuoyancy.Rigidbody.constraints = (RigidbodyConstraints)0;
			RaftStructure.StructureBuoyancy.EnableWind(enable: true);
			RaftStructure.StructureBuoyancy.ResetDampening();
		}
		UnityEvent onRetrieve = OnRetrieve;
		if (onRetrieve != null)
		{
			onRetrieve.Invoke();
		}
	}

	private bool CanDeploy()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Singleton<StrandedWorld>.Instance == (Object)null)
		{
			return true;
		}
		float height = Singleton<StrandedWorld>.Instance.GetHeight(transform.position);
		if (height < Ceto.Ocean.Instance.level && height >= -100f)
		{
			return true;
		}
		return false;
	}

	private Placement GetAnchorPlacement(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(position, Vector3.down, ref val, 100f, (1 << Layers.TERRAIN) | (1 << Layers.TERRAIN_OBJECTS)))
		{
			return new Placement
			{
				Position = val.point,
				Rotation = Quaternion.LookRotation(Vector3.forward, val.normal)
			};
		}
		return new Placement
		{
			Position = position - Vector3.up * 15f,
			Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up)
		};
	}

	public override void OnTransformSerialisationIdChanged()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.OnTransformSerialisationIdChanged();
		if (!MPExtensions.IsMaster(Game.Mode) && _deployed)
		{
			Deploy(GetAnchorPlacement(transform.position));
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Deployed", _deployed);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (data.GetField("Deployed").GetValue<bool>())
		{
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				Deploy(GetAnchorPlacement(transform.position));
			});
		}
	}
}
