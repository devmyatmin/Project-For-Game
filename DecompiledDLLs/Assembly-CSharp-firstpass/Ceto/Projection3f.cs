using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

public class Projection3f : IProjection
{
	private Ocean m_ocean;

	private Matrix4x4 m_projectorP;

	private Matrix4x4 m_projectorV;

	private Matrix4x4 m_projectorR;

	private Matrix4x4 m_projectorI;

	private Vector3[] m_frustumCorners;

	private List<Vector3> m_pointList;

	private static readonly Vector4[] m_quad = new Vector4[4]
	{
		new Vector4(0f, 0f, 0f, 1f),
		new Vector4(1f, 0f, 0f, 1f),
		new Vector4(1f, 1f, 0f, 1f),
		new Vector4(0f, 1f, 0f, 1f)
	};

	private static readonly Vector4[] m_corners = new Vector4[8]
	{
		new Vector4(-1f, -1f, -1f, 1f),
		new Vector4(1f, -1f, -1f, 1f),
		new Vector4(1f, 1f, -1f, 1f),
		new Vector4(-1f, 1f, -1f, 1f),
		new Vector4(-1f, -1f, 1f, 1f),
		new Vector4(1f, -1f, 1f, 1f),
		new Vector4(1f, 1f, 1f, 1f),
		new Vector4(-1f, 1f, 1f, 1f)
	};

	private static readonly int[,] m_indices = new int[12, 2]
	{
		{ 0, 1 },
		{ 1, 2 },
		{ 2, 3 },
		{ 3, 0 },
		{ 4, 5 },
		{ 5, 6 },
		{ 6, 7 },
		{ 7, 4 },
		{ 0, 4 },
		{ 1, 5 },
		{ 2, 6 },
		{ 3, 7 }
	};

	public bool IsDouble => false;

	public bool IsFlipped { get; set; }

	public bool TightFit { get; set; }

