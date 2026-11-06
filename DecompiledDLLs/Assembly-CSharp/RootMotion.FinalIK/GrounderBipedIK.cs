using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Biped")]
public class GrounderBipedIK : Grounder
{
	[Tooltip("The BipedIK componet.")]
	public BipedIK ik;

	[Tooltip("The amount of spine bending towards upward slopes.")]
	public float spineBend = 7f;

	[Tooltip("The interpolation speed of spine bending.")]
	public float spineSpeed = 3f;

	private Transform[] feet = new Transform[2];

	private Quaternion[] footRotations = new Quaternion[2];

	private Vector3 animatedPelvisLocalPosition;

	private Vector3 solvedPelvisLocalPosition;

	private Vector3 spineOffset;

	private float lastWeight;

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_biped_i_k.html");
	}

	public override void ResetPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		solver.Reset();
		spineOffset = Vector3.zero;
	}

	private bool IsReadyToInitiate()
	{
		if ((Object)(object)ik == (Object)null)
		{
			return false;
		}
		if (!ik.solvers.leftFoot.initiated)
		{
			return false;
		}
		if (!ik.solvers.rightFoot.initiated)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (!(weight <= 0f) && !initiated && IsReadyToInitiate())
		{
			Initiate();
		}
	}

	private void Initiate()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		feet = new Transform[2];
		footRotations = new Quaternion[2];
		feet[0] = ik.references.leftFoot;
		feet[1] = ik.references.rightFoot;
		footRotations[0] = Quaternion.identity;
		footRotations[1] = Quaternion.identity;
		IKSolverFABRIK spine = ik.solvers.spine;
		spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(spine.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
		IKSolverLimb rightFoot = ik.solvers.rightFoot;
		rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		animatedPelvisLocalPosition = ik.references.pelvis.localPosition;
		solver.Initiate(ik.references.root, feet);
		initiated = true;
	}

	private void OnDisable()
	{
		if (initiated)
		{
			ik.solvers.leftFoot.IKPositionWeight = 0f;
			ik.solvers.rightFoot.IKPositionWeight = 0f;
		}
	}

	private void OnSolverUpdate()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (weight <= 0f)
		{
			if (lastWeight <= 0f)
			{
				return;
			}
			OnDisable();
		}
		lastWeight = weight;
		if (OnPreGrounder != null)
		{
			OnPreGrounder();
		}
		if (ik.references.pelvis.localPosition != solvedPelvisLocalPosition)
		{
			animatedPelvisLocalPosition = ik.references.pelvis.localPosition;
		}
		else
		{
			ik.references.pelvis.localPosition = animatedPelvisLocalPosition;
		}
		solver.Update();
		Transform pelvis = ik.references.pelvis;
		pelvis.position += solver.pelvis.IKOffset * weight;
		SetLegIK(ik.solvers.leftFoot, 0);
		SetLegIK(ik.solvers.rightFoot, 1);
		if (spineBend != 0f && ik.references.spine.Length != 0)
		{
			spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
			Vector3 val = GetSpineOffsetTarget() * weight;
			spineOffset = Vector3.Lerp(spineOffset, val * spineBend, Time.deltaTime * spineSpeed);
			Quaternion rotation = ik.references.leftUpperArm.rotation;
			Quaternion rotation2 = ik.references.rightUpperArm.rotation;
			Vector3 up = solver.up;
			Quaternion val2 = Quaternion.FromToRotation(up, up + spineOffset);
			ik.references.spine[0].rotation = val2 * ik.references.spine[0].rotation;
			ik.references.leftUpperArm.rotation = rotation;
			ik.references.rightUpperArm.rotation = rotation2;
			ik.solvers.lookAt.SetDirty();
		}
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	private void SetLegIK(IKSolverLimb limb, int index)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		footRotations[index] = feet[index].rotation;
		limb.IKPosition = solver.legs[index].IKPosition;
		limb.IKPositionWeight = weight;
	}

	private void OnPostSolverUpdate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (!(weight <= 0f) && ((Behaviour)this).enabled)
		{
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * footRotations[i];
			}
			solvedPelvisLocalPosition = ik.references.pelvis.localPosition;
			if (OnPostIK != null)
			{
				OnPostIK();
			}
		}
	}

	private void OnDestroy()
	{
		if (initiated && (Object)(object)ik != (Object)null)
		{
			IKSolverFABRIK spine = ik.solvers.spine;
			spine.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(spine.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
			IKSolverLimb rightFoot = ik.solvers.rightFoot;
			rightFoot.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(rightFoot.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		}
	}
}
