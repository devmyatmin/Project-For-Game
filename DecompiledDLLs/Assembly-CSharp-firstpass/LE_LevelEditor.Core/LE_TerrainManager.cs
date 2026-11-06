using LE_LevelEditor.Events;
using UnityEngine;

namespace LE_LevelEditor.Core;

public class LE_TerrainManager
{
	private TerrainData m_terrainData;

	public TerrainData TerrainData => m_terrainData;

	public LE_TerrainManager(TerrainData p_terrainData)
	{
		m_terrainData = p_terrainData;
	}

	public void SmoothHeight(float p_amount, int p_neighbourCount, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, bool p_isDirected, float p_angle)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		float num = m_terrainData.heightmapResolution - 1;
		float num2 = m_terrainData.heightmapResolution - 1;
		float p_relativeBrushSize2 = Mathf.Clamp01(p_relativeBrushSize + (float)(p_neighbourCount - 1) / Mathf.Max(num, num2));
		GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize2, p_relativeLocalLocation, num, num2, out var o_minX, out var o_maxX, out var o_minY, out var o_maxY, out var _, out var _);
		GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, num, num2, out var o_minX2, out var o_maxX2, out var o_minY2, out var o_maxY2, out var o_relBrushMinX2, out var o_relBrushMinY2);
		float[,] heights = m_terrainData.GetHeights(o_minY, o_minX, o_maxY - o_minY + 1, o_maxX - o_minX + 1);
		int num3 = o_minX2 - o_minX;
		int num4 = o_minY2 - o_minY;
		int num5 = heights.GetLength(0) - 1;
		int num6 = heights.GetLength(1) - 1;
		int num7 = Mathf.Min(num3 + o_maxX2 - o_minX2, num5);
		int num8 = Mathf.Min(num4 + o_maxY2 - o_minY2, num6);
		int num9 = (p_neighbourCount - 1) / 2;
		for (int i = num4; i <= num8; i++)
		{
			int num10;
			int num11;
			if (i == 0)
			{
				num10 = num9 + 1;
				num11 = 0;
			}
			else if (i == num6)
			{
				num10 = num9 + 1;
				num11 = -num9;
			}
			else if (i - num9 < 0)
			{
				int num12 = num9 - i;
				num10 = p_neighbourCount - num12;
				num11 = -num9 + num12;
			}
			else if (i + num9 >= num6)
			{
				int num13 = num9 + i - num6;
				num10 = p_neighbourCount - num13;
				num11 = -num9;
			}
			else
			{
				num10 = p_neighbourCount;
				num11 = -num9;
			}
			for (int j = num3; j <= num7; j++)
			{
				int num14;
				int num15;
				if (j == 0)
				{
					num14 = num9 + 1;
					num15 = 0;
				}
				else if (j == num5)
				{
					num14 = num9 + 1;
					num15 = -num9;
				}
				else if (j - num9 < 0)
				{
					int num16 = num9 - j;
					num14 = p_neighbourCount - num16;
					num15 = -num9 + num16;
				}
				else if (j + num9 >= num5)
				{
					int num17 = num9 + j - num5;
					num14 = p_neighbourCount - num17;
					num15 = -num9;
				}
				else
				{
					num14 = p_neighbourCount;
					num15 = -num9;
				}
				float num18 = 0f;
				int num19 = 0;
				for (int k = 0; k < num10; k++)
				{
					for (int l = 0; l < num14; l++)
					{
						if (p_isDirected)
						{
							int num20 = l + num15;
							int num21 = k + num11;
							if (num20 != 0 || num21 != 0)
							{
								Vector2 val = new Vector2((float)num20, (float)num21);
								Vector2 normalized = val.normalized;
								float num22;
								if (normalized.y >= 0f)
								{
									num22 = 57.29578f * Mathf.Acos(normalized.x);
								}
								else
								{
									num22 = 57.29578f * Mathf.Asin(normalized.y);
									if (normalized.x < 0f)
									{
										num22 = -90f - (90f + num22);
									}
								}
								if (Mathf.Abs(Mathf.DeltaAngle(p_angle, num22)) > 5f && Mathf.Abs(Mathf.DeltaAngle(p_angle, num22 + 180f)) > 5f)
								{
									continue;
								}
							}
						}
						float num23 = heights[j + l + num15, i + k + num11];
						num18 += num23;
						num19++;
					}
				}
				float num24 = num18 / (float)num19;
				float num25 = heights[j, i];
				float num26 = ((float)(j + o_minX) / num - o_relBrushMinX2) / p_relativeBrushSize;
				float num27 = ((float)(i + o_minY) / num2 - o_relBrushMinY2) / p_relativeBrushSize;
				float num28 = p_amount * p_alphaBrushTexture.GetPixelBilinear(num27, num26).a;
				heights[j, i] = num25 * (1f - num28) + num24 * num28;
				heights[j, i] = ClampHeightBounds(heights[j, i], o_minX + j, o_minY + i);
			}
		}
		m_terrainData.SetHeights(o_minY, o_minX, heights);
		LE_EventInterface.LevelDataChanged(this);
	}

	public void ChangeHeight(float p_delta, float p_targetHeight, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		ChangeHeightInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, out var o_minX, out var o_maxX, out var o_minY, out var o_maxY, out var o_heights, out var o_relBrushMinX, out var o_relBrushMinY, out var o_heightmapMaxX, out var o_heightmapMaxY);
		int num = Mathf.Min(o_maxX - o_minX, o_heights.GetLength(0) - 1);
		int num2 = Mathf.Min(o_maxY - o_minY, o_heights.GetLength(1) - 1);
		for (int i = 0; i <= num; i++)
		{
			for (int j = 0; j <= num2; j++)
			{
				float num3 = o_heights[i, j];
				if (Mathf.Abs(num3 - p_targetHeight) > 0.0001f)
				{
					float num4 = ((float)(i + o_minX) / o_heightmapMaxX - o_relBrushMinX) / p_relativeBrushSize;
					float num5 = ((float)(j + o_minY) / o_heightmapMaxY - o_relBrushMinY) / p_relativeBrushSize;
					float num6 = p_delta * p_alphaBrushTexture.GetPixelBilinear(num5, num4).a;
					float num7 = ((p_targetHeight - num3 > 0f) ? 1f : (-1f));
					if (Mathf.Sign(p_targetHeight - num3) != Mathf.Sign(p_targetHeight - (num3 + num7 * num6)))
					{
						o_heights[i, j] = p_targetHeight;
					}
					else
					{
						o_heights[i, j] += num7 * num6;
					}
				}
				o_heights[i, j] = ClampHeightBounds(o_heights[i, j], o_minX + i, o_minY + j);
			}
		}
		m_terrainData.SetHeights(o_minY, o_minX, o_heights);
		LE_EventInterface.LevelDataChanged(this);
	}

	private float ClampHeightBounds(float heightToClamp, int x, int y)
	{
		float num = 20f;
		float num2 = 257f;
		if ((!((float)x > num) || !((float)x < num2 - num) || !((float)y > num) || !((float)y < num2 - num)) && heightToClamp > 0.65f)
		{
			heightToClamp = 0.65f;
		}
		return heightToClamp;
	}

	public void ChangeHeight(float p_delta, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		ChangeHeightInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, out var o_minX, out var o_maxX, out var o_minY, out var o_maxY, out var o_heights, out var o_relBrushMinX, out var o_relBrushMinY, out var o_heightmapMaxX, out var o_heightmapMaxY);
		int num = Mathf.Min(o_maxX - o_minX, o_heights.GetLength(0) - 1);
		int num2 = Mathf.Min(o_maxY - o_minY, o_heights.GetLength(1) - 1);
		for (int i = 0; i <= num; i++)
		{
			for (int j = 0; j <= num2; j++)
			{
				float num3 = ((float)(i + o_minX) / o_heightmapMaxX - o_relBrushMinX) / p_relativeBrushSize;
				float num4 = ((float)(j + o_minY) / o_heightmapMaxY - o_relBrushMinY) / p_relativeBrushSize;
				float a = p_alphaBrushTexture.GetPixelBilinear(num4, num3).a;
				o_heights[i, j] += p_delta * a;
				o_heights[i, j] = ClampHeightBounds(o_heights[i, j], o_minX + i, o_minY + j);
			}
		}
		m_terrainData.SetHeights(o_minY, o_minX, o_heights);
		LE_EventInterface.LevelDataChanged(this);
	}

	private void ChangeHeightInternal(Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, out int o_minX, out int o_maxX, out int o_minY, out int o_maxY, out float[,] o_heights, out float o_relBrushMinX, out float o_relBrushMinY, out float o_heightmapMaxX, out float o_heightmapMaxY)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		o_heightmapMaxX = m_terrainData.heightmapResolution - 1;
		o_heightmapMaxY = m_terrainData.heightmapResolution - 1;
		GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, o_heightmapMaxX, o_heightmapMaxY, out o_minX, out o_maxX, out o_minY, out o_maxY, out o_relBrushMinX, out o_relBrushMinY);
		o_heights = m_terrainData.GetHeights(o_minY, o_minX, o_maxY - o_minY + 1, o_maxX - o_minX + 1);
	}

	private void GetAffectedAreaInternal(Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, float p_maxX, float p_maxY, out int o_minX, out int o_maxX, out int o_minY, out int o_maxY, out float o_relBrushMinX, out float o_relBrushMinY)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		o_minX = 0;
		o_maxX = 0;
		o_minY = 0;
		o_maxY = 0;
		o_relBrushMinX = p_relativeLocalLocation.x - p_relativeBrushSize * 0.5f;
		float num = p_relativeLocalLocation.x + p_relativeBrushSize * 0.5f;
		o_relBrushMinY = p_relativeLocalLocation.y - p_relativeBrushSize * 0.5f;
		float num2 = p_relativeLocalLocation.y + p_relativeBrushSize * 0.5f;
		if ((Object)(object)p_alphaBrushTexture == (Object)null)
		{
			Debug.LogError((object)"LE_TerrainManager: Parameter 'p_alphaBrushTexture' is null!");
			return;
		}
		if (p_relativeBrushSize != Mathf.Clamp01(p_relativeBrushSize))
		{
			Debug.LogError((object)("LE_TerrainManager: Parameter 'p_relativeBrushSize' is out of bounds expected range [0,1], but was '" + p_relativeBrushSize + "'!"));
			return;
		}
		if (p_relativeLocalLocation.x != Mathf.Clamp01(p_relativeLocalLocation.x) || p_relativeLocalLocation.y != Mathf.Clamp01(p_relativeLocalLocation.y))
		{
			Vector2 val = p_relativeLocalLocation;
			Debug.LogError((object)("LE_TerrainManager: Parameter 'p_relativeLocalLocation' is out of bounds expected range ([0,1],[0,1]), but was '" + ((object)val/*cast due to constrained. prefix*/).ToString() + "'!"));
			return;
		}
		o_minX = Mathf.Clamp(Mathf.FloorToInt(p_maxX * o_relBrushMinX), 0, (int)p_maxX);
		o_maxX = Mathf.Clamp(Mathf.CeilToInt(p_maxX * num), 0, (int)p_maxX);
		o_minY = Mathf.Clamp(Mathf.FloorToInt(p_maxY * o_relBrushMinY), 0, (int)p_maxY);
		o_maxY = Mathf.Clamp(Mathf.CeilToInt(p_maxY * num2), 0, (int)p_maxY);
	}
}
