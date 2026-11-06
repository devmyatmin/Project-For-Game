using System.Collections.Generic;
using UnityEngine;

namespace AmplifyImpostors;

public class Triangulator
{
	private List<Vector2> m_points = new List<Vector2>();

	public List<Vector2> Points => m_points;

	public Triangulator(Vector2[] points)
	{
		m_points = new List<Vector2>(points);
	}

	public Triangulator(Vector2[] points, bool invertY = true)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (invertY)
		{
			m_points = new List<Vector2>();
			for (int i = 0; i < points.Length; i++)
			{
				m_points.Add(new Vector2(points[i].x, 1f - points[i].y));
			}
		}
		else
		{
			m_points = new List<Vector2>(points);
		}
	}

	public int[] Triangulate()
	{
		List<int> list = new List<int>();
		int count = m_points.Count;
		if (count < 3)
		{
			return list.ToArray();
		}
		int[] array = new int[count];
		if (Area() > 0f)
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = count - 1 - j;
			}
		}
		int num = count;
		int num2 = 2 * num;
		int num3 = 0;
		int num4 = num - 1;
		while (num > 2)
		{
			if (num2-- <= 0)
			{
				return list.ToArray();
			}
			int num5 = num4;
			if (num <= num5)
			{
				num5 = 0;
			}
			num4 = num5 + 1;
			if (num <= num4)
			{
				num4 = 0;
			}
			int num6 = num4 + 1;
			if (num <= num6)
			{
				num6 = 0;
			}
			if (Snip(num5, num4, num6, num, array))
			{
				int item = array[num5];
				int item2 = array[num4];
				int item3 = array[num6];
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				num3++;
				int num7 = num4;
				for (int k = num4 + 1; k < num; k++)
				{
					array[num7] = array[k];
					num7++;
				}
				num--;
				num2 = 2 * num;
			}
		}
		list.Reverse();
		return list.ToArray();
	}

	private float Area()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		int count = m_points.Count;
		float num = 0f;
		int index = count - 1;
		int num2 = 0;
		while (num2 < count)
		{
			Vector2 val = m_points[index];
			Vector2 val2 = m_points[num2];
			num += val.x * val2.y - val2.x * val.y;
			index = num2++;
		}
		return num * 0.5f;
	}

	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = m_points[V[u]];
		Vector2 val2 = m_points[V[v]];
		Vector2 val3 = m_points[V[w]];
		if (Mathf.Epsilon > (val2.x - val.x) * (val3.y - val.y) - (val2.y - val.y) * (val3.x - val.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector2 pt = m_points[V[i]];
				if (InsideTriangle(pt, val, val2, val3))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool InsideTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Vector2Ex.Cross(pt, v1, v2) < 0f;
		bool flag2 = Vector2Ex.Cross(pt, v2, v3) < 0f;
		bool flag3 = Vector2Ex.Cross(pt, v3, v1) < 0f;
		if (flag == flag2)
		{
			return flag2 == flag3;
		}
		return false;
	}
}
