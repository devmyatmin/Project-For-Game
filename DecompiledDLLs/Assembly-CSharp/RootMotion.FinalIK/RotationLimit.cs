using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class RotationLimit : MonoBehaviour
{
	public Vector3 axis = Vector3.forward;

	[HideInInspector]
	public Quaternion defaultLocalRotation;

	private bool initiated;

	private bool applicationQuit;

	private bool defaultLocalRotationSet;

	public Vector3 secondaryAxis
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(axis.y, axis.z, axis.x);
		}
	}

	public Vector3 crossAxis
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.Cross(axis, secondaryAxis);
		}
	}

	public bool defaultLocalRotationOverride { get; private set; }

	public void SetDefaultLocalRotation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		defaultLocalRotation = ((Component)this).transform.localRotation;
		defaultLocalRotationSet = true;
		defaultLocalRotationOverride = false;
	}

	public void SetDefaultLocalRotation(Quaternion localRotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		defaultLocalRotation = localRotation;
		defaultLocalRotationSet = true;
		defaultLocalRotationOverride = true;
	}

	public Quaternion GetLimitedLocalRotation(Quaternion localRotation, out bool changed)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			Awake();
		}
		Quaternion val = Quaternion.Inverse(defaultLocalRotation) * localRotation;
		Quaternion val2 = LimitRotation(val);
		val2 = Quaternion.Normalize(val2);
		changed = val2 != val;
		if (!changed)
		{
			return localRotation;
		}
		return defaultLocalRotation * val2;
	}

	public bool Apply()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		bool changed = false;
		((Component)this).transform.localRotation = GetLimitedLocalRotation(((Component)this).transform.localRotation, out changed);
		return changed;
	}

	public void Disable()
	{
		if (initiated)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		Awake();
		((Behaviour)this).enabled = false;
	}

	protected abstract Quaternion LimitRotation(Quaternion rotation);

	private void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!defaultLocalRotationSet)
		{
			SetDefaultLocalRotation();
		}
		if (axis == Vector3.zero)
		{
			Debug.LogError((object)"Axis is Vector3.zero.");
		}
		initiated = true;
	}

	private void LateUpdate()
	{
		Apply();
	}

	public void LogWarning(string message)
	{
		Warning.Log(message, ((Component)this).transform);
	}

	protected static Quaternion Limit1DOF(Quaternion rotation, Vector3 axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.FromToRotation(rotation * axis, axis) * rotation;
	}

	protected static Quaternion LimitTwist(Quaternion rotation, Vector3 axis, Vector3 orthoAxis, float twistLimit)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		twistLimit = Mathf.Clamp(twistLimit, 0f, 180f);
		if (twistLimit >= 180f)
		{
			return rotation;
		}
		Vector3 val = rotation * axis;
		Vector3 val2 = orthoAxis;
		Vector3.OrthoNormalize(ref val, ref val2);
		Vector3 val3 = rotation * orthoAxis;
		Vector3.OrthoNormalize(ref val, ref val3);
		Quaternion val4 = Quaternion.FromToRotation(val3, val2) * rotation;
		if (twistLimit <= 0f)
		{
			return val4;
		}
		return Quaternion.RotateTowards(val4, rotation, twistLimit);
	}

	protected static float GetOrthogonalAngle(Vector3 v1, Vector3 v2, Vector3 normal)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3.OrthoNormalize(ref normal, ref v1);
		Vector3.OrthoNormalize(ref normal, ref v2);
		return Vector3.Angle(v1, v2);
	}

	protected RotationLimit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
