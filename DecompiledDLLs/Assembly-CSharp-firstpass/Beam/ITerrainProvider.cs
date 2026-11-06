using UnityEngine;

namespace Beam;

public interface ITerrainProvider
{
	float GetHeight(Vector3 worldPosition);

	Vector3 GetNormal(Vector3 worldPosition);
}
