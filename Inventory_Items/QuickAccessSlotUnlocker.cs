using System.Runtime.CompilerServices;
using Beam.Events;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class QuickAccessSlotUnlocker : BaseObject, ICraftable, IBase, IHasCraftingType, IPlaceable
{
	[SerializeField]
	private int _toolbeltLevel;

	[SerializeField]
	private string _nextCombinationName;

	[SerializeField]
	private InteracterUnityEvent _crafted;

	public int CreatorId { get; set; }

	public bool CraftImmediate => true;

	public float PlacingDistance { get; }

	public GhostCollider.ColliderType ColliderType { get; }

	public Mesh GhostMesh { get; }

	public Renderer GhostRenderer { get; }

	public Material[] GhostMaterials => GhostRenderer.sharedMaterials;

	protected override void Awake()
	{
		base.Awake();
		_crafted.AddListener(UnlockSlot);
	}

	private void UnlockSlot(IPlayer player)
	{
		EventManager.RaiseEvent(new QuickAccessSlotUnlockedEvent(CreatorId, _toolbeltLevel));
		if (!string.IsNullOrEmpty(_nextCombinationName))
		{
			player.Crafter.CraftingCombinations.UnlockCombination(_nextCombinationName);
		}
		BeamTeam.Destroy(base.gameObject);
	}

	public void Place()
	{
	}

	public bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return false;
	}

	public bool ReplicantPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return false;
	}

	public bool CheckProximity(Transform ghost)
	{
		return false;
	}

	public void OnBeginCrafting()
	{
	}

	public void OnCrafted(IPlayer player)
	{
		_crafted?.Invoke(player);
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
