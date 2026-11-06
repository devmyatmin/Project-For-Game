using UnityEngine;

namespace Ceto;

public static class DrawCameraFrustum
{
	private static Vector4[] box = new Vector4[8]
	{
		new Vector4(-1f, -1f, -1f, 1f),
		new Vector4(-1f, 1f, -1f, 1f),
		new Vector4(1f, 1f, -1f, 1f),
		new Vector4(1f, -1f, -1f, 1f),
		new Vector4(-1f, -1f, 1f, 1f),
		new Vector4(-1f, 1f, 1f, 1f),
		new Vector4(1f, 1f, 1f, 1f),
		new Vector4(1f, -1f, 1f, 1f)
	};

	private static int[,] indexs = new int[12, 2]
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

	public static void DrawFrustum(Matrix4x4 projectionView, Color col)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Vector4[] array = new Vector4[8];
		Matrix4x4 inverse = projectionView.inverse;
		for (int i = 0; i < 8; i++)
		{
			array[i] = inverse * box[i];
			ref Vector4 reference = ref array[i];
			reference /= array[i].w;
		}
		Gizmos.color = col;
		for (int j = 0; j < indexs.GetLength(0); j++)
		{
			Gizmos.DrawLine(Vector4.op_Implicit(array[indexs[j, 0]]), Vector4.op_Implicit(array[indexs[j, 1]]));
		}
	}

	static DrawCameraFrustum()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
	}
}
