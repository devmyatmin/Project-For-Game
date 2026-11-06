using UnityEngine;

namespace Beam.Rendering;

public interface IInstancedMeshData
{
	Material[] GetMaterials();

	Matrix4x4[] GetMatrices();

	Mesh GetMesh();
}
