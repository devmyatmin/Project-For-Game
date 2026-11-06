using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class BendGoal : MonoBehaviour
{
	public LimbIK limbIK;

	[Range(0f, 1f)]
	public float weight = 1f;

	private void Start()
	{
		Debug.Log((object)"BendGoal is deprecated, you can now a bend goal from the custom inspector of the LimbIK component.");
	}

	private void LateUpdate()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)limbIK == (Object)null))
		{
			limbIK.solver.SetBendGoalPosition(((Component)this).transform.position, weight);
		}
	}
}
