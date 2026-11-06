using System.Collections.Generic;
using Beam.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beam;

public class SingleFishRenderer : FishRendererBase, ITick
{
	[SerializeField]
	private MeshProviderBase _meshProvider;

	[SerializeField]
	private Renderer _meshRenderer;

	private bool _visible;

	bool ITick.Active => ((Behaviour)this).isActiveAndEnabled;

	public override Bounds Bounds
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _meshRenderer.bounds;
		}
	}

	private void Awake()
	{
		_meshRenderer.shadowCastingMode = (ShadowCastingMode)(CastShadows ? 1 : 0);
		Ticker.Instance.Add(this);
	}

	private void OnDestroy()
	{
		Ticker.Instance?.Remove(this);
	}

	public override void SetMesh(Mesh mesh)
	{
		_meshProvider.SharedMesh = mesh;
	}

	public override void SetMaterial(Material material)
	{
		_meshRenderer.sharedMaterial = material;
	}

	public override void SetMatrices(List<Matrix4x4> matrices)
	{
	}

	public override void SetActive(bool active)
	{
		_meshRenderer.enabled = active;
	}

	public override bool IsVisible()
	{
		return _visible;
	}

	void ITick.Tick()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_visible = _meshRenderer.isVisible && !Cameras.IsCulled(((Component)this).transform.position, CullingDistance);
	}
}
