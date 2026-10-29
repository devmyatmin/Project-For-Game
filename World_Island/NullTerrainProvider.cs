using UnityEngine;

namespace Beam;

public class NullTerrainProvider : NullObject<NullTerrainProvider>, ITerrainProvider
{
	public float GetHeight(Vector3 worldPosition)
	{
		Debug.LogError("NullTerrainProvider::GetHeight:: Not implemented.");
		return 0f;
	}

	public Vector3 GetNormal(Vector3 worldPosition)
	{
		Debug.LogError("NullTerrainProvider::GetNormal:: Not implemented.");
		return Vector3.zero;
	}
}
