using System;
using UnityEngine;

namespace Beam.Rendering;

[Serializable]
public class RenderableProxy : MonoBehaviour
{
	[SerializeField]
	private MeshFilter _meshFilter;

	[SerializeField]
	private Renderer _renderer;

	public MeshFilter MeshFilter => _meshFilter;

	public Renderer Renderer => _renderer;
}
