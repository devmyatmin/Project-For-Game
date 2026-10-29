using System;
using System.Collections.Generic;
using SharpNeatLib.Maths;
using UnityEngine;

public static class ZonePositionGenerator
{
	public static Vector2[] GeneratePoints(int WORLD_SEED, float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
	{
		float num = radius / Mathf.Sqrt(2f);
		int[,] array = new int[Mathf.CeilToInt(sampleRegionSize.x / num), Mathf.CeilToInt(sampleRegionSize.y / num)];
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		list2.Add(sampleRegionSize / 2f);
		FastRandom fastRandom = new FastRandom(WORLD_SEED);
		while (list2.Count > 0)
		{
			int index = fastRandom.Next(0, list2.Count);
			Vector2 vector = list2[index];
			bool flag = false;
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float f = (float)fastRandom.NextDouble() * MathF.PI * 2f;
				Vector2 vector2 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
				Vector2 vector3 = vector + vector2 * (fastRandom.Next((int)(radius * 100f), (int)(1.25f * radius * 100f)) / 100);
				if (i == 0)
				{
					vector3 = vector;
				}
				if (IsValid(vector3, sampleRegionSize, num, radius, list, array))
				{
					list.Add(vector3);
					list2.Add(vector3);
					array[(int)(vector3.x / num), (int)(vector3.y / num)] = list.Count;
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
			float num2 = sampleRegionSize.x * 0.5f;
			array2[j] = new Vector2(list[j].x - num2, list[j].y - num2);
		}
		return array2;
	}

	private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
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
					if (num7 != -1 && (candidate - points[num7]).sqrMagnitude < radius * radius)
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}
}
