using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class TwistRelaxer : MonoBehaviour
{
	public IK ik;

	[Tooltip("If using multiple solvers, add them in inverse hierarchical order - first forearm roll bone, then forearm bone and upper arm bone.")]
	public TwistSolver[] twistSolvers = new TwistSolver[0];

	public void Start()
	{
		if (twistSolvers.Length == 0)
		{
			Debug.LogError((object)"TwistRelaxer has no TwistSolvers. TwistRelaxer.cs was restructured for FIK v2.0 to support multiple relaxers on the same body part and TwistRelaxer components need to be set up again, sorry for the inconvenience!", (Object)(object)((Component)this).transform);
			return;
		}
		TwistSolver[] array = twistSolvers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate();
		}
		if ((Object)(object)ik != (Object)null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostUpdate));
		}
	}

	private void Update()
	{
		if ((Object)(object)ik != (Object)null && ik.fixTransforms)
		{
			TwistSolver[] array = twistSolvers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FixTransforms();
			}
		}
	}

	private void OnPostUpdate()
	{
		if ((Object)(object)ik != (Object)null)
		{
			TwistSolver[] array = twistSolvers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Relax();
			}
		}
	}

	private void LateUpdate()
	{
		if ((Object)(object)ik == (Object)null)
		{
			TwistSolver[] array = twistSolvers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Relax();
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostUpdate));
		}
	}
}
