using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRIKPlatform : MonoBehaviour
{
	public VRIK ik;

	private Vector3 lastPosition;

	private Quaternion lastRotation = Quaternion.identity;

	private void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		lastPosition = ((Component)this).transform.position;
		lastRotation = ((Component)this).transform.rotation;
	}

	private void LateUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		ik.solver.AddPlatformMotion(((Component)this).transform.position - lastPosition, ((Component)this).transform.rotation * Quaternion.Inverse(lastRotation), ((Component)this).transform.position);
		lastRotation = ((Component)this).transform.rotation;
		lastPosition = ((Component)this).transform.position;
	}

	public VRIKPlatform()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
