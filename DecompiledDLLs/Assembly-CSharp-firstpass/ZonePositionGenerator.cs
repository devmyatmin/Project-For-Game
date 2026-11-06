using System;
using System.Collections.Generic;
using SharpNeatLib.Maths;
using UnityEngine;

public static class ZonePositionGenerator
{
	public static Vector2[] GeneratePoints(int WORLD_SEED, float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		float num = radius / Mathf.Sqrt(2f);
		int[,] array = new int[Mathf.CeilToInt(sampleRegionSize.x / num), Mathf.CeilToInt(sampleRegionSize.y / num)];
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		list2.Add(sampleRegionSize / 2f);
		FastRandom fastRandom = new FastRandom(WORLD_SEED);
		while (list2.Count > 0)
		{
			int index = fastRandom.Next(0, list2.Count);
			Vector2 val = list2[index];
			bool flag = false;
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float num2 = (float)fastRandom.NextDouble() * (float)Math.PI * 2f;
				Vector2 val2 = new Vector2(Mathf.Sin(num2), Mathf.Cos(num2));
				Vector2 val3 = val + val2 * (float)(fastRandom.Next((int)(radius * 100f), (int)(1.25f * radius * 100f)) / 100);
				if (i == 0)
				{
					val3 = val;
				}
				if (IsValid(val3, sampleRegionSize, num, radius, list, array))
				{
					list.Add(val3);
					list2.Add(val3);
					array[(int)(val3.x / num), (int)(val3.y / num)] = list.Count;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list2.RemoveAt(index);
			}
		}
		Vector2[] array2 = new Vector2[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			float num3 = sampleRegionSize.x * 0.5f;
			array2[j] = new Vector2(list[j].x - num3, list[j].y - num3);
		}
		return array2;
	}

	private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (candidate.x >= 0f && candidate.x < sampleRegionSize.x && candidate.y >= 0f && candidate.y < sampleRegionSize.y)
		{
			int num = (int)(candidate.x / cellSize);
			int num2 = (int)(candidate.y / cellSize);
			int num3 = Mathf.Max(0, num - 2);
			int num4 = Mathf.Min(num + 2, grid.GetLength(0) - 1);
			int num5 = Mathf.Max(0, num2 - 2);
			int num6 = Mathf.Min(num2 + 2, grid.GetLength(1) - 1);
			for (int i = num3; i <= num4; i++)
			{
				for (int j = num5; j <= num6; j++)
				{
					int num7 = grid[i, j] - 1;
					if (num7 != -1)
					{
						Vector2 val = candidate - points[num7];
						if (val.sqrMagnitude < radius * radius)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}
