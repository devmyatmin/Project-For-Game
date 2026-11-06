using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRIKPlatformController : MonoBehaviour
{
	public VRIK ik;

	public Transform trackingSpace;

	public Transform platform;

	public bool moveToPlatform = true;

	private Transform lastPlatform;

	private Vector3 lastPosition;

	private Quaternion lastRotation = Quaternion.identity;

	private void LateUpdate()
	{
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)platform != (Object)(object)lastPlatform)
		{
			if ((Object)(object)platform != (Object)null)
			{
				if (moveToPlatform)
				{
					lastPosition = ((Component)ik).transform.position;
					lastRotation = ((Component)ik).transform.rotation;
					((Component)ik).transform.position = platform.position;
					((Component)ik).transform.rotation = platform.rotation;
					trackingSpace.position = platform.position;
					trackingSpace.rotation = platform.rotation;
					ik.solver.AddPlatformMotion(platform.position - lastPosition, platform.rotation * Quaternion.Inverse(lastRotation), platform.position);
				}
				lastPosition = platform.position;
				lastRotation = platform.rotation;
			}
			((Component)ik).transform.parent = platform;
			trackingSpace.parent = platform;
			lastPlatform = platform;
		}
		if ((Object)(object)platform != (Object)null)
		{
			ik.solver.AddPlatformMotion(platform.position - lastPosition, platform.rotation * Quaternion.Inverse(lastRotation), platform.position);
			lastRotation = platform.rotation;
			lastPosition = platform.position;
		}
	}

	public VRIKPlatformController()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
	}
}
