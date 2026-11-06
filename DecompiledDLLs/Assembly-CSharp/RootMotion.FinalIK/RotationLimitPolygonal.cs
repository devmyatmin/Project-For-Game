using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page14.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Polygonal")]
public class RotationLimitPolygonal : RotationLimit
{
	[Serializable]
	public class ReachCone
	{
		public Vector3[] tetrahedron;

		public float volume;

		public Vector3 S;

		public Vector3 B;

		public Vector3 o
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return tetrahedron[0];
			}
		}

		public Vector3 a
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return tetrahedron[1];
			}
		}

		public Vector3 b
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return tetrahedron[2];
			}
		}

		public Vector3 c
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return tetrahedron[3];
			}
		}

		public bool isValid => volume > 0f;

		public ReachCone(Vector3 _o, Vector3 _a, Vector3 _b, Vector3 _c)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			tetrahedron = new Vector3[4];
			tetrahedron[0] = _o;
			tetrahedron[1] = _a;
			tetrahedron[2] = _b;
			tetrahedron[3] = _c;
			volume = 0f;
			S = Vector3.zero;
			B = Vector3.zero;
		}

		public void Calculate()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3.Cross(a, b);
			volume = Vector3.Dot(val, c) / 6f;
			Vector3 val2 = Vector3.Cross(a, b);
			S = val2.normalized;
			val2 = Vector3.Cross(b, c);
			B = val2.normalized;
		}
	}

	[Serializable]
	public class LimitPoint
	{
		public Vector3 point;

		public float tangentWeight;

		public LimitPoint()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			point = Vector3.forward;
			tangentWeight = 1f;
		}
	}

	[Range(0f, 180f)]
	public float twistLimit = 180f;

	[Range(0f, 3f)]
	public int smoothIterations;

	[HideInInspector]
	public LimitPoint[] points;

	[HideInInspector]
	public Vector3[] P;

	[HideInInspector]
	public ReachCone[] reachCones = new ReachCone[0];

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page14.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_polygonal.html");
	}

	[ContextMenu("Support Group")]
	private void SupportGroup()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	private void ASThread()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public void SetLimitPoints(LimitPoint[] points)
	{
		if (points.Length < 3)
		{
			LogWarning("The polygon must have at least 3 Limit Points.");
			return;
		}
		this.points = points;
		BuildReachCones();
	}

	protected override Quaternion LimitRotation(Quaternion rotation)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (reachCones.Length == 0)
		{
			Start();
		}
		return RotationLimit.LimitTwist(LimitSwing(rotation), axis, secondaryAxis, twistLimit);
	}

	private void Start()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (points.Length < 3)
		{
			ResetToDefault();
		}
		for (int i = 0; i < reachCones.Length; i++)
		{
			if (!reachCones[i].isValid)
			{
				if (smoothIterations <= 0)
				{
					int num = 0;
					num = ((i < reachCones.Length - 1) ? (i + 1) : 0);
					LogWarning("Reach Cone {point " + i + ", point " + num + ", Origin} has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
				}
				else
				{
					LogWarning("One of the Reach Cones in the polygon has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
				}
			}
		}
		axis = axis.normalized;
	}

	public void ResetToDefault()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		points = new LimitPoint[4];
		for (int i = 0; i < points.Length; i++)
		{
			points[i] = new LimitPoint();
		}
		Quaternion val = Quaternion.AngleAxis(45f, Vector3.right);
		Quaternion val2 = Quaternion.AngleAxis(45f, Vector3.up);
		points[0].point = val * val2 * axis;
		points[1].point = Quaternion.Inverse(val) * val2 * axis;
		points[2].point = Quaternion.Inverse(val) * Quaternion.Inverse(val2) * axis;
		points[3].point = val * Quaternion.Inverse(val2) * axis;
		BuildReachCones();
	}

	public void BuildReachCones()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		smoothIterations = Mathf.Clamp(smoothIterations, 0, 3);
		P = new Vector3[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			P[i] = points[i].point.normalized;
		}
		for (int j = 0; j < smoothIterations; j++)
		{
			P = SmoothPoints();
		}
		reachCones = new ReachCone[P.Length];
		for (int k = 0; k < reachCones.Length - 1; k++)
		{
			reachCones[k] = new ReachCone(Vector3.zero, axis.normalized, P[k], P[k + 1]);
		}
		reachCones[P.Length - 1] = new ReachCone(Vector3.zero, axis.normalized, P[P.Length - 1], P[0]);
		for (int l = 0; l < reachCones.Length; l++)
		{
			reachCones[l].Calculate();
		}
	}

	private Vector3[] SmoothPoints()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[P.Length * 2];
		float scalar = GetScalar(P.Length);
		for (int i = 0; i < array.Length; i += 2)
		{
			array[i] = PointToTangentPlane(P[i / 2], 1f);
		}
		for (int j = 1; j < array.Length; j += 2)
		{
			Vector3 val = Vector3.zero;
			Vector3 zero = Vector3.zero;
			Vector3 val2 = Vector3.zero;
			if (j > 1 && j < array.Length - 2)
			{
				val = array[j - 2];
				val2 = array[j + 1];
			}
			else if (j == 1)
			{
				val = array[^2];
				val2 = array[j + 1];
			}
			else if (j == array.Length - 1)
			{
				val = array[j - 2];
				val2 = array[0];
			}
			zero = ((j >= array.Length - 1) ? array[0] : array[j + 1]);
			int num = array.Length / points.Length;
			array[j] = 0.5f * (array[j - 1] + zero) + scalar * points[j / num].tangentWeight * (zero - val) + scalar * points[j / num].tangentWeight * (array[j - 1] - val2);
		}
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = TangentPointToSphere(array[k], 1f);
		}
		return array;
	}

	private float GetScalar(int k)
	{
		if (k <= 3)
		{
			return 0.1667f;
		}
		return k switch
		{
			4 => 0.1036f, 
			5 => 0.085f, 
			6 => 0.0773f, 
			7 => 0.07f, 
			_ => 0.0625f, 
		};
	}

	private Vector3 PointToTangentPlane(Vector3 p, float r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(axis, p);
		float num2 = 2f * r * r / (r * r + num);
		return num2 * p + (1f - num2) * -axis;
	}

	private Vector3 TangentPointToSphere(Vector3 q, float r)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(q - axis, q - axis);
		float num2 = 4f * r * r / (4f * r * r + num);
		return num2 * q + (1f - num2) * -axis;
	}

	private Quaternion LimitSwing(Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (rotation == Quaternion.identity)
		{
			return rotation;
		}
		Vector3 val = rotation * axis;
		int reachCone = GetReachCone(val);
		if (reachCone == -1)
		{
			if (!Warning.logged)
			{
				LogWarning("RotationLimitPolygonal reach cones are invalid.");
			}
			return rotation;
		}
		if (Vector3.Dot(reachCones[reachCone].B, val) > 0f)
		{
			return rotation;
		}
		Vector3 val2 = Vector3.Cross(axis, val);
		val = Vector3.Cross(-reachCones[reachCone].B, val2);
		return Quaternion.FromToRotation(rotation * axis, val) * rotation;
	}

	private int GetReachCone(Vector3 L)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(reachCones[0].S, L);
		for (int i = 0; i < reachCones.Length; i++)
		{
			float num2 = num;
			num = ((i >= reachCones.Length - 1) ? Vector3.Dot(reachCones[0].S, L) : Vector3.Dot(reachCones[i + 1].S, L));
			if (num2 >= 0f && num < 0f)
			{
				return i;
			}
		}
		return -1;
	}
}
