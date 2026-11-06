using UnityEngine;

namespace kode80.Clouds;

public class RenderSettings : ScriptableObject
{
	[Header("Sun")]
	public Vector3 sunDirection;

	public Color sunColor;

	[Header("Lighting")]
	public Color cloudBaseColor;

	public Color cloudTopColor;
}
