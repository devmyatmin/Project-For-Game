using UnityEngine;

namespace Beam;

public interface IAttachingAnchor
{
	Vector3 GetPosition();

	Quaternion GetRotation();

	bool IsKinematic();
}