	public Projection3f(Ocean ocean)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_ocean = ocean;
		m_projectorP = default;
		m_projectorV = default;
		m_projectorR = Matrix4x4.identity;
		m_projectorI = default;
		m_pointList = new List<Vector3>(12);
		m_frustumCorners = new Vector3[8];
	}

	public void UpdateProjection(Camera camera, CameraData data)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		Camera val = camera;
		if ((Object)(object)val == (Object)null || data == null)
		{
			return;
		}
		if (data.projection == null)
		{
			data.projection = new ProjectionData();
		}
		if (!data.projection.IsViewUpdated(camera))
		{
			if (Ocean.DISABLE_PROJECT_SCENE_VIEW && ((Object)val).name == "SceneCamera" && (Object)(object)Camera.main != (Object)null)
			{
				val = Camera.main;
			}
			AimProjector(val);
			Matrix4x4 val2 = m_projectorP * m_projectorV;
			CreateRangeMatrix(val, val2);
			Matrix4x4 ivp = val2.inverse * m_projectorR;
			m_projectorI.SetRow(0, HProject(ivp, m_quad[0]));
			m_projectorI.SetRow(1, HProject(ivp, m_quad[1]));
			m_projectorI.SetRow(2, HProject(ivp, m_quad[2]));
			m_projectorI.SetRow(3, HProject(ivp, m_quad[3]));
			data.projection.projectorVP = m_projectorR.inverse * val2;
			data.projection.interpolation = m_projectorI;
			data.projection.SetViewAsUpdated(camera);
		}
	}

	private void AimProjector(Camera cam)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		m_projectorP = cam.projectionMatrix;
		Vector3 position = ((Component)cam).transform.position;
		Vector3 forward = ((Component)cam).transform.forward;
		Vector3 val = default;
		float level = m_ocean.level;
		float num = (TightFit ? 20f : 50f);
		float num2 = Math.Max(0f, m_ocean.FindMaxDisplacement(inculdeOverlays: true)) + num;
		if (Ocean.DISABLE_PROJECTION_FLIPPING)
		{
			if (position.y < level)
			{
				position.y = level;
			}
			IsFlipped = false;
			position.y = Math.Max(position.y, level + num2);
		}
		else if (position.y < level)
		{
			IsFlipped = true;
			position.y = Math.Min(position.y, level - num2);
		}
		else
		{
			IsFlipped = false;
			position.y = Math.Max(position.y, level + num2);
		}
		val = position + forward * 50f;
		val.y = m_ocean.level;
		LookAt(position, val, Vector3.up);
	}

	private void CreateRangeMatrix(Camera cam, Matrix4x4 projectorVP)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		m_pointList.Clear();
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Matrix4x4 val = m_projectorP * worldToCameraMatrix;
		Matrix4x4 inverse = val.inverse;
		Vector3 up = Vector3.up;
		Vector4 zero = Vector4.zero;
		float level = m_ocean.level;
		float num = Mathf.Max(1f, m_ocean.FindMaxDisplacement(inculdeOverlays: true));
		for (int i = 0; i < 8; i++)
		{
			zero = inverse * m_corners[i];
			zero.x /= zero.w;
			zero.y /= zero.w;
			zero.z /= zero.w;
			m_frustumCorners[i] = Vector4.op_Implicit(zero);
		}
		for (int j = 0; j < 8; j++)
		{
			if (m_frustumCorners[j].y <= level + num && m_frustumCorners[j].y >= level - num)
			{
				m_pointList.Add(m_frustumCorners[j]);
			}
		}
		for (int k = 0; k < 12; k++)
		{
			Vector3 a = m_frustumCorners[m_indices[k, 0]];
			Vector3 b = m_frustumCorners[m_indices[k, 1]];
			Vector3 q = default;
			Vector3 q2 = default;
			if (SegmentPlaneIntersection(a, b, up, level + num, ref q))
			{
				m_pointList.Add(q);
			}
			if (SegmentPlaneIntersection(a, b, up, level - num, ref q2))
			{
				m_pointList.Add(q2);
			}
		}
		int count = m_pointList.Count;
		if (count == 0)
		{
			m_projectorR[0, 0] = 1f;
			m_projectorR[0, 3] = 0f;
			m_projectorR[1, 1] = 1f;
			m_projectorR[1, 3] = 0f;
			return;
		}
		float num2 = float.PositiveInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = float.NegativeInfinity;
		Vector4 zero2 = Vector4.zero;
		for (int l = 0; l < count; l++)
		{
			zero2.x = m_pointList[l].x;
			zero2.y = level;
			zero2.z = m_pointList[l].z;
			zero2.w = 1f;
			zero = projectorVP * zero2;
			zero.x /= zero.w;
			zero.y /= zero.w;
			if (zero.x < num2)
			{
				num2 = zero.x;
			}
			if (zero.y < num3)
			{
				num3 = zero.y;
			}
			if (zero.x > num4)
			{
				num4 = zero.x;
			}
			if (zero.y > num5)
			{
				num5 = zero.y;
			}
		}
		m_projectorR[0, 0] = num4 - num2;
		m_projectorR[0, 3] = num2;
		m_projectorR[1, 1] = num5 - num3;
		m_projectorR[1, 3] = num3;
	}

	private Vector4 HProject(Matrix4x4 ivp, Vector4 corner)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		corner.z = -1f;
		Vector4 val = ivp * corner;
		corner.z = 1f;
		Vector4 val2 = ivp * corner;
		float level = m_ocean.level;
		Vector4 val3 = val2 - val;
		float num = (val.w * level - val.y) / (val3.y - val3.w * level);
		return val + val3 * num;
	}

	private bool SegmentPlaneIntersection(Vector3 a, Vector3 b, Vector3 n, float d, ref Vector3 q)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = b - a;
		float num = (d - Vector3.Dot(n, a)) / Vector3.Dot(n, val);
		if (num > -0f && num <= 1f)
		{
			q = a + num * val;
			return true;
		}
		return false;
	}

	public void LookAt(Vector3 position, Vector3 target, Vector3 up)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position - target;
		Vector3 normalized = val.normalized;
		val = Vector3.Cross(up, normalized);
		Vector3 normalized2 = val.normalized;
		Vector3 val2 = Vector3.Cross(normalized, normalized2);
		m_projectorV[0, 0] = normalized2.x;
		m_projectorV[0, 1] = normalized2.y;
		m_projectorV[0, 2] = normalized2.z;
		m_projectorV[0, 3] = 0f - Vector3.Dot(normalized2, position);
		m_projectorV[1, 0] = val2.x;
		m_projectorV[1, 1] = val2.y;
		m_projectorV[1, 2] = val2.z;
		m_projectorV[1, 3] = 0f - Vector3.Dot(val2, position);
		m_projectorV[2, 0] = normalized.x;
		m_projectorV[2, 1] = normalized.y;
		m_projectorV[2, 2] = normalized.z;
		m_projectorV[2, 3] = 0f - Vector3.Dot(normalized, position);
		m_projectorV[3, 0] = 0f;
		m_projectorV[3, 1] = 0f;
		m_projectorV[3, 2] = 0f;
		m_projectorV[3, 3] = 1f;
		ref Matrix4x4 projectorV = ref m_projectorV;
		projectorV[0, 0] = projectorV[0, 0] * -1f;
		projectorV = ref m_projectorV;
		projectorV[0, 1] = projectorV[0, 1] * -1f;
		projectorV = ref m_projectorV;
		projectorV[0, 2] = projectorV[0, 2] * -1f;
		projectorV = ref m_projectorV;
		projectorV[0, 3] = projectorV[0, 3] * -1f;
	}

	static Projection3f()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
	}
}
