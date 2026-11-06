using UnityEngine;

namespace Ceto;

public interface IProjection
{
	bool IsDouble { get; }

	bool IsFlipped { get; }

	bool TightFit { get; set; }

	void UpdateProjection(Camera cam, CameraData data);
}
