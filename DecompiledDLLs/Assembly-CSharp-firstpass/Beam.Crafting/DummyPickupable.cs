using System;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

namespace Beam.Crafting;

public class DummyPickupable : IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IDisposable
{
	private string _description;

	private InteractiveObjectClassification _classification;

	public uint PrefabId { get; set; }

	public MiniGuid ReferenceId { get; set; }

	public CraftingType CraftingType { get; set; }

	public GameObject gameObject => null;

	public Transform transform
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Transform TransformParent
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string DisplayName { get; private set; }

	public RaycastMode RaycastMode
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool CanParent
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool IsDamageable
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool CanPickUp
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool IsPickedUp
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Collider[] AllColliders
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Vector3 LocalHoldingPosition
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Vector3 LocalHoldingRotation
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int OwnerId
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int PreviousOwnerId
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public AnimationType Idle
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public AnimationType Primary
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public AnimationType PrimaryOnObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Action<DummyPickupable> OnConsumed { get; set; }

	bool ISaveablePrefab.IsValidPrefab => false;

	bool ISaveablePrefab.IsMultiplayerEntity => false;

	bool ISaveablePrefab.IsOwner => true;

	BoltEntity ISaveablePrefab.Entity
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	Collider[] ISaveablePrefab.AllColliders
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	Rigidbody[] ISaveablePrefab.AllRigidbodies
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public event Action<IPickupable> PickedUp;

	public void Damage(float damage, IBase damager)
	{
		throw new NotImplementedException();
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
		throw new NotImplementedException();
	}

	public void SetOwner(IPlayer player)
	{
		throw new NotImplementedException();
	}

	public void Hold(bool holding)
	{
		throw new NotImplementedException();
	}

	public void SetLOD(int level)
	{
		throw new NotImplementedException();
	}

	public bool ValidatePrimary(IBase obj)
	{
		throw new NotImplementedException();
	}

	string ICraftingDisplayableStorable.GetHealthTerm()
	{
		return string.Empty;
	}

	float ICraftingDisplayableStorable.GetHealthUnits()
	{
		return 1f;
	}

	float ICraftingDisplayableStorable.GetHealthRange()
	{
		return 1f;
	}

	InteractiveObjectClassification ICraftingDisplayableStorable.GetClassification()
	{
		return _classification;
	}

	string IDisplayableStorable.GetDisplayName()
	{
		return DisplayName;
	}

	string IDisplayableStorable.GetDescription()
	{
		return _description;
	}

	public bool Store()
	{
		return true;
	}

	public void Release()
	{
	}

	public JObject Save()
	{
		throw new NotImplementedException();
	}

	public void Load(JObject data)
	{
		throw new NotImplementedException();
	}

	int ISaveablePrefab.GetColliderId(Collider collider)
	{
		throw new NotImplementedException();
	}

	int ISaveablePrefab.GetRigidbodyId(Rigidbody rigidbody)
	{
		throw new NotImplementedException();
	}

	public DummyPickupable(uint prefabId)
		: this(GetTemplate(prefabId))
	{
	}

	public DummyPickupable(IPickupable template)
	{
		PrefabId = template.PrefabId;
		ReferenceId = MiniGuid.New();
		CraftingType = template.CraftingType;
		DisplayName = template.DisplayName;
		_description = template.GetDescription();
		_classification = template.GetClassification();
	}

	public void Consume()
	{
		OnConsumed(this);
	}

	public void Dispose()
	{
		PickedUp = null;
		OnConsumed = null;
	}

	public IPickupable GetInstance()
	{
		if (!(Prefabs.GetPrefab(((ISaveablePrefab)this).PrefabId) is IPickupable))
		{
			return null;
		}
		return MultiplayerMng.Instantiate<SaveablePrefab>(((ISaveablePrefab)this).PrefabId, MiniGuid.New()) as IPickupable;
	}

	public static IPickupable GetTemplate(uint prefabId)
	{
		return BeamTeam.GetInterface<IPickupable>((Component)(object)Prefabs.GetPrefab(prefabId));
	}
}
