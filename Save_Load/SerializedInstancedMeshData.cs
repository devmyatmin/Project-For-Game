using UnityEngine;

namespace Beam.Rendering;

public struct SerializedInstancedMeshData : IInstancedMeshData
{
	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	private Material[] _materials;

	[SerializeField]
	private SerializedMatricesData _matricesData;

	public Mesh GetMesh()
	{
		return _mesh;
	}

	public Material[] GetMaterials()
	{
		return _materials;
	}

	public Matrix4x4[] GetMatrices()
	{
		return _matricesData.Matrices;
	}
}
