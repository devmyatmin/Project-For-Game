using System;
using System.Collections.Generic;
using System.Linq;
using Beam;
using IslandGen;
using UnityEngine;

public static class WorldTools
{
	private enum GeneratorType
	{
		Perlin,
		Normalise,
		Voronoi,
		Smooth
	}

	public class PerlinNoise2D
	{
		private double[,] noiseValues;

		private float amplitude = 1f;

		private int frequency = 1;

		public float Amplitude => amplitude;

		public int Frequency => frequency;

		public PerlinNoise2D(int freq, float _amp, System.Random _wSeed)
		{
			noiseValues = new double[freq, freq];
			amplitude = _amp;
			frequency = freq;
			for (int i = 0; i < freq; i++)
			{
				for (int j = 0; j < freq; j++)
				{
					noiseValues[i, j] = _wSeed.NextDouble();
				}
			}
		}

		public double getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, double Px, double Py)
		{
			double pa = interpolate(noiseValues[_xa % Frequency, _ya % frequency], noiseValues[_xb % Frequency, _ya % frequency], Px);
			double pb = interpolate(noiseValues[_xa % Frequency, _yb % frequency], noiseValues[_xb % Frequency, _yb % frequency], Px);
			return interpolate(pa, pb, Py);
		}

		private double interpolate(double Pa, double Pb, double Px)
		{
			double num = Px * 3.1415927410125732;
			double num2 = (double)(1f - Mathf.Cos((float)num)) * 0.5;
			return Pa * (1.0 - num2) + Pb * num2;
		}
	}

	private const int heightmapRes = 257;

	private static GeneratorType generatorType = GeneratorType.Perlin;

	private static int perlinSeed = 0;

	private static int perlinFrequency = 4;

	private static float perlinAmplitude = 1f;

	private static int perlinOctaves = 8;

	private static float perlinBlend = 1f;

	private static float normaliseMin = 0f;

	private static float normaliseMax = 1f;

	private static float normaliseBlend = 1f;

	public static float smoothBlend = 1f;

	public static int smoothIterations;

	private const float Island_Base_Height = 0.66f;

	public static Zone.BiomeType GenerateZoneBiome(Zone.BiomeType biomeType, int seed)
	{
		int num = new System.Random(seed).Next(0, 100);
		Zone.BiomeType biomeType2 = biomeType;
		if (biomeType == Zone.BiomeType.RANDOM)
		{
			biomeType2 = ((num < 50) ? Zone.BiomeType.ISLAND : Zone.BiomeType.ISLAND_SMALL);
		}
		if (biomeType2 == Zone.BiomeType.RANDOM)
		{
			Debug.LogError("Biome Error :: Biome is Random");
		}
		return biomeType2;
	}

	public static float[,] GenerateZoneHeightmap(Zone.BiomeType biomeType, int randomSeed)
	{
		float[,] array = new float[257, 257];
		switch (biomeType)
		{
		case Zone.BiomeType.ISLAND:
			array = GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType);
			break;
		case Zone.BiomeType.ISLAND_SMALL:
			array = GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType);
			break;
		case Zone.BiomeType.ISLAND_ROCK:
			array = GENERATE_ISLAND_HEIGHTMAP(randomSeed, biomeType);
			break;
		case Zone.BiomeType.SHALLOW:
			array = PerlinGenerator(array, randomSeed, 6, 0.5f, 6, 1f);
			array = NormalizeHeightmap(array, 0.44f, 0.55f, 1f, biomeType);
			break;
		case Zone.BiomeType.MEDIUM:
			array = PerlinGenerator(array, randomSeed, 12, 0.5f, 6, 1f);
			array = NormalizeHeightmap(array, 0.3f, 0.45f, 1f, biomeType);
			break;
		case Zone.BiomeType.DEEP_SEA:
			array = PerlinGenerator(array, randomSeed, 4, 0.9f, 4, 1f);
			array = NormalizeHeightmap(array, 0.1f, 0.2f, 1f, biomeType);
			break;
		}
		return array;
	}

	public static float[,] GENERATE_ISLAND_HEIGHTMAP(int seed, Zone.BiomeType biome, bool soilMapGeneration = false)
	{
		System.Random random = new System.Random(seed);
		int num = 140;
		int num2 = 200;
		int num3 = num2;
		switch (biome)
		{
		case Zone.BiomeType.ISLAND_SMALL:
			num3 = random.Next(num, num + 20);
			break;
		case Zone.BiomeType.ISLAND:
			num3 = random.Next(num2 - 20, num2);
			break;
		}
		float num4 = 0.685f;
		float num5 = 0.705f;
		float num6 = (float)(num3 - num) / (float)(num2 - num);
		float maxHeight = num4 + (num5 - num4) * num6;
		int numPoints = num3;
		Island island = new Island(num3);
		island.NewIsland(Island.IslandTypes.Radial, Island.PointTypes.Random, numPoints, seed, num3);
		int num7 = 257;
		float num8 = (int)((float)(250 - num3) * 0.5f);
		float[,] array = new float[num7, num7];
		if (!soilMapGeneration)
		{
			for (int i = 0; i < num7; i++)
			{
				for (int j = 0; j < num7; j++)
				{
					array[i, j] = 0.66f;
				}
			}
		}
		foreach (Center center in island.Centers)
		{
			if (center.Biome == Island.Biome.Ocean)
			{
				continue;
			}
			foreach (Center neighbor in center.Neighbors)
			{
				if (neighbor.Biome == Island.Biome.Lake)
				{
					_ = center.Biome;
					_ = 1;
				}
				List<Vector3> list = new List<Vector3>();
				Edge edge = island.LookUpEdgeFromCenter(center, neighbor);
				if (edge != null && edge.V0 != null && edge.V1 != null)
				{
					Corner v = edge.V0;
					Corner v2 = edge.V1;
					list.Add(new Vector3(v.Point.x, v.Elevation, v.Point.y));
					list.Add(new Vector3(v2.Point.x, v2.Elevation, v2.Point.y));
					list.Add(new Vector3(center.Point.x, center.Elevation, center.Point.y));
					if (soilMapGeneration)
					{
						array.FillCellHeightmap(list.ToArray(), (int)num8);
					}
					else
					{
						array.FillCellHeightmap(list.ToArray(), (int)num8, lake: false, 0.66f, maxHeight);
					}
					list = null;
				}
			}
		}
		if (!soilMapGeneration)
		{
			array = StitchTerrainEdges(array);
			array = PerlinGenerator(array, seed, 24, 1f, 6, 0.02f);
			foreach (Edge edge2 in island.Edges)
			{
				if (edge2 != null && edge2.V0 != null && edge2.V1 != null)
				{
					Corner v3 = edge2.V0;
					Corner v4 = edge2.V1;
					if (edge2.River > 0)
					{
						array.HeightmapRiver((int)v3.Point.x, (int)v3.Point.y, (int)v4.Point.x, (int)v4.Point.y);
						array.HeightmapRiver((int)v3.Point.x + 1, (int)v3.Point.y, (int)v4.Point.x + 1, (int)v4.Point.y);
						array.HeightmapRiver((int)v3.Point.x, (int)v3.Point.y + 1, (int)v4.Point.x, (int)(v4.Point.y + 1f));
					}
				}
			}
			array = SmoothTerrain(array, 2, 0.7f, biome);
		}
		island = null;
		return array;
	}

	private static float[,] StitchTerrainEdges(float[,] heightmap)
	{
		int num = 257;
		Color[] pixels = (Resources.Load("Terrain/Mask_Island_Stitch") as Texture2D).GetPixels();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (heightmap[i, j] <= 0.66f)
				{
					float r = pixels[i * num + j].r;
					heightmap[i, j] -= 1f * r;
				}
			}
		}
		return heightmap;
	}

	public static Texture2D CreateHeightMapTexture(TerrainData terrain)
	{
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D texture2D = new Texture2D(heightmapResolution, heightmapResolution2, TextureFormat.ARGB32, mipChain: false, linear: false);
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				float num = terrain.GetHeight(i, j) + -100f;
				float num2 = Mathf.Lerp(0f, 1f, Mathf.Clamp01((num + 6f) / 5f));
				Color color = ((num > 0f) ? new Color(1f, 1f, 1f, 1f) : new Color(num2, num2, num2, num2));
				texture2D.SetPixel(i, j, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D CreateShoreMaskTexture(TerrainData terrain)
	{
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D texture2D = new Texture2D(heightmapResolution, heightmapResolution2, TextureFormat.ARGB32, mipChain: false, linear: false);
		int num = -100;
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				Color color = ((terrain.GetHeight(i, j) + (float)num > 0.3f) ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 0f));
				texture2D.SetPixel(i, j, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private static float[,] GenerateTerrain(float[,] heightmap, Zone.BiomeType biome = Zone.BiomeType.RANDOM)
	{
		int num = 257;
		int num2 = num;
		float[,] array = (float[,])heightmap.Clone();
		float[,] array2 = (float[,])heightmap.Clone();
		switch (generatorType)
		{
		case GeneratorType.Perlin:
		{
			array2 = GeneratePerlin(array2, new Vector2(num, num2));
			for (int k = 0; k < num2; k++)
			{
				for (int l = 0; l < num; l++)
				{
					float num5 = array[l, k];
					float num6 = array2[l, k] * perlinBlend + num5 * (1f - perlinBlend);
					array[l, k] = num6;
				}
			}
			break;
		}
		case GeneratorType.Normalise:
		{
			array2 = Normalise(array2, new Vector2(num, num2));
			for (int m = 0; m < num2; m++)
			{
				for (int n = 0; n < num; n++)
				{
					float num7 = array[n, m];
					float num8 = array2[n, m] * normaliseBlend + num7 * (1f - normaliseBlend);
					array[n, m] = num8;
				}
			}
			break;
		}
		case GeneratorType.Smooth:
		{
			array2 = smooth(array2, new Vector2(num, num2));
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num3 = array[j, i];
					float num4 = array2[j, i] * smoothBlend + num3 * (1f - smoothBlend);
					array[j, i] = num4;
				}
			}
			break;
		}
		default:
			return null;
		}
		array2 = null;
		return array;
	}

	private static float[,] GeneratePerlin(float[,] heightmap, Vector2 arraySize)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		System.Random wSeed = new System.Random(perlinSeed);
		PerlinNoise2D[] array = new PerlinNoise2D[perlinOctaves];
		int num3 = perlinFrequency;
		float num4 = 1f;
		for (int i = 0; i < perlinOctaves; i++)
		{
			array[i] = new PerlinNoise2D(num3, num4, wSeed);
			num3 *= 2;
			num4 /= 2f;
		}
		for (int i = 0; i < perlinOctaves; i++)
		{
			double num5 = (float)num / (float)array[i].Frequency;
			double num6 = (float)num2 / (float)array[i].Frequency;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					int num7 = (int)((double)j / num5);
					int xb = num7 + 1;
					int num8 = (int)((double)k / num6);
					int yb = num8 + 1;
					double interpolatedPoint = array[i].getInterpolatedPoint(num7, xb, num8, yb, (double)j / num5 - (double)num7, (double)k / num6 - (double)num8);
					heightmap[j, k] += (float)(interpolatedPoint * (double)array[i].Amplitude);
				}
			}
		}
		float num9 = normaliseMin;
		float num10 = normaliseMax;
		float num11 = normaliseBlend;
		normaliseMin = 0f;
		normaliseMax = 1f;
		normaliseBlend = 1f;
		Normalise(heightmap, arraySize);
		normaliseMin = num9;
		normaliseMax = num10;
		normaliseBlend = num11;
		for (int l = 0; l < num; l++)
		{
			for (int m = 0; m < num2; m++)
			{
				heightmap[l, m] *= perlinAmplitude;
			}
		}
		for (int i = 0; i < perlinOctaves; i++)
		{
			array[i] = null;
		}
		array = null;
		return heightmap;
	}

	public static float[,] PerlinGenerator(float[,] heightmap, int seed, int frequency, float amplitude, int octaves, float blend)
	{
		generatorType = GeneratorType.Perlin;
		perlinFrequency = frequency;
		perlinAmplitude = amplitude;
		perlinOctaves = octaves;
		perlinBlend = blend;
		perlinSeed = seed;
		return GenerateTerrain(heightmap);
	}

	private static float[,] Normalise(float[,] heightmap, Vector2 arraySize)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float num3 = 0f;
		float num4 = 1f;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num5 = heightmap[j, i];
				if (num5 < num4)
				{
					num4 = num5;
				}
				else if (num5 > num3)
				{
					num3 = num5;
				}
			}
		}
		float num6 = num3 - num4;
		float num7 = normaliseMax - normaliseMin;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num8 = (heightmap[j, i] - num4) / num6 * num7;
				heightmap[j, i] = normaliseMin + num8;
				if (heightmap[j, i] > 1f)
				{
					heightmap[j, i] = 1f;
				}
				if (heightmap[j, i] < 0f)
				{
					heightmap[j, i] = 0f;
				}
			}
		}
		return heightmap;
	}

	public static float[,] NormalizeHeightmap(float[,] heightmap, float minHeight, float maxHeight, float blend, Zone.BiomeType biome = Zone.BiomeType.RANDOM)
	{
		generatorType = GeneratorType.Normalise;
		normaliseMin = minHeight;
		normaliseMax = maxHeight;
		normaliseBlend = blend;
		return GenerateTerrain(heightmap, biome);
	}

	public static float[,] SmoothTerrain(float[,] heightmap, int iterations, float blend, Zone.BiomeType biome = Zone.BiomeType.RANDOM)
	{
		generatorType = GeneratorType.Smooth;
		smoothIterations = iterations;
		smoothBlend = blend;
		return GenerateTerrain(heightmap, biome);
	}

	private static float[,] smooth(float[,] heightMap, Vector2 arraySize)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		for (int i = 0; i < smoothIterations; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num3;
				int num4;
				int num5;
				if (j == 0)
				{
					num3 = 2;
					num4 = 0;
					num5 = 0;
				}
				else if (j == num2 - 1)
				{
					num3 = 2;
					num4 = -1;
					num5 = 1;
				}
				else
				{
					num3 = 3;
					num4 = -1;
					num5 = 1;
				}
				for (int k = 0; k < num; k++)
				{
					int num6;
					int num7;
					int num8;
					if (k == 0)
					{
						num6 = 2;
						num7 = 0;
						num8 = 0;
					}
					else if (k == num - 1)
					{
						num6 = 2;
						num7 = -1;
						num8 = 1;
					}
					else
					{
						num6 = 3;
						num7 = -1;
						num8 = 1;
					}
					float num9 = 0f;
					int num10 = 0;
					for (int l = 0; l < num3; l++)
					{
						for (int m = 0; m < num6; m++)
						{
							if (m == num8 || l == num5)
							{
								float num11 = heightMap[k + m + num7, j + l + num4];
								num9 += num11;
								num10++;
							}
						}
					}
					float num12 = num9 / (float)num10;
					heightMap[k + num8 + num7, j + num5 + num4] = num12;
				}
			}
		}
		return heightMap;
	}

	public static void FillCellHeightmap(this float[,] heightmap, Vector3[] points, int offset, bool lake = false, float minHeight = 0f, float maxHeight = 1f)
	{
		int num = (int)points.Max((Vector3 p) => p.z);
		int num2 = (int)points.Min((Vector3 p) => p.z);
		int num3 = (int)points.Min((Vector3 p) => p.x);
		int num4 = (int)points.Max((Vector3 p) => p.x);
		int num5;
		int num6 = (num5 = points.Count());
		float[] array = points.Select((Vector3 p) => p.z).ToArray();
		float[] array2 = points.Select((Vector3 p) => p.x).ToArray();
		int[] array3 = new int[num6];
		for (int i = num2; i <= num; i++)
		{
			int num7 = 0;
			int num8 = num5 - 1;
			int j;
			for (j = 0; j < num5; j++)
			{
				if ((array[j] < (float)i && array[num8] >= (float)i) || (array[num8] < (float)i && array[j] >= (float)i))
				{
					array3[num7++] = (int)(array2[j] + ((float)i - array[j]) / (array[num8] - array[j]) * (array2[num8] - array2[j]));
				}
				num8 = j;
			}
			j = 0;
			while (j < num7 - 1)
			{
				if (array3[j] > array3[j + 1])
				{
					int num9 = array3[j];
					array3[j] = array3[j + 1];
					array3[j + 1] = num9;
					if (j != 0)
					{
						j--;
					}
				}
				else
				{
					j++;
				}
			}
			for (j = 0; j < num7 && array3[j] < num4; j += 2)
			{
				if (array3[j + 1] > num3)
				{
					if (array3[j] < num3)
					{
						array3[j] = num3;
					}
					if (array3[j + 1] > num4)
					{
						array3[j + 1] = num4;
					}
					for (num8 = array3[j]; num8 < array3[j + 1]; num8++)
					{
						Vector3 p2 = points[0];
						Vector3 p3 = points[1];
						Vector3 p4 = points[2];
						float heightAtPoint = Triangulation.GetHeightAtPoint(new Vector3(num8, 0f, i), new Triangle(p2, p3, p4));
						float num10 = minHeight + (maxHeight - minHeight) * heightAtPoint;
						heightmap[num8 + offset, i + offset] = num10;
					}
				}
			}
		}
	}

	public static void HeightmapRiver(this float[,] heightmap, int x0, int y0, int x1, int y1)
	{
		int num = y1 - y0;
		int num2 = x1 - x0;
		int num3;
		if (num < 0)
		{
			num = -num;
			num3 = -1;
		}
		else
		{
			num3 = 1;
		}
		int num4;
		if (num2 < 0)
		{
			num2 = -num2;
			num4 = -1;
		}
		else
		{
			num4 = 1;
		}
		num <<= 1;
		num2 <<= 1;
		float num5 = 0f;
		float num6 = 0.0065f;
		float num7 = 0.675f;
		if (heightmap[x0, y0] > num7)
		{
			heightmap[x0, y0] -= num6;
		}
		if (num2 > num)
		{
			num5 = num - (num2 >> 1);
			while (Mathf.Abs(x0 - x1) > 1)
			{
				if (num5 >= 0f)
				{
					y0 += num3;
					num5 -= (float)num2;
				}
				x0 += num4;
				num5 += (float)num;
				if (heightmap[x0, y0] > num7)
				{
					heightmap[x0, y0] -= num6;
				}
			}
			return;
		}
		num5 = num2 - (num >> 1);
		while (Mathf.Abs(y0 - y1) > 1)
		{
			if (num5 >= 0f)
			{
				x0 += num4;
				num5 -= (float)num;
			}
			y0 += num3;
			num5 += (float)num2;
			if (heightmap[x0, y0] > num7)
			{
				heightmap[x0, y0] -= num6;
			}
		}
	}
}
