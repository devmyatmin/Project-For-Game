using UnityEngine;

namespace RootMotion.Demos;

public abstract class CharacterBase : MonoBehaviour
{
	[Header("Base Parameters")]
	[Tooltip("If specified, will use the direction from the character to this Transform as the gravity vector instead of Physics.gravity. Physics.gravity.magnitude will be used as the magnitude of the gravity vector.")]
	public Transform gravityTarget;

	[Tooltip("Multiplies gravity applied to the character even if 'Individual Gravity' is unchecked.")]
	public float gravityMultiplier = 2f;

	public float airborneThreshold = 0.6f;

	public float slopeStartAngle = 50f;

	public float slopeEndAngle = 85f;

	public float spherecastRadius = 0.1f;

	public LayerMask groundLayers;

	private PhysicMaterial zeroFrictionMaterial;

	private PhysicMaterial highFrictionMaterial;

	protected Rigidbody r;

	protected const float half = 0.5f;

	protected float originalHeight;

	protected Vector3 originalCenter;

	protected CapsuleCollider capsule;

	public abstract void Move(Vector3 deltaPosition, Quaternion deltaRotation);

	protected Vector3 GetGravity()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gravityTarget != (Object)null)
		{
			Vector3 val = gravityTarget.position - ((Component)this).transform.position;
			Vector3 normalized = val.normalized;
			val = Physics.gravity;
			return normalized * val.magnitude;
		}
		return Physics.gravity;
	}

	protected virtual void Start()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected Obj, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected Obj, but got Unknown
		Collider component = ((Component)this).GetComponent<Collider>();
		capsule = (CapsuleCollider)(object)((component is CapsuleCollider) ? component : null);
		r = ((Component)this).GetComponent<Rigidbody>();
		originalHeight = capsule.height;
		originalCenter = capsule.center;
		zeroFrictionMaterial = new PhysicMaterial();
		zeroFrictionMaterial.dynamicFriction = 0f;
		zeroFrictionMaterial.staticFriction = 0f;
		zeroFrictionMaterial.frictionCombine = (PhysicMaterialCombine)2;
		zeroFrictionMaterial.bounciness = 0f;
		zeroFrictionMaterial.bounceCombine = (PhysicMaterialCombine)2;
		highFrictionMaterial = new PhysicMaterial();
		r.constraints = (RigidbodyConstraints)112;
	}

	protected virtual RaycastHit GetSpherecastHit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = ((Component)this).transform.up;
		Ray val = new Ray(r.position + up * airborneThreshold, -up);
		RaycastHit result = default;
		result.point = ((Component)this).transform.position - ((Component)((Component)this).transform).transform.up * airborneThreshold;
		result.normal = ((Component)this).transform.up;
		Physics.SphereCast(val, spherecastRadius, ref result, airborneThreshold * 2f, LayerMask.op_Implicit(groundLayers));
		return result;
	}

	public float GetAngleFromForward(Vector3 worldDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformDirection(worldDirection);
		return Mathf.Atan2(val.x, val.z) * 57.29578f;
	}

	protected void RigidbodyRotateAround(Vector3 point, Vector3 axis, float angle)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.AngleAxis(angle, axis);
		Vector3 val2 = ((Component)this).transform.position - point;
		r.MovePosition(point + val * val2);
		r.MoveRotation(val * ((Component)this).transform.rotation);
	}

	protected void ScaleCapsule(float mlp)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (capsule.height != originalHeight * mlp)
		{
			capsule.height = Mathf.MoveTowards(capsule.height, originalHeight * mlp, Time.deltaTime * 4f);
			capsule.center = Vector3.MoveTowards(capsule.center, originalCenter * mlp, Time.deltaTime * 2f);
		}
	}

	protected void HighFriction()
	{
		((Collider)capsule).material = highFrictionMaterial;
	}

	protected void ZeroFriction()
	{
		((Collider)capsule).material = zeroFrictionMaterial;
	}

	protected float GetSlopeDamper(Vector3 velocity, Vector3 groundNormal)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = 90f - Vector3.Angle(velocity, groundNormal);
		num -= slopeStartAngle;
		float num2 = slopeEndAngle - slopeStartAngle;
		return 1f - Mathf.Clamp(num / num2, 0f, 1f);
	}
}
