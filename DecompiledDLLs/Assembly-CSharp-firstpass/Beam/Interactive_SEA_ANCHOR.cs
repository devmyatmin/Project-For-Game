using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.UI;
using Ceto;
using Funlabs;
using MEC;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Interactive_SEA_ANCHOR : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, ISelectable, IQueryWaves, ISaveable
{
	private const float DISTANCE_TO_DEPLOY = 10f;

	private const float MIN_OCEAN_DEPTH_TO_DEPLOY = -5f;

	private const float ANCHOR_DEPTH_BELOW_OCEAN = 1.1f;

	[SerializeField]
	private MiniGuid _referenceId;

	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _displayName;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[SerializeField]
	private Transform _packedAnchorTransform;

	[SerializeField]
	private Transform _deployedAnchorTransform;

	[SerializeField]
	private SphereCollider _deployedAnchorCollider;

	[SerializeField]
	private LayerMask _deployedAnchorCollisionMask;

	[SerializeField]
	private VerletLine _deployedAnchorRope;

	[SerializeField]
	private InteractiveObject_LIFERAFT _raft;

	public UnityEvent OnDeploy;

	public UnityEvent OnRetrieve;

	private List<WaveQuery> _waveQueries;

	private WaveQuery _anchorWaveQuery = new WaveQuery();

	private CoroutineHandle _deployedSequence;

	public override MiniGuid ReferenceId
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (_referenceId.IsDefault())
			{
				NetworkId networkId = _raft.Entity.NetworkId;
				_referenceId = new MiniGuid(networkId.PackedValue);
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public RaycastMode RaycastMode => RaycastMode.Collider;

	public string DisplayName => _displayName;

	public Selecting Selecting => _selecting;

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType { get; } = new CraftingType(AttributeType.None, InteractiveType.None);

	public bool CanInteract { get; set; } = true;

	public bool CanParent { get; set; }

	public bool IsDamageable { get; }

	public bool Deployed { get; private set; }

	bool IQueryWaves.Active
	{
		get
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null)
			{
				return ((Component)this).gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	IEnumerable<WaveQuery> IQueryWaves.WaveQueries
	{
		get
		{
			if (_waveQueries == null)
			{
				_waveQueries = new List<WaveQuery> { _anchorWaveQuery };
			}
			return _waveQueries;
		}
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!Deployed)
		{
			return "ITEM_INTERACTION_DESCRIPTION_DEPLOY";
		}
		return "ITEM_INTERACTION_DESCRIPTION_RETRIEVE";
	}

	void IQueryWaves.InRange()
	{
	}

	void IQueryWaves.OutOfRange()
	{
	}

	private void Awake()
	{
		_raft.VehicleMovement.Operated += RaftVehicleMovement_Operated;
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_deployedSequence);
		_raft.VehicleMovement.Operated -= RaftVehicleMovement_Operated;
		Singleton<BuoyancyManager>.Instance?.Remove(this);
	}

	private void RaftVehicleMovement_Operated(bool operated)
	{
		if (operated && Deployed)
		{
			Retrieve();
		}
	}

	public bool Interact(IPlayer player)
	{
		if (!Deployed)
		{
			if (!CanDeploy())
			{
				LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "SEA_ANCHOR_NTF_TOO_SHALLOW_DESC");
				return false;
			}
			Deploy();
			return true;
		}
		if (Deployed)
		{
			Retrieve();
			return true;
		}
		return false;
	}

	private void Deploy()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Singleton<BuoyancyManager>.Instance?.Add(this);
		((Component)_deployedAnchorTransform).gameObject.SetActive(true);
		_deployedAnchorTransform.position = ((Component)this).transform.parent.position + Vector3.down * 1.1f + -Wind.Direction * 10f;
		_deployedAnchorTransform.rotation = Quaternion.LookRotation(Wind.Direction, Vector3.up);
		((Component)_deployedAnchorRope).gameObject.SetActive(true);
		((Component)_packedAnchorTransform).gameObject.SetActive(false);
		_raft.Buoyancy.EnableWind(enable: false);
		_raft.Buoyancy.SetDampening(0.7f);
		_deployedSequence = Timing.RunCoroutine(Deployed_Sequence());
		Deployed = true;
		UnityEvent onDeploy = OnDeploy;
		if (onDeploy != null)
		{
			onDeploy.Invoke();
		}
	}

	public void Retrieve()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Singleton<BuoyancyManager>.Instance?.Remove(this);
		((Component)_deployedAnchorTransform).gameObject.SetActive(false);
		_deployedAnchorTransform.localPosition = Vector3.zero;
		((Component)_deployedAnchorRope).gameObject.SetActive(false);
		((Component)_packedAnchorTransform).gameObject.SetActive(true);
		_raft.Buoyancy.EnableWind(enable: true);
		_raft.Buoyancy.ResetDampening();
		Timing.KillCoroutines(_deployedSequence);
		Deployed = false;
		UnityEvent onRetrieve = OnRetrieve;
		if (onRetrieve != null)
		{
			onRetrieve.Invoke();
		}
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private bool CanDeploy()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Singleton<StrandedWorld>.Instance == (Object)null)
		{
			return true;
		}
		Vector3 val = ((Component)this).transform.parent.position + -Wind.Direction * 10f;
		if (Physics.CheckSphere(val, _deployedAnchorCollider.radius, LayerMask.op_Implicit(_deployedAnchorCollisionMask), (QueryTriggerInteraction)1))
		{
			return false;
		}
		if (Singleton<StrandedWorld>.Instance.GetHeight(val) > -5f)
		{
			return false;
		}
		return true;
	}

	private IEnumerator<float> Deployed_Sequence()
	{
		while (true)
		{
			_anchorWaveQuery.posX = _deployedAnchorTransform.position.x;
			_anchorWaveQuery.posZ = _deployedAnchorTransform.position.z;
			Vector3 position = _deployedAnchorTransform.position;
			position.y = _anchorWaveQuery.result.height - 1.1f;
			_deployedAnchorTransform.position = position;
			yield return 0f;
		}
	}

	public void Parent()
	{
	}

	public void Damage(float damage, IBase damager1)
	{
	}

	public void Destroy()
	{
	}

	public new JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Deployed", Deployed);
		return jObject;
	}

	public new void Load(JObject data)
	{
		if (data.GetField("Deployed").GetValue<bool>())
		{
			Deploy();
		}
	}
}
