using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beam;

public class FlockFishRenderer : FishRendererBase
{
	[SerializeField]
	private Color[] _colors;

	private Mesh _mesh;

	private Material _material;

	private List<Matrix4x4> _matrices;

	private bool _active = true;

	private bool _visible;

	private Bounds _bounds;

	private MaterialPropertyBlock _propertyBlock;

	public override Bounds Bounds
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bounds;
		}
	}

	protected MaterialPropertyBlock PropertyBlock
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected Obj, but got Unknown
			if (_propertyBlock == null)
			{
				_propertyBlock = new MaterialPropertyBlock();
			}
			return _propertyBlock;
		}
	}

	private void Start()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_colors != null && _colors.Length != 0)
		{
			PropertyBlock.SetColor("_Color", _colors[Random.Range(0, _colors.Length)]);
		}
	}

	public override void SetActive(bool active)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		_active = active;
		if (_active)
		{
			_bounds.center = ((Component)this).transform.position;
			_bounds.size = Vector3.one * 3f;
		}
	}

	public override void SetMesh(Mesh mesh)
	{
		_mesh = mesh;
	}

	public override void SetMaterial(Material material)
	{
		_material = material;
		_material.SetFloat("_DitherDistance", CullingDistance);
	}

	public override void SetMatrices(List<Matrix4x4> matrices)
	{
		_matrices = matrices;
	}

	public override bool IsVisible()
	{
		return _visible;
	}

	private void LateUpdate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_visible = Cameras.IsVisible(Bounds) && !Cameras.IsCulled(((Component)this).transform.position, CullingDistance);
		if (!((Object)(object)_mesh == (Object)null) && !((Object)(object)_material == (Object)null) && _matrices != null && _active && _visible)
		{
			DoBounds();
			DoRender();
		}
	}

	private void DoBounds()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		_bounds.size = Vector3.zero;
		for (int i = 0; i < _matrices.Count; i++)
		{
			Matrix4x4 val2 = _matrices[i];
			Vector3 val3 = Vector4.op_Implicit(val2.GetColumn(3));
			val += val3;
			_bounds.Encapsulate(val3);
		}
		val /= (float)_matrices.Count;
		_bounds.center = val;
	}

	private void DoRender()
	{
		Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, PropertyBlock, (ShadowCastingMode)(CastShadows ? 1 : 0), ReceiveShadows, ((Component)this).gameObject.layer);
	}
}
