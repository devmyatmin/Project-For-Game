using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public abstract class FishRendererBase : MonoBehaviour
{
	[SerializeField]
	private float _cullingDistance;

	[SerializeField]
	private bool _castShadows;

	[SerializeField]
	private bool _receiveShadows;

	protected float CullingDistance => _cullingDistance;

	protected bool CastShadows => _castShadows;

	protected bool ReceiveShadows => _receiveShadows;

	public abstract Bounds Bounds { get; }

	public abstract void SetMesh(Mesh mesh);

	public abstract void SetMaterial(Material material);

	public abstract void SetMatrices(List<Matrix4x4> matrices);

	public abstract void SetActive(bool active);

	public abstract bool IsVisible();
}
