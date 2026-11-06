using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class FBIKBendGoal : MonoBehaviour
{
	public FullBodyBipedIK ik;

	public FullBodyBipedChain chain;

	public float weight;

	private void Start()
	{
		Debug.Log((object)"FBIKBendGoal is deprecated, you can now a bend goal from the custom inspector of the FullBodyBipedIK component.");
	}

	private void Update()
	{
		if (!((Object)(object)ik == (Object)null))
		{
			ik.solver.GetBendConstraint(chain).bendGoal = ((Component)this).transform;
			ik.solver.GetBendConstraint(chain).weight = weight;
		}
	}
}
