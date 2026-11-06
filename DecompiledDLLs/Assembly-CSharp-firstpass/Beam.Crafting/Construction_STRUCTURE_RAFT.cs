using System;
using System.Linq;
using Beam.Ocean;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Beam.Crafting;

[RequireComponent(typeof(Rigidbody))]
public class Construction_STRUCTURE_RAFT : Construction_STRUCTURE, IDraggable, IPhysical, IBase, IHasCraftingType, ISelectable, IMultiplayerState<IConstruction_STRUCTURE_RAFTState>
{
	[SerializeField]
	private StructureBuoyancy _structureBuoyancy;

	[SerializeField]
	private bool _nonKinematic;

	[SerializeField]
	private CollisionDetectionMode _collisionDetectionMode;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	private int _anchorsDeployed;

	public StructureBuoyancy StructureBuoyancy => _structureBuoyancy;

	public bool CanDrag { get; set; } = true;

	public bool IsPlatform { get; set; }

	public bool IsDragging { get; set; }

	public float CurrentDrag { get; set; }

	public float CurrentAngularDrag { get; set; }

	public bool IsKinematic => !_nonKinematic;

	CollisionDetectionMode IPhysical.CollisionDetectionMode
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _collisionDetectionMode;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_collisionDetectionMode = value;
		}
	}

	public Selecting Selecting => _selecting;

	public int AnchorsDeployed
	{
		get
		{
			return _anchorsDeployed;
		}
		set
		{
			_anchorsDeployed = value;
			CanDrag = _anchorsDeployed <= 0;
		}
	}

	public IConstruction_STRUCTURE_RAFTState MultiplayerState => ((MultiplayerBehaviour)this).GetMultiplayerState<IConstruction_STRUCTURE_RAFTState>();

	public event Action<IDraggable> BeginDragged;

	public event Action<IDraggable> EndDragged;

	public virtual void BeginDrag()
	{
		BeginDragged?.Invoke(this);
	}

	public virtual void EndDrag()
	{
		EndDragged?.Invoke(this);
	}

	protected override void Awake()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		CurrentDrag = rigidbody.drag;
		CurrentAngularDrag = rigidbody.angularDrag;
		this.InitializeKinematic(IsKinematic, MPExtensions.IsMaster(Game.Mode));
	}

	public override void Attached()
	{
		base.Attached();
		this.InitializeKinematic(IsKinematic, IsOwner);
	}

	public override void Damage(float damage, IBase damager)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
		if (BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			return;
		}
		RaycastHit lastHitInfo = playerFromId.GameInput.Raycaster.LastHitInfo;
		Construction_RAFT component = ((Component)lastHitInfo.collider).GetComponent<Construction_RAFT>();
		if ((Object)(object)component != (Object)null)
		{
			component.Damage(damage, damager);
			if (_structureBuoyancy.RaftBuoyancys.Count == 0)
			{
				Destroy();
			}
		}
	}

	public override void Recenter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < StructureBuoyancy.RaftBuoyancys.Count; i++)
		{
			val += ((Component)StructureBuoyancy.RaftBuoyancys[i]).transform.position;
		}
		for (int j = 0; j < Constructions.Count; j++)
		{
			Constructions[j].transform.parent = null;
		}
		if (StructureBuoyancy.RaftBuoyancys.Count != 0)
		{
			val /= (float)StructureBuoyancy.RaftBuoyancys.Count;
			transform.position = val;
		}
		for (int k = 0; k < Constructions.Count; k++)
		{
			Constructing constructing = Constructions[k];
			constructing.transform.parent = transform;
			constructing.transform.localScale = Vector3.one;
		}
	}

	public void EnablePhysics()
	{
	}

	public void DisablePhysics()
	{
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)rigidbody);
		jObject.AddField("Rigidbody", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		LevelLoader.CurrentLoader.PhysicalLoading.Add(() =>
		{
			this.LoadRigidbody(data);
		});
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			StructureBuoyancy.RaftBuoyancys = ((Component)this).GetComponentsInChildren<RaftBuoyancy>().ToList();
			StructureBuoyancy.Resize();
		});
	}
}
