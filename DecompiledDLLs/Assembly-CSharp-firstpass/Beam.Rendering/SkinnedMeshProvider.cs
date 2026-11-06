using UnityEngine;

namespace Beam.Rendering;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshProvider : MeshProviderBase
{
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	protected SkinnedMeshRenderer SkinnedMeshRenderer => _skinnedMeshRenderer ?? (_skinnedMeshRenderer = ((Component)this).GetComponent<SkinnedMeshRenderer>());

	public override Mesh SharedMesh
	{
		get
		{
			return SkinnedMeshRenderer.sharedMesh;
		}
		set
		{
			SkinnedMeshRenderer.sharedMesh = value;
		}
	}
}
