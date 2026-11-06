using UnityEngine;

namespace Ceto;

public class ProjectionData : ViewData
{
	public bool checkedForFlipping;

	public Matrix4x4 projectorVP;

	public Matrix4x4 interpolation;
}
