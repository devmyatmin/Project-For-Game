using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class AimBoxing : MonoBehaviour
{
	public AimIK aimIK;

	public Transform pin;

	private void LateUpdate()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		aimIK.solver.transform.LookAt(pin.position);
		aimIK.solver.IKPosition = ((Component)this).transform.position;
	}
}
