using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Full Body Biped")]
public class GrounderFBBIK : Grounder
{
	[Serializable]
	public class SpineEffector
	{
		[Tooltip("The type of the effector.")]
		public FullBodyBipedEffector effectorType;

		[Tooltip("The weight of horizontal bend offset towards the slope.")]
		public float horizontalWeight = 1f;

		[Tooltip("The vertical bend offset weight.")]
		public float verticalWeight;

		public SpineEffector()
		{
		}

		public SpineEffector(FullBodyBipedEffector effectorType, float horizontalWeight, float verticalWeight)
		{
			this.effectorType = effectorType;
			this.horizontalWeight = horizontalWeight;
			this.verticalWeight = verticalWeight;
		}
	}

	[Tooltip("Reference to the FBBIK componet.")]
	public FullBodyBipedIK ik;

	[Tooltip("The amount of spine bending towards upward slopes.")]
	public float spineBend = 2f;

	[Tooltip("The interpolation speed of spine bending.")]
	public float spineSpeed = 3f;

	public SpineEffector[] spine = new SpineEffector[0];

	private Transform[] feet = new Transform[2];

	private Vector3 spineOffset;

	private bool firstSolve;

	[ContextMenu("TUTORIAL VIDEO")]
	private void OpenTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_f_b_b_i_k.html");
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
		if (!ik.solver.initiated)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		firstSolve = true;
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (!(weight <= 0f) && !initiated && IsReadyToInitiate())
		{
			Initiate();
		}
	}

	private void FixedUpdate()
	{
		firstSolve = true;
	}

	private void LateUpdate()
	{
		firstSolve = true;
	}

	private void Initiate()
	{
		ik.solver.leftLegMapping.maintainRotationWeight = 1f;
		ik.solver.rightLegMapping.maintainRotationWeight = 1f;
		feet = new Transform[2];
		feet[0] = ik.solver.leftFootEffector.bone;
		feet[1] = ik.solver.rightFootEffector.bone;
		IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
		iKSolverFullBodyBiped.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolverFullBodyBiped.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
		IKSolverFullBodyBiped iKSolverFullBodyBiped2 = ik.solver;
		iKSolverFullBodyBiped2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolverFullBodyBiped2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		solver.Initiate(ik.references.root, feet);
		initiated = true;
	}

	private void OnSolverUpdate()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		if (!firstSolve)
		{
			return;
		}
		firstSolve = false;
		if (!((Behaviour)this).enabled || weight <= 0f)
		{
			return;
		}
		if (OnPreGrounder != null)
		{
			OnPreGrounder();
		}
		solver.Update();
		Transform pelvis = ik.references.pelvis;
		pelvis.position += solver.pelvis.IKOffset * weight;
		SetLegIK(ik.solver.leftFootEffector, solver.legs[0]);
		SetLegIK(ik.solver.rightFootEffector, solver.legs[1]);
		if (spineBend != 0f)
		{
			spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
			Vector3 val = GetSpineOffsetTarget() * weight;
			spineOffset = Vector3.Lerp(spineOffset, val * spineBend, Time.deltaTime * spineSpeed);
			Vector3 val2 = ik.references.root.up * spineOffset.magnitude;
			for (int i = 0; i < spine.Length; i++)
			{
				IKEffector effector = ik.solver.GetEffector(spine[i].effectorType);
				effector.positionOffset += spineOffset * spine[i].horizontalWeight + val2 * spine[i].verticalWeight;
			}
		}
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	private void SetLegIK(IKEffector effector, Grounding.Leg leg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		effector.positionOffset += (leg.IKPosition - effector.bone.position) * weight;
		effector.bone.rotation = Quaternion.Slerp(Quaternion.identity, leg.rotationOffset, weight) * effector.bone.rotation;
	}

	private void OnDrawGizmosSelected()
	{
		if ((Object)(object)ik == (Object)null)
		{
			ik = ((Component)this).GetComponent<FullBodyBipedIK>();
		}
		if ((Object)(object)ik == (Object)null)
		{
			ik = ((Component)this).GetComponentInParent<FullBodyBipedIK>();
		}
		if ((Object)(object)ik == (Object)null)
		{
			ik = ((Component)this).GetComponentInChildren<FullBodyBipedIK>();
		}
	}

	private void OnPostSolverUpdate()
	{
		if (OnPostIK != null)
		{
			OnPostIK();
		}
	}

	private void OnDestroy()
	{
		if (initiated && (Object)(object)ik != (Object)null)
		{
			IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
			iKSolverFullBodyBiped.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolverFullBodyBiped.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
			IKSolverFullBodyBiped iKSolverFullBodyBiped2 = ik.solver;
			iKSolverFullBodyBiped2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolverFullBodyBiped2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		}
	}
}
