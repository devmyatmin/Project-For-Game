using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class PendulumExample : MonoBehaviour
{
	[Tooltip("The master weight of this script.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("Multiplier for the distance of the root to the target.")]
	public float hangingDistanceMlp = 1.3f;

	[Tooltip("Where does the root of the character land when weight is blended out?")]
	[HideInInspector]
	public Vector3 rootTargetPosition;

	[Tooltip("How is the root of the character rotated when weight is blended out?")]
	[HideInInspector]
	public Quaternion rootTargetRotation;

	public Transform target;

	public Transform leftHandTarget;

	public Transform rightHandTarget;

	public Transform leftFootTarget;

	public Transform rightFootTarget;

	public Transform pelvisTarget;

	public Transform bodyTarget;

	public Transform headTarget;

	public Vector3 pelvisDownAxis = Vector3.right;

	private FullBodyBipedIK ik;

	private Quaternion rootRelativeToPelvis;

	private Vector3 pelvisToRoot;

	private float lastWeight;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		ik = ((Component)this).GetComponent<FullBodyBipedIK>();
		Quaternion rotation = target.rotation;
		target.rotation = leftHandTarget.rotation;
		((Joint)((Component)target).gameObject.AddComponent<FixedJoint>()).connectedBody = ((Component)leftHandTarget).GetComponent<Rigidbody>();
		((Component)target).GetComponent<Rigidbody>().MoveRotation(rotation);
		rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * ((Component)this).transform.rotation;
		pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (((Component)this).transform.position - ik.references.pelvis.position);
		rootTargetPosition = ((Component)this).transform.position;
		rootTargetRotation = ((Component)this).transform.rotation;
		lastWeight = weight;
	}

	private void LateUpdate()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		if (weight > 0f)
		{
			ik.solver.leftHandEffector.positionWeight = weight;
			ik.solver.leftHandEffector.rotationWeight = weight;
		}
		else
		{
			rootTargetPosition = ((Component)this).transform.position;
			rootTargetRotation = ((Component)this).transform.rotation;
			if (lastWeight > 0f)
			{
				ik.solver.leftHandEffector.positionWeight = 0f;
				ik.solver.leftHandEffector.rotationWeight = 0f;
			}
		}
		lastWeight = weight;
		if (!(weight <= 0f))
		{
			((Component)this).transform.position = Vector3.Lerp(rootTargetPosition, pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp, weight);
			((Component)this).transform.rotation = Quaternion.Lerp(rootTargetRotation, pelvisTarget.rotation * rootRelativeToPelvis, weight);
			ik.solver.leftHandEffector.position = leftHandTarget.position;
			ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
			Vector3 val = ik.references.pelvis.rotation * pelvisDownAxis;
			Quaternion val2 = Quaternion.FromToRotation(val, rightHandTarget.position - headTarget.position);
			ik.references.rightUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, val2, weight) * ik.references.rightUpperArm.rotation;
			Quaternion val3 = Quaternion.FromToRotation(val, leftFootTarget.position - bodyTarget.position);
			ik.references.leftThigh.rotation = Quaternion.Lerp(Quaternion.identity, val3, weight) * ik.references.leftThigh.rotation;
			Quaternion val4 = Quaternion.FromToRotation(val, rightFootTarget.position - bodyTarget.position);
			ik.references.rightThigh.rotation = Quaternion.Lerp(Quaternion.identity, val4, weight) * ik.references.rightThigh.rotation;
		}
	}

	public PendulumExample()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
