using System.Collections.Generic;
using System.Linq;
using Beam;
using IslandGen;
using UnityEngine;

public class IslandDebugTexture
{
	public void AttachTexture(Island island)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		int num = 256;
		int num2 = 256;
		int num3 = (int)((250f - island.Size) * 0.5f);
		Texture2D val = new Texture2D(num, num2);
		val.SetPixels(Enumerable.Repeat<Color>(Color.magenta, num * num2).ToArray());
		foreach (Center center in island.Centers)
		{
			List<Vector2> list = new List<Vector2>();
			foreach (Center neighbor in center.Neighbors)
			{
				List<Vector3> list2 = new List<Vector3>();
				Edge edge = island.LookUpEdgeFromCenter(center, neighbor);
				if (edge == null || edge.D0 == null || edge.D1 == null)
				{
					continue;
				}
				Center d = edge.D0;
				Center d2 = edge.D1;
				list.Add(d.Point);
				list.Add(d2.Point);
				list.Add(center.Point);
				if (edge != null && edge.V0 != null && edge.V1 != null)
				{
					list2.Add(new Vector3(center.Point.x, center.Elevation, center.Point.y));
					list2.Add(new Vector3(edge.V0.Point.x, edge.V0.Elevation, edge.V0.Point.y));
					list2.Add(new Vector3(edge.V1.Point.x, edge.V1.Elevation, edge.V1.Point.y));
				}
				foreach (Vector3 item in list2)
				{
					_ = item;
					FillTexturePolygon(val, list2.ToArray(), Island.BiomeProperties.Colors[center.Biome], num3);
				}
			}
			val.SetPixel((int)center.Point.x + num3, (int)center.Point.y + num3, Color.red);
		}
		val.Apply();
		GameObject val2 = GameObject.Find("DebugVoronoi");
		if (Object.op_Implicit((Object)(object)val2))
		{
			val2.GetComponent<Renderer>().material.mainTexture = (Texture)(object)val;
		}
	}

	private void DrawLine(Texture2D texture, float x0, float y0, float x1, float y1, Color color)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		DrawLines(texture, (int)x0, (int)y0, (int)x1, (int)y1, color);
	}

	private void DrawLines(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
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
		tex.SetPixel(x0, y0, col);
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
				tex.SetPixel(x0, y0, col);
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
			tex.SetPixel(x0, y0, col);
		}
	}

	private void FillTexturePolygon(Texture2D texture, Vector3[] points, Color color, int offset)
	{
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)points.Max((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.z;
		});
		int num2 = (int)points.Min((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.z;
		});
		int num3 = (int)points.Min((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.x;
		});
		int num4 = (int)points.Max((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.x;
		});
		int num6;
		int num5 = (num6 = points.Count());
		float[] array = points.Select((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.z;
		}).ToArray();
		float[] array2 = points.Select((Vector3 val) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return val.x;
		}).ToArray();
		int[] array3 = new int[num5];
		for (int num7 = num2; num7 <= num; num7++)
		{
			int num8 = 0;
			int num9 = num6 - 1;
			int num10;
			for (num10 = 0; num10 < num6; num10++)
			{
				if ((array[num10] < (float)num7 && array[num9] >= (float)num7) || (array[num9] < (float)num7 && array[num10] >= (float)num7))
				{
					array3[num8++] = (int)(array2[num10] + ((float)num7 - array[num10]) / (array[num9] - array[num10]) * (array2[num9] - array2[num10]));
				}
				num9 = num10;
			}
			num10 = 0;
			while (num10 < num8 - 1)
			{
				if (array3[num10] > array3[num10 + 1])
				{
					int num11 = array3[num10];
					array3[num10] = array3[num10 + 1];
					array3[num10 + 1] = num11;
					if (num10 != 0)
					{
						num10--;
					}
				}
				else
				{
					num10++;
				}
			}
			for (num10 = 0; num10 < num8 && array3[num10] < num4; num10 += 2)
			{
				if (array3[num10 + 1] > num3)
				{
					if (array3[num10] < num3)
					{
						array3[num10] = num3;
					}
					if (array3[num10 + 1] > num4)
					{
						array3[num10 + 1] = num4;
					}
					for (num9 = array3[num10]; num9 < array3[num10 + 1]; num9++)
					{
						Vector3 p = points[0];
						Vector3 p2 = points[1];
						Vector3 p3 = points[2];
						Triangulation.GetHeightAtPoint(new Vector3((float)num9, 0f, (float)num7), new Triangle(p, p2, p3));
						texture.SetPixel(num9 + offset, num7 + offset, new Color(color.r, color.g, color.b, 1f));
					}
				}
			}
		}
	}

	public static Texture2D RenderSoilMapImageIcon(float[,] soilmap, int heightmapRes)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(256, 256);
		for (int i = 0; i < heightmapRes - 1; i++)
		{
			for (int j = 0; j < heightmapRes - 1; j++)
			{
				float num = soilmap[j, i];
				Color val2 = Color.Lerp(Color.yellow, Color.green, num);
				if (num == 0f)
				{
					val2 = Color.black;
				}
				val.SetPixel(j, i, val2);
			}
		}
		return val;
	}
}
