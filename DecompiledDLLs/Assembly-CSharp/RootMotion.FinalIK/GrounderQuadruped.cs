using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page9.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Quadruped")]
public class GrounderQuadruped : Grounder
{
	public struct Foot(IKSolver solver, Transform transform)
	{
		public IKSolver solver = solver;

		public Transform transform = transform;

		public Quaternion rotation = transform.rotation;

		public Grounding.Leg leg = null;
	}

	[Tooltip("The Grounding solver for the forelegs.")]
	public Grounding forelegSolver = new Grounding();

	[Tooltip("The weight of rotating the character root to the ground angle (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float rootRotationWeight = 0.5f;

	[Tooltip("The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).")]
	[Range(-90f, 0f)]
	public float minRootRotation = -25f;

	[Tooltip("The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).")]
	[Range(0f, 90f)]
	public float maxRootRotation = 45f;

	[Tooltip("The speed of interpolating the character root rotation (range: 0 - inf).")]
	public float rootRotationSpeed = 5f;

	[Tooltip("The maximum IK offset for the legs (range: 0 - inf).")]
	public float maxLegOffset = 0.5f;

	[Tooltip("The maximum IK offset for the forelegs (range: 0 - inf).")]
	public float maxForeLegOffset = 0.5f;

	[Tooltip("The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float maintainHeadRotationWeight = 0.5f;

	[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
	public Transform characterRoot;

	[Tooltip("The pelvis transform. Common ancestor of both legs and the spine.")]
	public Transform pelvis;

	[Tooltip("The last bone in the spine that is the common parent for both forelegs.")]
	public Transform lastSpineBone;

	[Tooltip("The head (optional, if you intend to maintain its rotation).")]
	public Transform head;

	public IK[] legs;

	public IK[] forelegs;

	[HideInInspector]
	public Vector3 gravity = Vector3.down;

	private Foot[] feet = new Foot[0];

	private Vector3 animatedPelvisLocalPosition;

	private Quaternion animatedPelvisLocalRotation;

	private Quaternion animatedHeadLocalRotation;

	private Vector3 solvedPelvisLocalPosition;

	private Quaternion solvedPelvisLocalRotation;

	private Quaternion solvedHeadLocalRotation;

	private int solvedFeet;

	private bool solved;

	private float angle;

	private Transform forefeetRoot;

	private Quaternion headRotation;

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
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_quadruped.html");
	}

	public override void ResetPosition()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].GetIKSolver().IKPosition = feet[i].transform.position;
			if (legs[i] is LimbIK)
			{
				(legs[i] as LimbIK).solver.IKRotation = solver.legs[i].transform.rotation;
			}
		}
		solver.Reset();
		forelegSolver.Reset();
	}

	private bool IsReadyToInitiate()
	{
		if ((Object)(object)pelvis == (Object)null)
		{
			return false;
		}
		if ((Object)(object)lastSpineBone == (Object)null)
		{
			return false;
		}
		if (legs.Length == 0)
		{
			return false;
		}
		if (forelegs.Length == 0)
		{
			return false;
		}
		if ((Object)(object)characterRoot == (Object)null)
		{
			return false;
		}
		if (!IsReadyToInitiateLegs(legs))
		{
			return false;
		}
		if (!IsReadyToInitiateLegs(forelegs))
		{
			return false;
		}
		return true;
	}

	private bool IsReadyToInitiateLegs(IK[] ikComponents)
	{
		foreach (IK iK in ikComponents)
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
		for (int i = 0; i < feet.Length; i++)
		{
			if (feet[i].solver != null)
			{
				feet[i].solver.IKPositionWeight = 0f;
			}
		}
	}

	private void Update()
	{
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (!(weight <= 0f))
		{
			solved = false;
			if (!initiated && IsReadyToInitiate())
			{
				Initiate();
			}
		}
	}

	private void Initiate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		feet = new Foot[legs.Length + forelegs.Length];
		Transform[] array = InitiateFeet(legs, ref feet, 0);
		Transform[] array2 = InitiateFeet(forelegs, ref feet, legs.Length);
		animatedPelvisLocalPosition = pelvis.localPosition;
		animatedPelvisLocalRotation = pelvis.localRotation;
		if ((Object)(object)head != (Object)null)
		{
			animatedHeadLocalRotation = head.localRotation;
		}
		forefeetRoot = new GameObject().transform;
		forefeetRoot.parent = ((Component)this).transform;
		((Object)forefeetRoot).name = "Forefeet Root";
		solver.Initiate(((Component)this).transform, array);
		forelegSolver.Initiate(forefeetRoot, array2);
		for (int i = 0; i < array.Length; i++)
		{
			feet[i].leg = solver.legs[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			feet[j + legs.Length].leg = forelegSolver.legs[j];
		}
		characterRootRigidbody = ((Component)characterRoot).GetComponent<Rigidbody>();
		initiated = true;
	}

	private Transform[] InitiateFeet(IK[] ikComponents, ref Foot[] f, int indexOffset)
	{
		Transform[] array = new Transform[ikComponents.Length];
		for (int i = 0; i < ikComponents.Length; i++)
		{
			IKSolver.Point[] points = ikComponents[i].GetIKSolver().GetPoints();
			f[i + indexOffset] = new Foot(ikComponents[i].GetIKSolver(), points[^1].transform);
			array[i] = f[i + indexOffset].transform;
			IKSolver iKSolver = f[i + indexOffset].solver;
			iKSolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPreUpdate, new IKSolver.UpdateDelegate(OnSolverUpdate));
			IKSolver iKSolver2 = f[i + indexOffset].solver;
			iKSolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostSolverUpdate));
		}
		return array;
	}

	private void LateUpdate()
	{
		if (!(weight <= 0f))
		{
			rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
			minRootRotation = Mathf.Clamp(minRootRotation, -90f, maxRootRotation);
			maxRootRotation = Mathf.Clamp(maxRootRotation, minRootRotation, 90f);
			rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
			maxLegOffset = Mathf.Clamp(maxLegOffset, 0f, maxLegOffset);
			maxForeLegOffset = Mathf.Clamp(maxForeLegOffset, 0f, maxForeLegOffset);
			maintainHeadRotationWeight = Mathf.Clamp(maintainHeadRotationWeight, 0f, 1f);
			RootRotation();
		}
	}

	private void RootRotation()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (!(rootRotationWeight <= 0f) && !(rootRotationSpeed <= 0f))
		{
			solver.rotateSolver = true;
			forelegSolver.rotateSolver = true;
			Vector3 forward = characterRoot.forward;
			Vector3 val = -gravity;
			Vector3.OrthoNormalize(ref val, ref forward);
			Quaternion val2 = Quaternion.LookRotation(forward, -gravity);
			RaycastHit rootHit = forelegSolver.rootHit;
			Vector3 point = rootHit.point;
			rootHit = solver.rootHit;
			Vector3 val3 = point - rootHit.point;
			Vector3 val4 = Quaternion.Inverse(val2) * val3;
			float num = Mathf.Atan2(val4.y, val4.z) * 57.29578f;
			num = Mathf.Clamp(num * rootRotationWeight, minRootRotation, maxRootRotation);
			angle = Mathf.Lerp(angle, num, Time.deltaTime * rootRotationSpeed);
			if ((Object)(object)characterRootRigidbody == (Object)null)
			{
				characterRoot.rotation = Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(0f - angle, characterRoot.right) * val2, weight);
			}
			else
			{
				characterRootRigidbody.MoveRotation(Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(0f - angle, characterRoot.right) * val2, weight));
			}
		}
	}

	private void OnSolverUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
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
		if (solved)
		{
			return;
		}
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
		if (pelvis.localRotation != solvedPelvisLocalRotation)
		{
			animatedPelvisLocalRotation = pelvis.localRotation;
		}
		else
		{
			pelvis.localRotation = animatedPelvisLocalRotation;
		}
		if ((Object)(object)head != (Object)null)
		{
			if (head.localRotation != solvedHeadLocalRotation)
			{
				animatedHeadLocalRotation = head.localRotation;
			}
			else
			{
				head.localRotation = animatedHeadLocalRotation;
			}
		}
		for (int i = 0; i < feet.Length; i++)
		{
			feet[i].rotation = feet[i].transform.rotation;
		}
		if ((Object)(object)head != (Object)null)
		{
			headRotation = head.rotation;
		}
		UpdateForefeetRoot();
		solver.Update();
		forelegSolver.Update();
		Transform val = pelvis;
		val.position += solver.pelvis.IKOffset * weight;
		Vector3 val2 = lastSpineBone.position - pelvis.position;
		Vector3 val3 = lastSpineBone.position + forelegSolver.root.up * Mathf.Clamp(forelegSolver.pelvis.heightOffset, float.NegativeInfinity, 0f) - solver.root.up * solver.pelvis.heightOffset - pelvis.position;
		Quaternion val4 = Quaternion.FromToRotation(val2, val3);
		pelvis.rotation = Quaternion.Slerp(Quaternion.identity, val4, weight) * pelvis.rotation;
		for (int j = 0; j < feet.Length; j++)
		{
			SetFootIK(feet[j], (j < 2) ? maxLegOffset : maxForeLegOffset);
		}
		solved = true;
		solvedFeet = 0;
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	private void UpdateForefeetRoot()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < forelegSolver.legs.Length; i++)
		{
			val += forelegSolver.legs[i].transform.position;
		}
		val /= (float)forelegs.Length;
		Vector3 val2 = val - ((Component)this).transform.position;
		Vector3 up = ((Component)this).transform.up;
		Vector3 val3 = val2;
		Vector3.OrthoNormalize(ref up, ref val3);
		forefeetRoot.position = ((Component)this).transform.position + val3.normalized * val2.magnitude;
	}

	private void SetFootIK(Foot foot, float maxOffset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = foot.leg.IKPosition - foot.transform.position;
		foot.solver.IKPosition = foot.transform.position + Vector3.ClampMagnitude(val, maxOffset);
		foot.solver.IKPositionWeight = weight;
	}

	private void OnPostSolverUpdate()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f || !((Behaviour)this).enabled)
		{
			return;
		}
		solvedFeet++;
		if (solvedFeet >= feet.Length)
		{
			for (int i = 0; i < feet.Length; i++)
			{
				feet[i].transform.rotation = Quaternion.Slerp(Quaternion.identity, feet[i].leg.rotationOffset, weight) * feet[i].rotation;
			}
			if ((Object)(object)head != (Object)null)
			{
				head.rotation = Quaternion.Lerp(head.rotation, headRotation, maintainHeadRotationWeight * weight);
			}
			solvedPelvisLocalPosition = pelvis.localPosition;
			solvedPelvisLocalRotation = pelvis.localRotation;
			if ((Object)(object)head != (Object)null)
			{
				solvedHeadLocalRotation = head.localRotation;
			}
			if (OnPostIK != null)
			{
				OnPostIK();
			}
		}
	}

	private void OnDestroy()
	{
		if (initiated)
		{
			DestroyLegs(legs);
			DestroyLegs(forelegs);
		}
	}

	private void DestroyLegs(IK[] ikComponents)
	{
		foreach (IK iK in ikComponents)
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

	public GrounderQuadruped()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
	}
}
