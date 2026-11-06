using UnityEngine;

namespace Beam.Rendering;

public class SerializedMatricesData : ScriptableObject
{
	[SerializeField]
	private Matrix4x4[] _matrices;

	public Matrix4x4[] Matrices => _matrices;
}
