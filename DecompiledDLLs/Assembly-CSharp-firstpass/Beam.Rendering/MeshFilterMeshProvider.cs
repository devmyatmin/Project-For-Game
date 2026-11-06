using UnityEngine;

namespace Beam.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class MeshFilterMeshProvider : MeshProviderBase
{
	private MeshFilter _meshFilter;

	protected MeshFilter MeshFilter => _meshFilter ?? (_meshFilter = ((Component)this).GetComponent<MeshFilter>());

	public override Mesh SharedMesh
	{
		get
		{
			return MeshFilter.sharedMesh;
		}
		set
		{
			MeshFilter.sharedMesh = value;
		}
	}
}
