using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class MechSpiderLeg : MonoBehaviour
{
	public MechSpider mechSpider;

	public MechSpiderLeg unSync;

	public Vector3 offset;

	public float minDelay = 0.2f;

	public float maxOffset = 1f;

	public float stepSpeed = 5f;

	public float footHeight = 0.15f;

	public float velocityPrediction = 0.2f;

	public float raycastFocus = 0.1f;

	public AnimationCurve yOffset;

	public Transform foot;

	public Vector3 footUpAxis;

	public float footRotationSpeed = 10f;

	public ParticleSystem sand;

	private IK ik;

	private float stepProgress = 1f;

	private float lastStepTime;

	private Vector3 defaultPosition;

	private RaycastHit hit;

	private Quaternion lastFootLocalRotation;

	private Vector3 smoothHitNormal = Vector3.up;

	private Vector3 lastStepPosition;

	public bool isStepping => stepProgress < 1f;

	public Vector3 position
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ik.GetIKSolver().GetIKPosition();
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ik.GetIKSolver().SetIKPosition(value);
		}
	}

	private void Awake()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		ik = ((Component)this).GetComponent<IK>();
		if ((Object)(object)foot != (Object)null)
		{
			if (footUpAxis == Vector3.zero)
			{
				footUpAxis = Quaternion.Inverse(foot.rotation) * Vector3.up;
			}
			lastFootLocalRotation = foot.localRotation;
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterIK));
		}
	}

	private void AfterIK()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)foot == (Object)null))
		{
			foot.localRotation = lastFootLocalRotation;
			smoothHitNormal = Vector3.Slerp(smoothHitNormal, hit.normal, Time.deltaTime * footRotationSpeed);
			Quaternion val = Quaternion.FromToRotation(foot.rotation * footUpAxis, smoothHitNormal);
			foot.rotation = val * foot.rotation;
		}
	}

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		stepProgress = 1f;
		hit = default;
		position = ik.GetIKSolver().GetPoints()[^1].transform.position;
		lastStepPosition = position;
		hit.point = position;
		defaultPosition = ((Component)mechSpider).transform.InverseTransformPoint(position + offset * mechSpider.scale);
		((MonoBehaviour)this).StartCoroutine(Step(position, position));
	}

	private Vector3 GetStepTarget(out bool stepFound, float focus, float distance)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		stepFound = false;
		Vector3 val = ((Component)mechSpider).transform.TransformPoint(defaultPosition) + mechSpider.velocity * velocityPrediction;
		Vector3 up = ((Component)mechSpider).transform.up;
		Vector3 val2 = mechSpider.body.position - position;
		Vector3 val3 = Vector3.Cross(up, val2);
		up = Quaternion.AngleAxis(focus, val3) * up;
		if (Physics.Raycast(val + up * mechSpider.raycastHeight * mechSpider.scale, -up, ref hit, mechSpider.raycastHeight * mechSpider.scale + distance, LayerMask.op_Implicit(mechSpider.raycastLayers)))
		{
			stepFound = true;
		}
		return hit.point + hit.normal * footHeight * mechSpider.scale;
	}

	private void UpdatePosition(float distance)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = ((Component)mechSpider).transform.up;
		if (Physics.Raycast(lastStepPosition + up * mechSpider.raycastHeight * mechSpider.scale, -up, ref hit, mechSpider.raycastHeight * mechSpider.scale + distance, LayerMask.op_Implicit(mechSpider.raycastLayers)))
		{
			position = hit.point + hit.normal * footHeight * mechSpider.scale;
		}
	}

	private void Update()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		UpdatePosition(mechSpider.raycastDistance * mechSpider.scale);
		if (!isStepping && !(Time.time < lastStepTime + minDelay) && (!((Object)(object)unSync != (Object)null) || !unSync.isStepping))
		{
			bool stepFound = false;
			Vector3 stepTarget = GetStepTarget(out stepFound, raycastFocus, mechSpider.raycastDistance * mechSpider.scale);
			if (!stepFound)
			{
				stepTarget = GetStepTarget(out stepFound, 0f - raycastFocus, mechSpider.raycastDistance * 3f * mechSpider.scale);
			}
			if (stepFound && !(Vector3.Distance(position, stepTarget) < maxOffset * mechSpider.scale * Random.Range(0.9f, 1.2f)))
			{
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(Step(position, stepTarget));
			}
		}
	}

	private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		stepProgress = 0f;
		while (stepProgress < 1f)
		{
			stepProgress += Time.deltaTime * stepSpeed;
			position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
			position += ((Component)mechSpider).transform.up * yOffset.Evaluate(stepProgress) * mechSpider.scale;
			lastStepPosition = position;
			yield return null;
		}
		position = targetPosition;
		lastStepPosition = position;
		if ((Object)(object)sand != (Object)null)
		{
			((Component)sand).transform.position = position - ((Component)mechSpider).transform.up * footHeight * mechSpider.scale;
			sand.Emit(20);
		}
		lastStepTime = Time.time;
	}

	public MechSpiderLeg()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
	}
}
