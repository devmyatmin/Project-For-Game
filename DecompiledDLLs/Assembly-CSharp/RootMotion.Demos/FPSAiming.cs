using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class FPSAiming : MonoBehaviour
{
	[Range(0f, 1f)]
	public float aimWeight = 1f;

	[Range(0f, 1f)]
	public float sightWeight = 1f;

	[Range(0f, 180f)]
	public float maxAngle = 80f;

	public Vector3 aimOffset;

	public bool animatePhysics;

	public Transform gun;

	public Transform gunTarget;

	public FullBodyBipedIK ik;

	public AimIK gunAim;

	public AimIK headAim;

	public CameraControllerFPS cam;

	public Recoil recoil;

	[Range(0f, 1f)]
	public float cameraRecoilWeight = 0.5f;

	private Vector3 gunTargetDefaultLocalPosition;

	private Vector3 gunTargetDefaultLocalRotation;

	private Vector3 camDefaultLocalPosition;

	private Vector3 camRelativeToGunTarget;

	private bool updateFrame;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		gunTargetDefaultLocalPosition = gunTarget.localPosition;
		gunTargetDefaultLocalRotation = gunTarget.localEulerAngles;
		camDefaultLocalPosition = ((Component)cam).transform.localPosition;
		((Behaviour)cam).enabled = false;
		((Behaviour)gunAim).enabled = false;
		if ((Object)(object)headAim != (Object)null)
		{
			((Behaviour)headAim).enabled = false;
		}
		((Behaviour)ik).enabled = false;
		if ((Object)(object)recoil != (Object)null && ik.solver.iterations == 0)
		{
			Debug.LogWarning((object)"FPSAiming with Recoil needs FBBIK solver iteration count to be at least 1 to maintain accuracy.");
		}
	}

	private void FixedUpdate()
	{
		updateFrame = true;
	}

	private void LateUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!animatePhysics)
		{
			updateFrame = true;
		}
		if (updateFrame)
		{
			updateFrame = false;
			((Component)cam).transform.localPosition = camDefaultLocalPosition;
			camRelativeToGunTarget = gunTarget.InverseTransformPoint(((Component)cam).transform.position);
			cam.LateUpdate();
			RotateCharacter();
			Aiming();
			LookDownTheSight();
		}
	}

	private void Aiming()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!(aimWeight <= 0f))
		{
			Quaternion rotation = ((Component)cam).transform.rotation;
			if ((Object)(object)headAim != (Object)null)
			{
				headAim.solver.IKPosition = ((Component)cam).transform.position + ((Component)cam).transform.forward * 10f;
				headAim.solver.IKPositionWeight = 1f - aimWeight;
				headAim.solver.Update();
			}
			gunAim.solver.IKPosition = ((Component)cam).transform.position + ((Component)cam).transform.forward * 10f + ((Component)cam).transform.rotation * aimOffset;
			gunAim.solver.IKPositionWeight = aimWeight;
			gunAim.solver.Update();
			((Component)cam).transform.rotation = rotation;
		}
	}

	private void LookDownTheSight()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		float num = aimWeight * sightWeight;
		gunTarget.position = Vector3.Lerp(gun.position, gunTarget.parent.TransformPoint(gunTargetDefaultLocalPosition), num);
		gunTarget.rotation = Quaternion.Lerp(gun.rotation, gunTarget.parent.rotation * Quaternion.Euler(gunTargetDefaultLocalRotation), num);
		Vector3 val = gun.InverseTransformPoint(ik.solver.leftHandEffector.bone.position);
		Vector3 val2 = gun.InverseTransformPoint(ik.solver.rightHandEffector.bone.position);
		Quaternion val3 = Quaternion.Inverse(gun.rotation) * ik.solver.leftHandEffector.bone.rotation;
		Quaternion val4 = Quaternion.Inverse(gun.rotation) * ik.solver.rightHandEffector.bone.rotation;
		float num2 = 1f;
		IKEffector leftHandEffector = ik.solver.leftHandEffector;
		leftHandEffector.positionOffset += (gunTarget.TransformPoint(val) - (ik.solver.leftHandEffector.bone.position + ik.solver.leftHandEffector.positionOffset)) * num2;
		IKEffector rightHandEffector = ik.solver.rightHandEffector;
		rightHandEffector.positionOffset += (gunTarget.TransformPoint(val2) - (ik.solver.rightHandEffector.bone.position + ik.solver.rightHandEffector.positionOffset)) * num2;
		ik.solver.headMapping.maintainRotationWeight = 1f;
		if ((Object)(object)recoil != (Object)null)
		{
			recoil.SetHandRotations(gunTarget.rotation * val3, gunTarget.rotation * val4);
		}
		ik.solver.Update();
		if ((Object)(object)recoil != (Object)null)
		{
			ik.references.leftHand.rotation = recoil.rotationOffset * (gunTarget.rotation * val3);
			ik.references.rightHand.rotation = recoil.rotationOffset * (gunTarget.rotation * val4);
		}
		else
		{
			ik.references.leftHand.rotation = gunTarget.rotation * val3;
			ik.references.rightHand.rotation = gunTarget.rotation * val4;
		}
		((Component)cam).transform.position = Vector3.Lerp(((Component)cam).transform.position, Vector3.Lerp(gunTarget.TransformPoint(camRelativeToGunTarget), ((Component)gun).transform.TransformPoint(camRelativeToGunTarget), cameraRecoilWeight), num);
	}

	private void RotateCharacter()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (maxAngle >= 180f)
		{
			return;
		}
		if (maxAngle <= 0f)
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(new Vector3(((Component)cam).transform.forward.x, 0f, ((Component)cam).transform.forward.z));
			return;
		}
		Vector3 val = ((Component)this).transform.InverseTransformDirection(((Component)cam).transform.forward);
		float num = Mathf.Atan2(val.x, val.z) * 57.29578f;
		if (Mathf.Abs(num) > Mathf.Abs(maxAngle))
		{
			float num2 = num - maxAngle;
			if (num < 0f)
			{
				num2 = num + maxAngle;
			}
			((Component)this).transform.rotation = Quaternion.AngleAxis(num2, ((Component)this).transform.up) * ((Component)this).transform.rotation;
		}
	}
}
