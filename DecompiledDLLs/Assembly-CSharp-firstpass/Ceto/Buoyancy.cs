using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Buoyancy/Buoyancy")]
public class Buoyancy : MonoBehaviour
{
	public enum MASS_UNIT
	{
		KILOGRAMS,
		TENS_OF_KILOGRAMS,
		TONNES,
		TENS_OF_TONNES
	}

	private float DENSITY_WATER = 999.97f;

	public float radius = 0.5f;

	[Range(100f, 10000f)]
	public float density = 400f;

	[Range(0f, 100f)]
	public float stickyness = 0.1f;

	public MASS_UNIT unit = MASS_UNIT.TENS_OF_TONNES;

	public float dragCoefficient = 0.3f;

	public bool PartOfStructure { get; set; }

	public float Volume { get; private set; }

	public float SubmergedVolume { get; private set; }

	public float PercentageSubmerged => SubmergedVolume / Volume;

	public float SurfaceArea { get; private set; }

	public float Mass { get; private set; }

	public float WaterHeight { get; private set; }

	public Vector3 BuoyantForce
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

	public Vector3 DragForce
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

	public Vector3 Stickyness
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

	public Vector3 TotalForces
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return BuoyantForce + DragForce + Stickyness;
		}
	}

	private void Start()
	{
		UpdateProperties();
	}

	private void FixedUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!PartOfStructure)
		{
			Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)this).gameObject.AddComponent<Rigidbody>();
			}
			val.mass = Mass;
			UpdateProperties();
			UpdateForces(val);
			val.AddForce(TotalForces);
		}
	}

	public void UpdateProperties()
	{
		Volume = 4.1887903f * Mathf.Pow(radius, 3f);
		Mass = Volume * density * GetUnitScale();
		SurfaceArea = (float)Math.PI * 4f * Mathf.Pow(radius, 2f);
	}

	public void UpdateForces(Rigidbody body)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ocean.Instance == (Object)null)
		{
			BuoyantForce = Vector3.zero;
			DragForce = Vector3.zero;
			Stickyness = Vector3.zero;
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		WaterHeight = Ocean.Instance.QueryWaves(position.x, position.z);
		CalculateSubmersion(radius, position.y);
		float unitScale = GetUnitScale();
		float num = DENSITY_WATER * unitScale * SubmergedVolume;
		BuoyantForce = Physics.gravity * (0f - num);
		Vector3 val = body.velocity;
		float magnitude = val.magnitude;
		val = val.normalized * magnitude * magnitude * -1f;
		DragForce = 0.5f * dragCoefficient * DENSITY_WATER * unitScale * SubmergedVolume * val;
		Stickyness = Vector3.up * (WaterHeight - position.y) * Mass * stickyness;
	}

	private void CalculateSubmersion(float r, float y)
	{
		float num = WaterHeight - (y - radius);
		float num2 = 2f * r - num;
		if (num2 <= 0f)
		{
			SubmergedVolume = Volume;
			return;
		}
		if (num2 > 2f * r)
		{
			SubmergedVolume = 0f;
			return;
		}
		float num3 = Mathf.Sqrt(num * num2);
		SubmergedVolume = (float)Math.PI / 6f * num * (3f * num3 * num3 + num * num);
	}

	private float GetUnitScale()
	{
		return (int)unit switch
		{
			0 => 1f, 
			1 => 0.1f, 
			2 => 0.001f, 
			3 => 0.0001f, 
			_ => 1f, 
		};
	}

	private void OnDrawGizmos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(((Component)this).transform.position, radius);
		}
	}
}
