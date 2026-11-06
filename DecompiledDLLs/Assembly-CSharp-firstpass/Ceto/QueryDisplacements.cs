using System;
using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public static class QueryDisplacements
{
	public static readonly int CHANNELS = 3;

	public static readonly int GRIDS = 4;

	public static void QueryWaves(WaveQuery query, int enabled, IList<InterpolatedArray2f> displacements, QueryGridScaling scaling)
	{
		if (displacements.Count != GRIDS)
		{
			int gRIDS = GRIDS;
			throw new InvalidOperationException("Query Displacements requires a displacement buffer for each of the " + gRIDS + " grids.");
		}
		if (displacements[0].Channels != CHANNELS)
		{
			int gRIDS = CHANNELS;
			throw new InvalidOperationException("Query Displacements requires displacement buffers have " + gRIDS + " channels.");
		}
		if (query.mode != QUERY_MODE.DISPLACEMENT && query.mode != QUERY_MODE.POSITION)
		{
			return;
		}
		float num = query.posX + scaling.offset.x;
		float num2 = query.posZ + scaling.offset.z;
		if (scaling.result == null)
		{
			scaling.result = new float[CHANNELS];
		}
		switch (enabled)
		{
		case 0:
			break;
		default:
			if (query.mode != QUERY_MODE.DISPLACEMENT)
			{
				float num3 = num;
				float num4 = num2;
				float num5 = num;
				float num6 = num2;
				float num7 = query.minError;
				if (num7 < WaveQuery.MIN_ERROR)
				{
					num7 = WaveQuery.MIN_ERROR;
				}
				num7 *= num7;
				int num8 = 0;
				float num9;
				float num10;
				float num13;
				do
				{
					num5 += num - num3;
					num6 += num2 - num4;
					Sample(query.result.displacement, displacements, query.sampleSpectrum, num5, num6, scaling);
					num9 = query.result.displacement[0].x + query.result.displacement[1].x + query.result.displacement[2].x + query.result.displacement[3].x;
					num10 = query.result.displacement[0].z + query.result.displacement[1].z + query.result.displacement[2].z + query.result.displacement[3].z;
					num3 = num5 + num9;
					num4 = num6 + num10;
					num8++;
					float num11 = num - num3;
					float num12 = num2 - num4;
					num13 = num11 * num11 + num12 * num12;
				}
				while (num13 > num7 && num8 <= WaveQuery.MAX_ITERATIONS);
				query.result.height = query.result.displacement[0].y + query.result.displacement[1].y + query.result.displacement[2].y + query.result.displacement[3].y;
				query.result.displacementX = num9;
				query.result.displacementZ = num10;
				query.result.iterations = num8;
				query.result.error = num13;
				query.result.height = Mathf.Clamp(query.result.height, -40f, 40f);
				break;
			}
			goto case 1;
		case 1:
			SampleHeightOnly(query.result.displacement, displacements, query.sampleSpectrum, num, num2, scaling);
			query.result.height = query.result.displacement[0].y + query.result.displacement[1].y + query.result.displacement[2].y + query.result.displacement[3].y;
			query.result.displacementX = 0f;
			query.result.displacementZ = 0f;
			query.result.iterations = 0;
			query.result.error = 0f;
			query.result.height = Mathf.Clamp(query.result.height, -40f, 40f);
			break;
		}
	}

	private static void SampleHeightOnly(Vector3[] d, IList<InterpolatedArray2f> displacements, bool[] sample, float x, float z, QueryGridScaling scaling)
	{
		float[] result = scaling.result;
		if (sample[0] && scaling.numGrids > 0)
		{
			float x2 = x * scaling.invGridSizes.x;
			float y = z * scaling.invGridSizes.x;
			displacements[0].Get(x2, y, result);
			d[0].y = result[1] * scaling.scaleY;
		}
		if (sample[1] && scaling.numGrids > 1)
		{
			float x2 = x * scaling.invGridSizes.y;
			float y = z * scaling.invGridSizes.y;
			displacements[1].Get(x2, y, result);
			d[1].y = result[1] * scaling.scaleY;
		}
		if (sample[2] && scaling.numGrids > 2)
		{
			float x2 = x * scaling.invGridSizes.z;
			float y = z * scaling.invGridSizes.z;
			displacements[2].Get(x2, y, result);
			d[2].y = result[1] * scaling.scaleY;
		}
		if (sample[3] && scaling.numGrids > 3)
		{
			float x2 = x * scaling.invGridSizes.w;
			float y = z * scaling.invGridSizes.w;
			displacements[3].Get(x2, y, result);
			d[3].y = result[1] * scaling.scaleY;
		}
	}

	private static void Sample(Vector3[] d, IList<InterpolatedArray2f> displacements, bool[] sample, float x, float z, QueryGridScaling scaling)
	{
		float[] result = scaling.result;
		if (sample[0] && scaling.numGrids > 0)
		{
			float x2 = x * scaling.invGridSizes.x;
			float y = z * scaling.invGridSizes.x;
			displacements[0].Get(x2, y, result);
			d[0].x = result[0] * scaling.choppyness.x;
			d[0].y = result[1] * scaling.scaleY;
			d[0].z = result[2] * scaling.choppyness.x;
		}
		if (sample[1] && scaling.numGrids > 1)
		{
			float x2 = x * scaling.invGridSizes.y;
			float y = z * scaling.invGridSizes.y;
			displacements[1].Get(x2, y, result);
			d[1].x = result[0] * scaling.choppyness.y;
			d[1].y = result[1] * scaling.scaleY;
			d[1].z = result[2] * scaling.choppyness.y;
		}
		if (sample[2] && scaling.numGrids > 2)
		{
			float x2 = x * scaling.invGridSizes.z;
			float y = z * scaling.invGridSizes.z;
			displacements[2].Get(x2, y, result);
			d[2].x = result[0] * scaling.choppyness.z;
			d[2].y = result[1] * scaling.scaleY;
			d[2].z = result[2] * scaling.choppyness.z;
		}
		if (sample[3] && scaling.numGrids > 3)
		{
			float x2 = x * scaling.invGridSizes.w;
			float y = z * scaling.invGridSizes.w;
			displacements[3].Get(x2, y, result);
			d[3].x = result[0] * scaling.choppyness.w;
			d[3].y = result[1] * scaling.scaleY;
			d[3].z = result[2] * scaling.choppyness.w;
		}
	}

	public static Vector4 MaxRange(IList<InterpolatedArray2f> displacements, Vector4 choppyness, Vector2 gridScale, ICancelToken token)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		if (displacements.Count != GRIDS)
		{
			int gRIDS = GRIDS;
			throw new InvalidOperationException("Query Displacements requires a displacement buffer for each of the " + gRIDS + " grids.");
		}
		if (displacements[0].Channels != CHANNELS)
		{
			int gRIDS = CHANNELS;
			throw new InvalidOperationException("Query Displacements requires displacement buffers have " + gRIDS + " channels.");
		}
		int sX = displacements[0].SX;
		Vector3 val = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		Vector3 val2 = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3[] array = new Vector3[4] { val, val, val, val };
		Vector3[] array2 = new Vector3[4] { val2, val2, val2, val2 };
		float[] array3 = new float[CHANNELS];
		int gRIDS2 = GRIDS;
		gRIDS2 = 3;
		for (int i = 0; i < gRIDS2; i++)
		{
			float[] data = displacements[i].Data;
			for (int j = 0; j < sX; j++)
			{
				for (int k = 0; k < sX; k++)
				{
					if (token != null && token.Cancelled)
					{
						return Vector4.zero;
					}
					int num = (k + j * sX) * CHANNELS;
					array3[0] = data[num];
					array3[1] = data[num + 1];
					array3[2] = data[num + 2];
					if (array3[0] < array2[i].x)
					{
						array2[i].x = array3[0];
					}
					if (array3[0] > array[i].x)
					{
						array[i].x = array3[0];
					}
					if (array3[1] < array2[i].y)
					{
						array2[i].y = array3[1];
					}
					if (array3[1] > array[i].y)
					{
						array[i].y = array3[1];
					}
					if (array3[2] < array2[i].z)
					{
						array2[i].z = array3[2];
					}
					if (array3[2] > array[i].z)
					{
						array[i].z = array3[2];
					}
				}
			}
		}
		Vector4 zero = Vector4.zero;
		for (int l = 0; l < gRIDS2; l++)
		{
			zero.x += Mathf.Max(array[l].x, Mathf.Abs(array2[l].x)) * choppyness[l];
			zero.y += Mathf.Max(array[l].y, Mathf.Abs(array2[l].y));
			zero.z += Mathf.Max(array[l].z, Mathf.Abs(array2[l].z)) * choppyness[l];
		}
		zero.x *= gridScale.x;
		zero.y *= gridScale.y;
		zero.z *= gridScale.x;
		return zero;
	}

	public static void CopyAndCreateDisplacements(IList<InterpolatedArray2f> source, out IList<InterpolatedArray2f> des)
	{
		if (source.Count != GRIDS)
		{
			int gRIDS = GRIDS;
			throw new InvalidOperationException("Query Displacements requires a displacement buffer for each of the " + gRIDS + " grids.");
		}
		if (source[0].Channels != CHANNELS)
		{
			int gRIDS = CHANNELS;
			throw new InvalidOperationException("Query Displacements requires displacement buffers have " + gRIDS + " channels.");
		}
		int sX = source[0].SX;
		des = new InterpolatedArray2f[GRIDS];
		des[0] = new InterpolatedArray2f(source[0].Data, sX, sX, CHANNELS, wrap: true);
		des[1] = new InterpolatedArray2f(source[1].Data, sX, sX, CHANNELS, wrap: true);
		des[2] = new InterpolatedArray2f(source[2].Data, sX, sX, CHANNELS, wrap: true);
		des[3] = new InterpolatedArray2f(source[3].Data, sX, sX, CHANNELS, wrap: true);
	}

	public static void CopyDisplacements(IList<InterpolatedArray2f> source, IList<InterpolatedArray2f> des)
	{
		if (source.Count != GRIDS)
		{
			int gRIDS = GRIDS;
			throw new InvalidOperationException("Query Displacements requires a displacement buffer for each of the " + gRIDS + " grids.");
		}
		if (source[0].Channels != CHANNELS)
		{
			int gRIDS = CHANNELS;
			throw new InvalidOperationException("Query Displacements requires displacement buffers have " + gRIDS + " channels.");
		}
		des[0].Copy(source[0].Data);
		des[1].Copy(source[1].Data);
		des[2].Copy(source[2].Data);
		des[3].Copy(source[3].Data);
	}
}
