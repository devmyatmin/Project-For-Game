using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam.Crafting;

public class ConstructionObject_WATERBOTTLE : InteractiveObject_FOOD, ICraftable, IBase, IHasCraftingType, IPlaceable
{
	[SerializeField]
	private GhostCollider.ColliderType _colliderType;

	[SerializeField]
	private bool _craftImmediate;

	[SerializeField]
	private GameObject _ghostObject;

	private Mesh _ghostMesh;

	private Renderer _ghostRenderer;

	[SerializeField]
	private float _placingDistance = 4f;

	[FormerlySerializedAs("_onCrafted")]
	[SerializeField]
	[HideInInspector]
	private InteracterUnityEvent _crafted;

	public int CreatorId { get; set; }

	public GhostCollider.ColliderType ColliderType => _colliderType;

	public bool CraftImmediate => _craftImmediate;

	public Mesh GhostMesh => _ghostMesh;

	public Material[] GhostMaterials => _ghostRenderer.sharedMaterials;

	public float PlacingDistance => _placingDistance;

	protected override void Awake()
	{
		base.Awake();
		_ghostRenderer = _ghostObject.GetComponent<Renderer>();
		_ghostMesh = _ghostObject.GetComponent<MeshFilter>().sharedMesh;
	}

	public void OnBeginCrafting()
	{
	}

	public bool CheckProximity(Transform ghost)
	{
		return false;
	}

	public void Place()
	{
	}

	public bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return true;
	}

	public bool ReplicantPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return true;
	}

	public void OnCrafted(IPlayer player)
	{
		((UnityEvent<IPlayer>)_crafted)?.Invoke(player);
	}
}
