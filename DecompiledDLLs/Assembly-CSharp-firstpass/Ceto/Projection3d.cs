using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

public class Projection3d : IProjection
{
	private Ocean m_ocean;

	private double[] m_projectorP;

	private double[] m_projectorV;

	private double[] m_projectorVP;

	private double[] m_projectorIVP;

	private double[] m_projectorR;

	private double[] m_projectorI;

	private List<double[]> m_frustumCorners;

	private List<double[]> m_pointList;

	private List<double[]> m_quad;

	private List<double[]> m_corners;

	private double[] MATRIX_BUFFER0;

	private double[] MATRIX_BUFFER1;

	private double[] VECTOR_BUFFER;

	private double[] m_xaxis;

	private double[] m_yaxis;

	private double[] m_zaxis;

	private double[] m_up;

	private double[] m_a;

	private double[] m_b;

	private double[] m_ab;

	private double[] m_pos;

	private double[] m_dir;

	private double[] m_lookAt;

	private double[] m_p;

	private double[] m_q;

	private double[] m_p0;

	private double[] m_p1;

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

	public bool IsDouble => true;

	public bool IsFlipped { get; set; }

	public bool TightFit { get; set; }

	public Projection3d(Ocean ocean)
	{
		m_ocean = ocean;
		m_projectorP = new double[16];
		m_projectorV = new double[16];
		m_projectorVP = new double[16];
		m_projectorIVP = new double[16];
		m_projectorR = new double[16];
		m_projectorI = new double[16];
		MATRIX_BUFFER0 = new double[16];
		MATRIX_BUFFER1 = new double[16];
		VECTOR_BUFFER = new double[4];
		m_up = new double[3] { 0.0, 1.0, 0.0 };
		m_xaxis = new double[3];
		m_yaxis = new double[3];
		m_zaxis = new double[3];
		m_a = new double[4];
		m_b = new double[4];
		m_ab = new double[4];
		m_pos = new double[3];
		m_dir = new double[3];
		m_lookAt = new double[3];
		m_p = new double[4];
		m_q = new double[4];
		m_p0 = new double[4];
		m_p1 = new double[4];
		Identity(m_projectorR);
		m_pointList = new List<double[]>(32);
		for (int i = 0; i < 32; i++)
		{
			m_pointList.Add(new double[3]);
		}
		m_frustumCorners = new List<double[]>(8);
		for (int j = 0; j < 8; j++)
		{
			m_frustumCorners.Add(new double[3]);
		}
		m_quad = new List<double[]>(4);
		m_quad.Add(new double[4] { 0.0, 0.0, 0.0, 1.0 });
		m_quad.Add(new double[4] { 1.0, 0.0, 0.0, 1.0 });
		m_quad.Add(new double[4] { 1.0, 1.0, 0.0, 1.0 });
		m_quad.Add(new double[4] { 0.0, 1.0, 0.0, 1.0 });
		m_corners = new List<double[]>(8);
		m_corners.Add(new double[4] { -1.0, -1.0, -1.0, 1.0 });
		m_corners.Add(new double[4] { 1.0, -1.0, -1.0, 1.0 });
		m_corners.Add(new double[4] { 1.0, 1.0, -1.0, 1.0 });
		m_corners.Add(new double[4] { -1.0, 1.0, -1.0, 1.0 });
		m_corners.Add(new double[4] { -1.0, -1.0, 1.0, 1.0 });
		m_corners.Add(new double[4] { 1.0, -1.0, 1.0, 1.0 });
		m_corners.Add(new double[4] { 1.0, 1.0, 1.0, 1.0 });
		m_corners.Add(new double[4] { -1.0, 1.0, 1.0, 1.0 });
	}

