using System.Collections.Generic;
using System.Linq;
using Beam;
using IslandGen;
using UnityEngine;

public class IslandDebugTexture
{
	public void AttachTexture(Island island)
	{
		int num = 256;
		int num2 = 256;
		int num3 = (int)((250f - island.Size) * 0.5f);
		Texture2D texture2D = new Texture2D(num, num2);
		texture2D.SetPixels(Enumerable.Repeat(Color.magenta, num * num2).ToArray());
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
					FillTexturePolygon(texture2D, list2.ToArray(), Island.BiomeProperties.Colors[center.Biome], num3);
				}
			}
			texture2D.SetPixel((int)center.Point.x + num3, (int)center.Point.y + num3, Color.red);
		}
		texture2D.Apply();
		GameObject gameObject = GameObject.Find("DebugVoronoi");
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
		}
	}

	private void DrawLine(Texture2D texture, float x0, float y0, float x1, float y1, Color color)
	{
		DrawLines(texture, (int)x0, (int)y0, (int)x1, (int)y1, color);
	}

	private void DrawLines(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
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
						Triangulation.GetHeightAtPoint(new Vector3(num8, 0f, i), new Triangle(p2, p3, p4));
						texture.SetPixel(num8 + offset, i + offset, new Color(color.r, color.g, color.b, 1f));
					}
				}
			}
		}
	}

	public static Texture2D RenderSoilMapImageIcon(float[,] soilmap, int heightmapRes)
	{
		Texture2D texture2D = new Texture2D(256, 256);
		for (int i = 0; i < heightmapRes - 1; i++)
		{
			for (int j = 0; j < heightmapRes - 1; j++)
			{
				float num = soilmap[j, i];
				Color color = Color.Lerp(Color.yellow, Color.green, num);
				if (num == 0f)
				{
					color = Color.black;
				}
				texture2D.SetPixel(j, i, color);
			}
		}
		return texture2D;
	}
}
