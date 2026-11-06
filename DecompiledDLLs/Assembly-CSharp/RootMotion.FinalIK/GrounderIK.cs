using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder IK")]
public class GrounderIK : Grounder
{
	public IK[] legs;

	[Tooltip("The pelvis transform. Common ancestor of all the legs.")]
	public Transform pelvis;

	[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
	public Transform characterRoot;

	[Tooltip("The weight of rotating the character root to the ground normal (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float rootRotationWeight;

	[Tooltip("The speed of rotating the character root to the ground normal (range: 0 - inf).")]
	public float rootRotationSpeed = 5f;

	[Tooltip("The maximum angle of root rotation (range: 0 - 90).")]
	public float maxRootRotationAngle = 45f;

	private Transform[] feet = new Transform[0];

	private Quaternion[] footRotations = new Quaternion[0];

	private Vector3 animatedPelvisLocalPosition;

	private Vector3 solvedPelvisLocalPosition;

	private int solvedFeet;

	private bool solved;

	private float lastWeight;

	private Rigidbody characterRootRigidbody;

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_i_k.html");
	}

	public override void ResetPosition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].GetIKSolver().IKPosition = ((Component)feet[i]).transform.position;
			if (legs[i] is LimbIK)
			{
				(legs[i] as LimbIK).solver.IKRotation = solver.legs[i].transform.rotation;
			}
			footRotations[i] = feet[i].rotation;
		}
		animatedPelvisLocalPosition = pelvis.localPosition;
		solvedPelvisLocalPosition = pelvis.localPosition;
		solver.Reset();
	}

	private bool IsReadyToInitiate()
	{
		if ((Object)(object)pelvis == (Object)null)
		{
			return false;
		}
		if (legs.Length == 0)
		{
			return false;
		}
		IK[] array = legs;
		foreach (IK iK in array)
		{
			if ((Object)(object)iK == (Object)null)
			{
				return false;
			}
			if (iK is FullBodyBipedIK)
			{
				LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
				return false;
			}
			if (iK is FABRIKRoot)
			{
				LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
				return false;
			}
			if (iK is AimIK)
			{
				LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
				return false;
			}
		}
		return true;
	}

	private void OnDisable()
	{
		if (!initiated)
		{
			return;
		}
		for (int i = 0; i < legs.Length; i++)
		{
			if ((Object)(object)legs[i] != (Object)null)
			{
				legs[i].GetIKSolver().IKPositionWeight = 0f;
			}
		}
	}

	private void Update()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (weight <= 0f)
		{
			return;
		}
		solved = false;
		if (initiated)
		{
			rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
			rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
			if ((Object)(object)characterRoot != (Object)null && rootRotationSpeed > 0f && rootRotationWeight > 0f && solver.isGrounded)
			{
				Vector3 val = solver.GetLegsPlaneNormal();
				if (rootRotationWeight < 1f)
				{
					val = Vector3.Slerp(Vector3.up, val, rootRotationWeight);
				}
				Quaternion val2 = Quaternion.RotateTowards(Quaternion.FromToRotation(((Component)this).transform.up, Vector3.up) * characterRoot.rotation, Quaternion.FromToRotation(((Component)this).transform.up, val) * characterRoot.rotation, maxRootRotationAngle);
				if ((Object)(object)characterRootRigidbody == (Object)null)
				{
					characterRoot.rotation = Quaternion.Lerp(characterRoot.rotation, val2, Time.deltaTime * rootRotationSpeed);
				}
				else
				{
					characterRootRigidbody.MoveRotation(Quaternion.Lerp(characterRoot.rotation, val2, Time.deltaTime * rootRotationSpeed));
				}
			}
		}
		else if (IsReadyToInitiate())
		{
			Initiate();
		}
	}

	private void Initiate()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		feet = new Transform[legs.Length];
		footRotations = new Quaternion[legs.Length];
		for (int i = 0; i < feet.Length; i++)
		{
			footRotations[i] = Quaternion.identity;
		}
		for (int j = 0; j < legs.Length; j++)
		{
			IKSolver.Point[] points = legs[j].GetIKSolver().GetPoints();
			feet[j] = points[^1].transform;
			IKSolver iKSolver = legs[j].GetIKSolver();
			iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
			IKSolver iKSolver2 = legs[j].GetIKSolver();
			iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		}
		animatedPelvisLocalPosition = pelvis.localPosition;
		solver.Initiate(((Component)this).transform, feet);
		for (int k = 0; k < legs.Length; k++)
		{
			if (legs[k] is LegIK)
			{
				solver.legs[k].invertFootCenter = true;
			}
		}
		characterRootRigidbody = ((Component)characterRoot).GetComponent<Rigidbody>();
		initiated = true;
	}

	private void OnSolverUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
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
		if (!solved)
		{
			if (OnPreGrounder != null)
			{
				OnPreGrounder();
			}
			if (pelvis.localPosition != solvedPelvisLocalPosition)
			{
				animatedPelvisLocalPosition = pelvis.localPosition;
			}
			else
			{
				pelvis.localPosition = animatedPelvisLocalPosition;
			}
			solver.Update();
			for (int i = 0; i < legs.Length; i++)
			{
				SetLegIK(i);
			}
			Transform val = pelvis;
			val.position += solver.pelvis.IKOffset * weight;
			solved = true;
			solvedFeet = 0;
			if (OnPostGrounder != null)
			{
				OnPostGrounder();
			}
		}
	}

	private void SetLegIK(int index)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		footRotations[index] = feet[index].rotation;
		if (legs[index] is LegIK)
		{
			(legs[index].GetIKSolver() as IKSolverLeg).IKRotation = Quaternion.Slerp(Quaternion.identity, solver.legs[index].rotationOffset, weight) * footRotations[index];
			(legs[index].GetIKSolver() as IKSolverLeg).IKRotationWeight = 1f;
		}
		legs[index].GetIKSolver().IKPosition = solver.legs[index].IKPosition;
		legs[index].GetIKSolver().IKPositionWeight = weight;
	}

	private void OnPostSolverUpdate()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f || !((Behaviour)this).enabled)
		{
			return;
		}
		solvedFeet++;
		if (solvedFeet >= feet.Length)
		{
			solved = false;
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * footRotations[i];
			}
			solvedPelvisLocalPosition = pelvis.localPosition;
			if (OnPostIK != null)
			{
				OnPostIK();
			}
		}
	}

	private void OnDestroy()
	{
		if (!initiated)
		{
			return;
		}
		IK[] array = legs;
		foreach (IK iK in array)
		{
			if ((Object)(object)iK != (Object)null)
			{
				IKSolver iKSolver = iK.GetIKSolver();
				iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
				IKSolver iKSolver2 = iK.GetIKSolver();
				iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iKSolver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
			}
		}
	}
}
