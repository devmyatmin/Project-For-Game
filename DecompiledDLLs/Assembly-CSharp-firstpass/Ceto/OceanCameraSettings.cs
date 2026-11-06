using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Camera/OceanCameraSettings")]
[RequireComponent(typeof(Camera))]
public class OceanCameraSettings : MonoBehaviour
{
	public bool disableAllOverlays;

	public OVERLAY_MAP_SIZE heightOverlaySize = OVERLAY_MAP_SIZE.HALF;

	public OVERLAY_MAP_SIZE normalOverlaySize = OVERLAY_MAP_SIZE.FULL;

	public OVERLAY_MAP_SIZE foamOverlaySize = OVERLAY_MAP_SIZE.FULL;

	public OVERLAY_MAP_SIZE clipOverlaySize = OVERLAY_MAP_SIZE.HALF;

	public bool disableReflections;

	public LayerMask reflectionMask = LayerMask.op_Implicit(1);

	public REFLECTION_RESOLUTION reflectionResolution = REFLECTION_RESOLUTION.HALF;

	public bool disableUnderwater;

	public LayerMask oceanDepthsMask = LayerMask.op_Implicit(1);

	public OceanCameraSettings()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
	}
}
