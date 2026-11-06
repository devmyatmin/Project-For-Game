using System;
using UnityEngine;

namespace AmplifyImpostors;

public static class Vector2Ex
{
	public static float Cross(this Vector2 O, Vector2 A, Vector2 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
	}

	public static float TriangleArea(this Vector2 O, Vector2 A, Vector2 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs((A.x - B.x) * (O.y - A.y) - (A.x - O.x) * (B.y - A.y)) * 0.5f;
	}

	public static float TriangleArea(this Vector3 O, Vector3 A, Vector3 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs((A.x - B.x) * (O.y - A.y) - (A.x - O.x) * (B.y - A.y)) * 0.5f;
	}

	public static Vector2[] ConvexHull(Vector2[] P)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (P.Length > 1)
		{
			int num = P.Length;
			int num2 = 0;
			Vector2[] array = new Vector2[2 * num];
			Comparison<Vector2> comparison = (Vector2 a, Vector2 b) =>
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				return (a.x == b.x) ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x);
			};
			Array.Sort(P, comparison);
			for (int num3 = 0; num3 < num; num3++)
			{
				while (num2 >= 2 && Cross(P[num3], array[num2 - 2], array[num2 - 1]) <= 0f)
				{
					num2--;
				}
				array[num2++] = P[num3];
			}
			int num4 = num - 2;
			int num5 = num2 + 1;
			while (num4 >= 0)
			{
				while (num2 >= num5 && Cross(P[num4], array[num2 - 2], array[num2 - 1]) <= 0f)
				{
					num2--;
				}
				array[num2++] = P[num4];
				num4--;
			}
			if (num2 > 1)
			{
				Array.Resize(ref array, num2 - 1);
			}
			return array;
		}
		if (P.Length <= 1)
		{
			return P;
		}
		return null;
	}

	public static Vector2[] ScaleAlongNormals(Vector2[] P, float scaleAmount)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] array = new Vector2[P.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int num = i - 1;
			int num2 = i + 1;
			if (i == 0)
			{
				num = P.Length - 1;
			}
			if (i == P.Length - 1)
			{
				num2 = 0;
			}
			Vector2 val = P[i] - P[num];
			Vector2 val2 = P[i] - P[num2];
			Vector2 val3 = val.normalized + val2.normalized;
			Vector2 normalized = val3.normalized;
			array[i] = normalized;
		}
		for (int j = 0; j < array.Length; j++)
		{
			P[j] += array[j] * scaleAmount;
		}
		return P;
	}

	private static Vector2[] ReduceLeastSignificantVertice(Vector2[] P)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int smallestIndex = 0;
		int num2 = 0;
		Vector2 val = Vector2.zero;
		for (int i = 0; i < P.Length; i++)
		{
			int num3 = i + 1;
			int num4 = i + 2;
			int num5 = i + 3;
			if (num3 >= P.Length)
			{
				num3 -= P.Length;
			}
			if (num4 >= P.Length)
			{
				num4 -= P.Length;
			}
			if (num5 >= P.Length)
			{
				num5 -= P.Length;
			}
			Vector2 intersectionPointCoordinates = GetIntersectionPointCoordinates(P[i], P[num3], P[num4], P[num5]);
			if (i == 0)
			{
				num = TriangleArea(intersectionPointCoordinates, P[num3], P[num4]);
				if (OutsideBounds(intersectionPointCoordinates) > 0f)
				{
					num += OutsideBounds(intersectionPointCoordinates) * 1f;
				}
				smallestIndex = num3;
				num2 = num4;
				val = intersectionPointCoordinates;
				continue;
			}
			float num6 = TriangleArea(intersectionPointCoordinates, P[num3], P[num4]);
			if (OutsideBounds(intersectionPointCoordinates) > 0f)
			{
				num6 += OutsideBounds(intersectionPointCoordinates) * 1f;
			}
			if (num6 < num && OutsideBounds(intersectionPointCoordinates) <= 0f)
			{
				num = num6;
				smallestIndex = num3;
				num2 = num4;
				val = intersectionPointCoordinates;
			}
		}
		P[num2] = val;
		return Array.FindAll(P, (Vector2 x) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Array.IndexOf(P, x) != smallestIndex;
		});
	}

	public static Vector2[] ReduceVertices(Vector2[] P, int maxVertices)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (maxVertices == 4)
		{
			Rect val = new Rect(P[0].x, P[0].y, 0f, 0f);
			for (int i = 0; i < P.Length; i++)
			{
				val.xMin = Mathf.Min(val.xMin, P[i].x);
				val.xMax = Mathf.Max(val.xMax, P[i].x);
				val.yMin = Mathf.Min(val.yMin, P[i].y);
				val.yMax = Mathf.Max(val.yMax, P[i].y);
			}
			P = new Vector2[4]
			{
				new Vector2(val.xMin, val.yMin),
				new Vector2(val.xMax, val.yMin),
				new Vector2(val.xMax, val.yMax),
				new Vector2(val.xMin, val.yMax)
			};
		}
		else
		{
			int num = Math.Max(0, P.Length - maxVertices);
			for (int j = 0; j < num; j++)
			{
				P = ReduceLeastSignificantVertice(P);
			}
		}
		return P;
	}

	private static Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		float num = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
		if (num == 0f)
		{
			return (Vector2.Lerp(A2, B1, 0.5f) - Vector2.one * 0.5f) * 1000f + Vector2.one * 500f;
		}
		float num2 = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / num;
		return new Vector2(B1.x + (B2.x - B1.x) * num2, B1.y + (B2.y - B1.y) * num2);
	}

	private static float OutsideBounds(Vector2 P)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		P -= Vector2.one * 0.5f;
		float num = Mathf.Clamp01(Mathf.Abs(P.y) - 0.5f);
		return Mathf.Clamp01(Mathf.Abs(P.x) - 0.5f) + num;
	}
}
