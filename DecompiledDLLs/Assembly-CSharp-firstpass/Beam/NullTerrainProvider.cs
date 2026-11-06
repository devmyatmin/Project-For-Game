using UnityEngine;

namespace Beam;

public class NullTerrainProvider : NullObject<NullTerrainProvider>, ITerrainProvider
{
	public float GetHeight(Vector3 worldPosition)
	{
		Debug.LogError((object)"NullTerrainProvider::GetHeight:: Not implemented.");
		return 0f;
	}

	public Vector3 GetNormal(Vector3 worldPosition)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogError((object)"NullTerrainProvider::GetNormal:: Not implemented.");
		return Vector3.zero;
	}
}
