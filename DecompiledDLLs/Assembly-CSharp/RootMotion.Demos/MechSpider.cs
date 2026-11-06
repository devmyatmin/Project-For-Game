using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.Demos;

public class MechSpider : MonoBehaviour
{
	public LayerMask raycastLayers;

	public float scale = 1f;

	public Transform body;

	public MechSpiderLeg[] legs;

	public float legRotationWeight = 1f;

	public float rootPositionSpeed = 5f;

	public float rootRotationSpeed = 30f;

	public float breatheSpeed = 2f;

	public float breatheMagnitude = 0.2f;

	public float height = 3.5f;

	public float minHeight = 2f;

	public float raycastHeight = 10f;

	public float raycastDistance = 5f;

	private Vector3 lastPosition;

	private Vector3 defaultBodyLocalPosition;

	private float sine;

	private RaycastHit rootHit;

	public Vector3 velocity
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		lastPosition = ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		velocity = (((Component)this).transform.position - lastPosition) / Time.deltaTime;
		lastPosition = ((Component)this).transform.position;
		Vector3 legsPlaneNormal = GetLegsPlaneNormal();
		Quaternion val = Quaternion.FromToRotation(((Component)this).transform.up, legsPlaneNormal);
		((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, val * ((Component)this).transform.rotation, Time.deltaTime * rootRotationSpeed);
		Vector3 val2 = Vector3.Project(GetLegCentroid() + ((Component)this).transform.up * height * scale - ((Component)this).transform.position, ((Component)this).transform.up);
		Transform transform = ((Component)this).transform;
		transform.position += val2 * Time.deltaTime * (rootPositionSpeed * scale);
		if (Physics.Raycast(((Component)this).transform.position + ((Component)this).transform.up * raycastHeight * scale, -((Component)this).transform.up, ref rootHit, raycastHeight * scale + raycastDistance * scale, LayerMask.op_Implicit(raycastLayers)))
		{
			ref RaycastHit reference = ref rootHit;
			reference.distance -= raycastHeight * scale + minHeight * scale;
			if (rootHit.distance < 0f)
			{
				Vector3 val3 = ((Component)this).transform.position - ((Component)this).transform.up * rootHit.distance;
				((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, val3, Time.deltaTime * rootPositionSpeed * scale);
			}
		}
		sine += Time.deltaTime * breatheSpeed;
		if (sine >= (float)Math.PI * 2f)
		{
			sine -= (float)Math.PI * 2f;
		}
		float num = Mathf.Sin(sine) * breatheMagnitude * scale;
		Vector3 val4 = ((Component)this).transform.up * num;
		((Component)body).transform.position = ((Component)this).transform.position + val4;
	}

	private Vector3 GetLegCentroid()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 1f / (float)legs.Length;
		for (int i = 0; i < legs.Length; i++)
		{
			val += legs[i].position * num;
		}
		return val;
	}

	private Vector3 GetLegsPlaneNormal()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.up;
		if (legRotationWeight <= 0f)
		{
			return val;
		}
		float num = 1f / Mathf.Lerp((float)legs.Length, 1f, legRotationWeight);
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 val2 = legs[i].position - (((Component)this).transform.position - ((Component)this).transform.up * height * scale);
			Vector3 up = ((Component)this).transform.up;
			Vector3 val3 = val2;
			Vector3.OrthoNormalize(ref up, ref val3);
			Quaternion val4 = Quaternion.FromToRotation(val3, val2);
			val4 = Quaternion.Lerp(Quaternion.identity, val4, num);
			val = val4 * val;
		}
		return val;
	}
}
