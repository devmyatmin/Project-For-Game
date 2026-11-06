using System;
using System.Collections.Generic;
using Beam.Ocean;
using Beam.UI;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Construction_RAFT : Construction_FOUNDATION, IDraggable, IPhysical, IBase, IHasCraftingType
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private RaftBuoyancy _raftBuoyancy;

	private Construction_STRUCTURE_RAFT _raftStructure;

	private static readonly HashSet<Constructing> _DfsVisitedNodes = new HashSet<Constructing>();

	private Construction_STRUCTURE_RAFT RaftStructure
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_raftStructure))
			{
				return _raftStructure = Structure as Construction_STRUCTURE_RAFT;
			}
			return _raftStructure;
		}
	}

	public bool CanDrag
	{
		get
		{
			return RaftStructure.CanDrag;
		}
		set
		{
			RaftStructure.CanDrag = value;
		}
	}

	public bool IsDragging
	{
		get
		{
			return RaftStructure.IsDragging;
		}
		set
		{
			RaftStructure.IsDragging = value;
		}
	}

	public float CurrentDrag
	{
		get
		{
			return RaftStructure.CurrentDrag;
		}
		set
		{
			RaftStructure.CurrentDrag = value;
		}
	}

	public float CurrentAngularDrag
	{
		get
		{
			return RaftStructure.CurrentAngularDrag;
		}
		set
		{
			RaftStructure.CurrentAngularDrag = value;
		}
	}

	Rigidbody IPhysical.rigidbody => RaftStructure.rigidbody;

	public bool IsKinematic => RaftStructure.IsKinematic;

	CollisionDetectionMode IPhysical.CollisionDetectionMode
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((IPhysical)RaftStructure).CollisionDetectionMode;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((IPhysical)RaftStructure).CollisionDetectionMode = value;
		}
	}

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

	public void EnablePhysics()
	{
	}

	public void DisablePhysics()
	{
	}

	protected override Placement CheckPlacement(Placement placement)
	{
		return placement;
	}

	public override void CheckStructure()
	{
		base.CheckStructure();
		if (MyConnector.Type == ConnectorType.RAFT)
		{
			RaftStructure.StructureBuoyancy.RaftBuoyancys.Add(_raftBuoyancy);
			RaftStructure.Recenter();
			RaftStructure.StructureBuoyancy.Resize();
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		switch (MyConnector.Type)
		{
		case ConnectorType.RAFT:
			DoRaftBaseDamage(damage, damager);
			break;
		case ConnectorType.RAFT_FLOOR:
			DoRaftFloorDamage(damage, damager);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private IPlayer GetPlayer(IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return null;
		}
		return PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
	}

	private void DoDamage(float damage, IBase damager)
	{
		base.Damage(damage, damager);
	}

	private void DoRaftFloorDamage(float damage, IBase damager)
	{
		IPlayer player = GetPlayer(damager);
		if (BeamTeam.IsNullOrDestroyed(player))
		{
			return;
		}
		bool flag = true;
		Connector[] myConnectors = MyConnectors;
		foreach (Connector connector in myConnectors)
		{
			if (connector.IsValid)
			{
				continue;
			}
			if (connector.Type == ConnectorType.VEHICLE_MAST || connector.Type == ConnectorType.LIGHT_HOOK || connector.Type == ConnectorType.CONTAINER_SHELF)
			{
				flag = false;
				break;
			}
			if (connector.Type != ConnectorType.VEHICLE_MOTOR)
			{
				continue;
			}
			foreach (Constructing construction in connector.Constructions)
			{
				if (construction.MyConnector.Type == ConnectorType.VEHICLE_MOTOR)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		if (!flag)
		{
			LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_SUPPORTS_BUILDINGS_DESC");
		}
		else
		{
			DoDamage(damage, damager);
		}
	}

	private void DoRaftBaseDamage(float damage, IBase damager)
	{
		IPlayer player = GetPlayer(damager);
		if (!BeamTeam.IsNullOrDestroyed(player))
		{
			if (HasRaftFloor() || !CanTraverseAllRaftBasesAfterDeleting(this))
			{
				LocalizedNotification.Post(player, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_SUPPORTS_BUILDINGS_DESC");
			}
			else
			{
				DoDamage(damage, damager);
			}
		}
	}

	private bool HasRaftFloor()
	{
		Connector[] myConnectors = MyConnectors;
		foreach (Connector connector in myConnectors)
		{
			if (!connector.IsValid && connector.Type == ConnectorType.RAFT_FLOOR)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanTraverseAllRaftBasesAfterDeleting(Constructing deleting)
	{
		Constructing constructing = FindOtherRaftBase(deleting);
		if (!Object.op_Implicit((Object)(object)constructing))
		{
			return true;
		}
		int num = CountRaftBasesInStructure();
		return CountRaftBasesConnected(constructing, deleting) == num;
	}

	private Constructing FindOtherRaftBase(Constructing exclude)
	{
		foreach (Constructing construction in Structure.Constructions)
		{
			if (construction.MyConnector.Type == ConnectorType.RAFT && !((Object)(object)exclude == (Object)(object)construction))
			{
				return construction;
			}
		}
		return null;
	}

	private int CountRaftBasesInStructure()
	{
		int num = 0;
		foreach (Constructing construction in Structure.Constructions)
		{
			if (construction.MyConnector.Type == ConnectorType.RAFT && !((Object)(object)construction == (Object)(object)this))
			{
				num++;
			}
		}
		return num;
	}

	private int CountRaftBasesConnected(Constructing rootNode, Constructing exclude)
	{
		DepthFirstTraversal(rootNode, (Constructing constructing) => (Object)(object)exclude == (Object)(object)constructing);
		int count = _DfsVisitedNodes.Count;
		_DfsVisitedNodes.Clear();
		return count;
	}

	private void DepthFirstTraversal(Constructing currentNode, Func<Constructing, bool> excludePredicate)
	{
		_DfsVisitedNodes.Add(currentNode);
		Connector[] myConnectors = currentNode.MyConnectors;
		foreach (Connector connector in myConnectors)
		{
			if (connector.Type == ConnectorType.RAFT)
			{
				Constructing constructing = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(connector.Constructions);
				if (Object.op_Implicit((Object)(object)constructing) && !excludePredicate(constructing) && !_DfsVisitedNodes.Contains(constructing))
				{
					DepthFirstTraversal(constructing, excludePredicate);
				}
			}
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if (MyConnector.Type == ConnectorType.RAFT && Structure.Constructions.Count > 0)
		{
			RaftStructure.StructureBuoyancy.RaftBuoyancys.Remove(_raftBuoyancy);
			RaftStructure.Recenter();
			RaftStructure.StructureBuoyancy.Resize();
		}
	}
}
