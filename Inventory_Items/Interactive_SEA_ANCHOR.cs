using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beam.Crafting;
using Beam.Language;
using Beam.Serialization.Json;
using Beam.UI;
using Ceto;
using Funlabs;
using MEC;
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
			if (_referenceId.IsDefault())
			{
				_referenceId = new MiniGuid(_raft.Entity.NetworkId.PackedValue);
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
			if (base.gameObject != null)
			{
				return base.gameObject.activeInHierarchy;
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
		Singleton<BuoyancyManager>.Instance?.Add(this);
		_deployedAnchorTransform.gameObject.SetActive(value: true);
		_deployedAnchorTransform.position = base.transform.parent.position + Vector3.down * 1.1f + -Wind.Direction * 10f;
		_deployedAnchorTransform.rotation = Quaternion.LookRotation(Wind.Direction, Vector3.up);
		_deployedAnchorRope.gameObject.SetActive(value: true);
		_packedAnchorTransform.gameObject.SetActive(value: false);
		_raft.Buoyancy.EnableWind(enable: false);
		_raft.Buoyancy.SetDampening(0.7f);
		_deployedSequence = Timing.RunCoroutine(Deployed_Sequence());
		Deployed = true;
		OnDeploy?.Invoke();
	}

	public void Retrieve()
	{
		Singleton<BuoyancyManager>.Instance?.Remove(this);
		_deployedAnchorTransform.gameObject.SetActive(value: false);
		_deployedAnchorTransform.localPosition = Vector3.zero;
		_deployedAnchorRope.gameObject.SetActive(value: false);
		_packedAnchorTransform.gameObject.SetActive(value: true);
		_raft.Buoyancy.EnableWind(enable: true);
		_raft.Buoyancy.ResetDampening();
		Timing.KillCoroutines(_deployedSequence);
		Deployed = false;
		OnRetrieve?.Invoke();
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private bool CanDeploy()
	{
		if (Singleton<StrandedWorld>.Instance == null)
		{
			return true;
		}
		Vector3 vector = base.transform.parent.position + -Wind.Direction * 10f;
		if (Physics.CheckSphere(vector, _deployedAnchorCollider.radius, _deployedAnchorCollisionMask, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		if (Singleton<StrandedWorld>.Instance.GetHeight(vector) > -5f)
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

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IBase.get_transform()
	{
		return base.transform;
	}
}
