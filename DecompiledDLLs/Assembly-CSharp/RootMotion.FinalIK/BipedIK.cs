using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page4.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Biped IK")]
public class BipedIK : SolverManager
{
	public BipedReferences references = new BipedReferences();

	public BipedIKSolvers solvers = new BipedIKSolvers();

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page4.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_biped_i_k.html");
	}

	[ContextMenu("Support Group")]
	private void SupportGroup()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	private void ASThread()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public float GetIKPositionWeight(AvatarIKGoal goal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetGoalIK(goal).GetIKPositionWeight();
	}

	public float GetIKRotationWeight(AvatarIKGoal goal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetGoalIK(goal).GetIKRotationWeight();
	}

	public void SetIKPositionWeight(AvatarIKGoal goal, float weight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetGoalIK(goal).SetIKPositionWeight(weight);
	}

	public void SetIKRotationWeight(AvatarIKGoal goal, float weight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetGoalIK(goal).SetIKRotationWeight(weight);
	}

	public void SetIKPosition(AvatarIKGoal goal, Vector3 IKPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetGoalIK(goal).SetIKPosition(IKPosition);
	}

	public void SetIKRotation(AvatarIKGoal goal, Quaternion IKRotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetGoalIK(goal).SetIKRotation(IKRotation);
	}

	public Vector3 GetIKPosition(AvatarIKGoal goal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetGoalIK(goal).GetIKPosition();
	}

	public Quaternion GetIKRotation(AvatarIKGoal goal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetGoalIK(goal).GetIKRotation();
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight, float clampWeightHead, float clampWeightEyes)
	{
		solvers.lookAt.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight, clampWeightHead, clampWeightEyes);
	}

	public void SetLookAtPosition(Vector3 lookAtPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		solvers.lookAt.SetIKPosition(lookAtPosition);
	}

	public void SetSpinePosition(Vector3 spinePosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		solvers.spine.SetIKPosition(spinePosition);
	}

	public void SetSpineWeight(float weight)
	{
		solvers.spine.SetIKPositionWeight(weight);
	}

	public IKSolverLimb GetGoalIK(AvatarIKGoal goal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)goal switch
		{
			0 => solvers.leftFoot, 
			1 => solvers.rightFoot, 
			2 => solvers.leftHand, 
			3 => solvers.rightHand, 
			_ => null, 
		};
	}

	public void InitiateBipedIK()
	{
		InitiateSolver();
	}

	public void UpdateBipedIK()
	{
		UpdateSolver();
	}

	public void SetToDefaults()
	{
		IKSolverLimb[] limbs = solvers.limbs;
		foreach (IKSolverLimb obj in limbs)
		{
			obj.SetIKPositionWeight(0f);
			obj.SetIKRotationWeight(0f);
			obj.bendModifier = IKSolverLimb.BendModifier.Animation;
			obj.bendModifierWeight = 1f;
		}
		solvers.leftHand.maintainRotationWeight = 0f;
		solvers.rightHand.maintainRotationWeight = 0f;
		solvers.spine.SetIKPositionWeight(0f);
		solvers.spine.tolerance = 0f;
		solvers.spine.maxIterations = 2;
		solvers.spine.useRotationLimits = false;
		solvers.aim.SetIKPositionWeight(0f);
		solvers.aim.tolerance = 0f;
		solvers.aim.maxIterations = 2;
		SetLookAtWeight(0f, 0.5f, 1f, 1f, 0.5f, 0.7f, 0.5f);
	}

	protected override void FixTransforms()
	{
		solvers.lookAt.FixTransforms();
		for (int i = 0; i < solvers.limbs.Length; i++)
		{
			solvers.limbs[i].FixTransforms();
		}
	}

	protected override void InitiateSolver()
	{
		string errorMessage = "";
		if (BipedReferences.SetupError(references, ref errorMessage))
		{
			Warning.Log(errorMessage, references.root);
			return;
		}
		solvers.AssignReferences(references);
		if (solvers.spine.bones.Length > 1)
		{
			solvers.spine.Initiate(((Component)this).transform);
		}
		solvers.lookAt.Initiate(((Component)this).transform);
		solvers.aim.Initiate(((Component)this).transform);
		IKSolverLimb[] limbs = solvers.limbs;
		for (int i = 0; i < limbs.Length; i++)
		{
			limbs[i].Initiate(((Component)this).transform);
		}
		solvers.pelvis.Initiate(references.pelvis);
	}

	protected override void UpdateSolver()
	{
		for (int i = 0; i < solvers.limbs.Length; i++)
		{
			solvers.limbs[i].MaintainBend();
			solvers.limbs[i].MaintainRotation();
		}
		solvers.pelvis.Update();
		if (solvers.spine.bones.Length > 1)
		{
			solvers.spine.Update();
		}
		solvers.aim.Update();
		solvers.lookAt.Update();
		for (int j = 0; j < solvers.limbs.Length; j++)
		{
			solvers.limbs[j].Update();
		}
	}

	public void LogWarning(string message)
	{
		Warning.Log(message, ((Component)this).transform);
	}
}
