using UnityEngine;

namespace Beam.UI;

public interface ITransformViewAdapter
{
	Transform Transform { get; }

	Vector3 Position { get; set; }

	Quaternion Rotation { get; set; }

	Vector3 LocalPosition { get; set; }

	Quaternion LocalRotation { get; set; }

	void SetParent(ITransformViewAdapter transform, bool worldPositionStays);
}