	public void UpdateProjection(Camera camera, CameraData data)
	{
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
			MulMatrixByMatrix(m_projectorVP, m_projectorP, m_projectorV);
			CreateRangeMatrix(val, m_projectorVP);
			Inverse(MATRIX_BUFFER0, m_projectorVP);
			MulMatrixByMatrix(m_projectorIVP, MATRIX_BUFFER0, m_projectorR);
			HProject(m_projectorIVP, m_quad[0], VECTOR_BUFFER);
			SetRow(0, m_projectorI, VECTOR_BUFFER);
			HProject(m_projectorIVP, m_quad[1], VECTOR_BUFFER);
			SetRow(1, m_projectorI, VECTOR_BUFFER);
			HProject(m_projectorIVP, m_quad[2], VECTOR_BUFFER);
			SetRow(2, m_projectorI, VECTOR_BUFFER);
			HProject(m_projectorIVP, m_quad[3], VECTOR_BUFFER);
			SetRow(3, m_projectorI, VECTOR_BUFFER);
			Inverse(MATRIX_BUFFER0, m_projectorR);
			MulMatrixByMatrix(MATRIX_BUFFER1, MATRIX_BUFFER0, m_projectorVP);
			CopyMatrix(ref data.projection.projectorVP, MATRIX_BUFFER1);
			CopyMatrix(ref data.projection.interpolation, m_projectorI);
			data.projection.SetViewAsUpdated(camera);
		}
	}

	private void AimProjector(Camera cam)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		CopyMatrix(m_projectorP, cam.projectionMatrix);
		CopyVector3(m_pos, ((Component)cam).transform.position);
		CopyVector3(m_dir, ((Component)cam).transform.forward);
		double num = m_ocean.level;
		double num2 = (TightFit ? 20.0 : 50.0);
		double num3 = Math.Max(0.0, m_ocean.FindMaxDisplacement(inculdeOverlays: true)) + num2;
		if (Ocean.DISABLE_PROJECTION_FLIPPING)
		{
			if (m_pos[1] < num)
			{
				m_pos[1] = num;
			}
			IsFlipped = false;
			m_pos[1] = Math.Max(m_pos[1], num + num3);
		}
		else if (m_pos[1] < num)
		{
			IsFlipped = true;
			m_pos[1] = Math.Min(m_pos[1], num - num3);
		}
		else
		{
			IsFlipped = false;
			m_pos[1] = Math.Max(m_pos[1], num + num3);
		}
		m_lookAt[0] = m_pos[0] + m_dir[0] * 50.0;
		m_lookAt[1] = m_ocean.level;
		m_lookAt[2] = m_pos[2] + m_dir[2] * 50.0;
		LookAt(m_pos, m_lookAt, m_up);
	}

	private void CreateRangeMatrix(Camera cam, double[] projectorVP)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Identity(m_projectorR);
		CopyMatrix(MATRIX_BUFFER0, cam.worldToCameraMatrix);
		MulMatrixByMatrix(MATRIX_BUFFER1, m_projectorP, MATRIX_BUFFER0);
		Inverse(MATRIX_BUFFER0, MATRIX_BUFFER1);
		double num = m_ocean.level;
		double num2 = Math.Max(1.0, m_ocean.FindMaxDisplacement(inculdeOverlays: true));
		for (int i = 0; i < 8; i++)
		{
			MulVector4ByMatrix(m_p, m_corners[i], MATRIX_BUFFER0);
			m_frustumCorners[i][0] = m_p[0] / m_p[3];
			m_frustumCorners[i][1] = m_p[1] / m_p[3];
			m_frustumCorners[i][2] = m_p[2] / m_p[3];
		}
		int num3 = 0;
		for (int j = 0; j < 8; j++)
		{
			if (m_frustumCorners[j][1] <= num + num2 && m_frustumCorners[j][1] >= num - num2)
			{
				m_pointList[num3][0] = m_frustumCorners[j][0];
				m_pointList[num3][1] = m_frustumCorners[j][1];
				m_pointList[num3][2] = m_frustumCorners[j][2];
				num3++;
			}
		}
		for (int k = 0; k < 12; k++)
		{
			int index = m_indices[k, 0];
			m_p0[0] = m_frustumCorners[index][0];
			m_p0[1] = m_frustumCorners[index][1];
			m_p0[2] = m_frustumCorners[index][2];
			int index2 = m_indices[k, 1];
			m_p1[0] = m_frustumCorners[index2][0];
			m_p1[1] = m_frustumCorners[index2][1];
			m_p1[2] = m_frustumCorners[index2][2];
			if (SegmentPlaneIntersection(m_p0, m_p1, m_up, num + num2, m_pointList[num3]))
			{
				num3++;
			}
			if (SegmentPlaneIntersection(m_p0, m_p1, m_up, num - num2, m_pointList[num3]))
			{
				num3++;
			}
		}
		if (num3 > m_pointList.Count)
		{
			throw new InvalidOperationException("Count can not be greater than poin list count");
		}
		if (num3 == 0)
		{
			m_projectorR[0] = 1.0;
			m_projectorR[12] = 0.0;
			m_projectorR[5] = 1.0;
			m_projectorR[13] = 0.0;
			return;
		}
		double num4 = double.PositiveInfinity;
		double num5 = double.PositiveInfinity;
		double num6 = double.NegativeInfinity;
		double num7 = double.NegativeInfinity;
		for (int l = 0; l < num3; l++)
		{
			m_q[0] = m_pointList[l][0];
			m_q[1] = num;
			m_q[2] = m_pointList[l][2];
			m_q[3] = 1.0;
			MulVector4ByMatrix(m_p, m_q, projectorVP);
			m_p[0] /= m_p[3];
			m_p[1] /= m_p[3];
			if (m_p[0] < num4)
			{
				num4 = m_p[0];
			}
			if (m_p[1] < num5)
			{
				num5 = m_p[1];
			}
			if (m_p[0] > num6)
			{
				num6 = m_p[0];
			}
			if (m_p[1] > num7)
			{
				num7 = m_p[1];
			}
		}
		m_projectorR[0] = num6 - num4;
		m_projectorR[12] = num4;
		m_projectorR[5] = num7 - num5;
		m_projectorR[13] = num5;
	}

	private void HProject(double[] ivp, double[] corner, double[] result)
	{
		corner[2] = -1.0;
		MulVector4ByMatrix(m_a, corner, ivp);
		corner[2] = 1.0;
		MulVector4ByMatrix(m_b, corner, ivp);
		double num = m_ocean.level;
		Sub4(m_ab, m_b, m_a);
		double num2 = (m_a[3] * num - m_a[1]) / (m_ab[1] - m_ab[3] * num);
		result[0] = m_a[0] + m_ab[0] * num2;
		result[1] = m_a[1] + m_ab[1] * num2;
		result[2] = m_a[2] + m_ab[2] * num2;
		result[3] = m_a[3] + m_ab[3] * num2;
	}

	private bool SegmentPlaneIntersection(double[] a, double[] b, double[] n, double d, double[] q)
	{
		Sub3(m_ab, b, a);
		double num = (d - Dot3(n, a)) / Dot3(n, m_ab);
		if (num > -0.0 && num <= 1.0)
		{
			q[0] = a[0] + num * m_ab[0];
			q[1] = a[1] + num * m_ab[1];
			q[2] = a[2] + num * m_ab[2];
			return true;
		}
		return false;
	}

	public void LookAt(double[] position, double[] target, double[] up)
	{
		Sub3(m_zaxis, position, target);
		Normalize3(m_zaxis);
		Cross3(m_xaxis, up, m_zaxis);
		Normalize3(m_xaxis);
		Cross3(m_yaxis, m_zaxis, m_xaxis);
		m_projectorV[0] = m_xaxis[0];
		m_projectorV[4] = m_xaxis[1];
		m_projectorV[8] = m_xaxis[2];
		m_projectorV[12] = 0.0 - Dot3(m_xaxis, position);
		m_projectorV[1] = m_yaxis[0];
		m_projectorV[5] = m_yaxis[1];
		m_projectorV[9] = m_yaxis[2];
		m_projectorV[13] = 0.0 - Dot3(m_yaxis, position);
		m_projectorV[2] = m_zaxis[0];
		m_projectorV[6] = m_zaxis[1];
		m_projectorV[10] = m_zaxis[2];
		m_projectorV[14] = 0.0 - Dot3(m_zaxis, position);
		m_projectorV[3] = 0.0;
		m_projectorV[7] = 0.0;
		m_projectorV[11] = 0.0;
		m_projectorV[15] = 1.0;
		m_projectorV[0] *= -1.0;
		m_projectorV[4] *= -1.0;
		m_projectorV[8] *= -1.0;
		m_projectorV[12] *= -1.0;
	}

	private void MulVector4ByMatrix(double[] des, double[] src, double[] m)
	{
		des[0] = m[0] * src[0] + m[4] * src[1] + m[8] * src[2] + m[12] * src[3];
		des[1] = m[1] * src[0] + m[5] * src[1] + m[9] * src[2] + m[13] * src[3];
		des[2] = m[2] * src[0] + m[6] * src[1] + m[10] * src[2] + m[14] * src[3];
		des[3] = m[3] * src[0] + m[7] * src[1] + m[11] * src[2] + m[15] * src[3];
	}

	private void MulMatrixByMatrix(double[] des, double[] m1, double[] m2)
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				des[i + j * 4] = m1[i] * m2[j * 4] + m1[i + 4] * m2[1 + j * 4] + m1[i + 8] * m2[2 + j * 4] + m1[i + 12] * m2[3 + j * 4];
			}
		}
	}

	private void Identity(double[] m)
	{
		m[0] = 1.0;
		m[4] = 0.0;
		m[8] = 0.0;
		m[12] = 0.0;
		m[1] = 0.0;
		m[5] = 1.0;
		m[9] = 0.0;
		m[13] = 0.0;
		m[2] = 0.0;
		m[6] = 0.0;
		m[10] = 1.0;
		m[14] = 0.0;
		m[3] = 0.0;
		m[7] = 0.0;
		m[11] = 0.0;
		m[15] = 1.0;
	}

	private void CopyMatrix(double[] des, Matrix4x4 m)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		des[0] = m.m00;
		des[4] = m.m01;
		des[8] = m.m02;
		des[12] = m.m03;
		des[1] = m.m10;
		des[5] = m.m11;
		des[9] = m.m12;
		des[13] = m.m13;
		des[2] = m.m20;
		des[6] = m.m21;
		des[10] = m.m22;
		des[14] = m.m23;
		des[3] = m.m30;
		des[7] = m.m31;
		des[11] = m.m32;
		des[15] = m.m33;
	}

	private void CopyMatrix(ref Matrix4x4 des, double[] m)
	{
		des.m00 = (float)m[0];
		des.m01 = (float)m[4];
		des.m02 = (float)m[8];
		des.m03 = (float)m[12];
		des.m10 = (float)m[1];
		des.m11 = (float)m[5];
		des.m12 = (float)m[9];
		des.m13 = (float)m[13];
		des.m20 = (float)m[2];
		des.m21 = (float)m[6];
		des.m22 = (float)m[10];
		des.m23 = (float)m[14];
		des.m30 = (float)m[3];
		des.m31 = (float)m[7];
		des.m32 = (float)m[11];
		des.m33 = (float)m[15];
	}

	private void CopyVector3(double[] des, Vector3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		des[0] = v.x;
		des[1] = v.y;
		des[2] = v.z;
	}

	private void Normalize3(double[] v)
	{
		double num = 1.0 / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
		v[0] *= num;
		v[1] *= num;
		v[2] *= num;
	}

	private void Sub3(double[] des, double[] v1, double[] v2)
	{
		des[0] = v1[0] - v2[0];
		des[1] = v1[1] - v2[1];
		des[2] = v1[2] - v2[2];
	}

	private void Sub4(double[] des, double[] v1, double[] v2)
	{
		des[0] = v1[0] - v2[0];
		des[1] = v1[1] - v2[1];
		des[2] = v1[2] - v2[2];
		des[3] = v1[3] - v2[3];
	}

	private double Dot3(double[] v1, double[] v2)
	{
		return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
	}

	private void Cross3(double[] des, double[] v1, double[] v2)
	{
		des[0] = v1[1] * v2[2] - v1[2] * v2[1];
		des[1] = v1[2] * v2[0] - v1[0] * v2[2];
		des[2] = v1[0] * v2[1] - v1[1] * v2[0];
	}

	private double Minor(double[] m, int r0, int r1, int r2, int c0, int c1, int c2)
	{
		return m[r0 + c0 * 4] * (m[r1 + c1 * 4] * m[r2 + c2 * 4] - m[r2 + c1 * 4] * m[r1 + c2 * 4]) - m[r0 + c1 * 4] * (m[r1 + c0 * 4] * m[r2 + c2 * 4] - m[r2 + c0 * 4] * m[r1 + c2 * 4]) + m[r0 + c2 * 4] * (m[r1 + c0 * 4] * m[r2 + c1 * 4] - m[r2 + c0 * 4] * m[r1 + c1 * 4]);
	}

	private double Determinant(double[] m)
	{
		return m[0] * Minor(m, 1, 2, 3, 1, 2, 3) - m[4] * Minor(m, 1, 2, 3, 0, 2, 3) + m[8] * Minor(m, 1, 2, 3, 0, 1, 3) - m[12] * Minor(m, 1, 2, 3, 0, 1, 2);
	}

	private void Adjoint(double[] des, double[] m)
	{
		des[0] = Minor(m, 1, 2, 3, 1, 2, 3);
		des[4] = 0.0 - Minor(m, 0, 2, 3, 1, 2, 3);
		des[8] = Minor(m, 0, 1, 3, 1, 2, 3);
		des[12] = 0.0 - Minor(m, 0, 1, 2, 1, 2, 3);
		des[1] = 0.0 - Minor(m, 1, 2, 3, 0, 2, 3);
		des[5] = Minor(m, 0, 2, 3, 0, 2, 3);
		des[9] = 0.0 - Minor(m, 0, 1, 3, 0, 2, 3);
		des[13] = Minor(m, 0, 1, 2, 0, 2, 3);
		des[2] = Minor(m, 1, 2, 3, 0, 1, 3);
		des[6] = 0.0 - Minor(m, 0, 2, 3, 0, 1, 3);
		des[10] = Minor(m, 0, 1, 3, 0, 1, 3);
		des[14] = 0.0 - Minor(m, 0, 1, 2, 0, 1, 3);
		des[3] = 0.0 - Minor(m, 1, 2, 3, 0, 1, 2);
		des[7] = Minor(m, 0, 2, 3, 0, 1, 2);
		des[11] = 0.0 - Minor(m, 0, 1, 3, 0, 1, 2);
		des[15] = Minor(m, 0, 1, 2, 0, 1, 2);
	}

	private void Inverse(double[] des, double[] m)
	{
		Adjoint(des, m);
		double num = 1.0 / Determinant(m);
		for (int i = 0; i < 16; i++)
		{
			des[i] *= num;
		}
	}

	private void SetRow(int i, double[] m, double[] v)
	{
		m[i] = v[0];
		m[i + 4] = v[1];
		m[i + 8] = v[2];
		m[i + 12] = v[3];
	}
}
