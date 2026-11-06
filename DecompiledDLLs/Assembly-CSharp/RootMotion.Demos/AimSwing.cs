using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class AimSwing : MonoBehaviour
{
	public AimIK ik;

	[Tooltip("The direction in which the weapon is aimed in animation (in character space). Tweak this value to adjust the aiming.")]
	public Vector3 animatedAimDirection = Vector3.forward;

	private void LateUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		ik.solver.axis = ik.solver.transform.InverseTransformVector(((Component)ik).transform.rotation * animatedAimDirection);
	}

	public AimSwing()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
