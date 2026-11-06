using System;
using UnityEngine;

namespace Beam.Rendering;

[Serializable]
public struct InstancedMeshData(Mesh mesh, Material[] materials, Matrix4x4[] matrices) : IInstancedMeshData
{
	private readonly Mesh _mesh = mesh;

	private readonly Material[] _materials = materials;

	private readonly Matrix4x4[] _matrices = matrices;

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
		return _matrices;
	}
}
