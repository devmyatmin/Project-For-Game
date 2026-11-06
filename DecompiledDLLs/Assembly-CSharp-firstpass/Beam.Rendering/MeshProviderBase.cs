using UnityEngine;

namespace Beam.Rendering;

public abstract class MeshProviderBase : MonoBehaviour
{
	public abstract Mesh SharedMesh { get; set; }
}
