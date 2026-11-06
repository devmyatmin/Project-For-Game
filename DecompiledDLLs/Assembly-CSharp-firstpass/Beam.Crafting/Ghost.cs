using System.Collections.Generic;
using Beam.Rendering;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Ghost : MonoBehaviour
{
	[SerializeField]
	private Material _material;

	[SerializeField]
	private InstancingData _instancingData;

	[SerializeField]
	private InstancedMeshRenderer _renderer;

	[SerializeField]
	private BoxCollider _universalCollider;

	[SerializeField]
	private SphereCollider _playerProximityCollider;

	private Dictionary<GhostCollider.ColliderType, GhostCollider> _ghostColliders = new Dictionary<GhostCollider.ColliderType, GhostCollider>();

	private GhostCollider _activeCollider;

	private Material[] _materials;

	public bool Colliding => _activeCollider?.Colliding ?? false;

	private void Awake()
	{
		_renderer.IsStatic = false;
		GhostCollider[] componentsInChildren = ((Component)this).GetComponentsInChildren<GhostCollider>(true);
		foreach (GhostCollider ghostCollider in componentsInChildren)
		{
			if (ghostCollider.Colliders != null)
			{
				Collider[] colliders = ghostCollider.Colliders;
				foreach (Collider val in colliders)
				{
					Physics.IgnoreCollision((Collider)(object)_playerProximityCollider, val);
				}
			}
			_ghostColliders.Add(ghostCollider.Type, ghostCollider);
		}
	}

	public void Enable()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Disable()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		CleanupGhostMaterials();
	}

	public bool Initialize(ICraftable craftable)
	{
		DisableAllColliders();
		InitializeGhostProxy(craftable);
		InitializeCollider(craftable);
		return true;
	}

	public void SetHighlightColor(Color color)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		_renderer.SetMaterialsColor("_HighlightColor", color.linear);
		_renderer.SetMaterialsColor("_HighlightRimColor", color.linear);
	}

	private void InitializeGhostProxy(ICraftable craftable)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (craftable is ICraftingMaterialProvider craftingMaterialProvider)
		{
			IInstancedMeshData instancingData = _instancingData.GetInstancingData(craftingMaterialProvider.MaterialsType);
			Material[] materials = instancingData.GetMaterials();
			Material[] materials2 = CreateGhostMaterials(materials);
			_renderer.SetData(new InstancedMeshData(instancingData.GetMesh(), materials2, instancingData.GetMatrices()));
		}
		else
		{
			Material[] ghostMaterials = craftable.GhostMaterials;
			Material[] materials3 = CreateGhostMaterials(ghostMaterials);
			_renderer.SetData(new InstancedMeshData(craftable.GhostMesh, materials3, new Matrix4x4[1] { Matrix4x4.identity }));
		}
		_renderer.DrawCount = _renderer.Data.GetMatrices().Length;
	}

	private void CleanupGhostMaterials()
	{
		if (_materials != null && _materials.Length != 0)
		{
			for (int i = 0; i < _materials.Length; i++)
			{
				BeamTeam.Destroy<Material>(_materials[i]);
			}
			_materials = null;
		}
	}

	private Material[] CreateGhostMaterials(Material[] materials)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected Obj, but got Unknown
		CleanupGhostMaterials();
		_materials = new Material[materials.Length];
		for (int i = 0; i < materials.Length; i++)
		{
			_materials[i] = new Material(_material);
			Texture texture = materials[i].GetTexture("_MainTex");
			Texture texture2 = materials[i].GetTexture("_BumpMap");
			if ((Object)(object)texture != (Object)null)
			{
				_materials[i].SetTexture("_MainTex", texture);
			}
			if ((Object)(object)texture2 != (Object)null)
			{
				_materials[i].SetTexture("_BumpTex", texture2);
			}
		}
		return _materials;
	}

	private void InitializeCollider(ICraftable craftable)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_ghostColliders.TryGetValue(craftable.ColliderType, out _activeCollider);
		if (craftable.ColliderType == GhostCollider.ColliderType.NONE)
		{
			Bounds bounds = _renderer.GetBounds();
			BoxCollider component = ((Component)_activeCollider).GetComponent<BoxCollider>();
			component.center = bounds.center;
			component.size = bounds.size;
		}
		((Component)_activeCollider).gameObject.tag = craftable.gameObject.tag;
		((Component)_activeCollider).gameObject.SetActive(true);
	}

	public void IgnoreCollider(Collider collider, bool ignore)
	{
		_activeCollider.IgnoreCollider(collider, ignore);
	}

	private void DisableAllColliders()
	{
		((Component)_universalCollider).gameObject.SetActive(false);
		foreach (KeyValuePair<GhostCollider.ColliderType, GhostCollider> ghostCollider in _ghostColliders)
		{
			((Component)ghostCollider.Value).gameObject.SetActive(false);
		}
	}
}
