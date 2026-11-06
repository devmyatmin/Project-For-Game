using System;

namespace Ceto.Common.Containers.Interpolation;

public class InterpolatedArray2f : InterpolatedArray
{
	private float[] m_data;

	private int m_sx;

	private int m_sy;

	private int m_c;

	public float[] Data => m_data;

	public int SX => m_sx;

	public int SY => m_sy;

	public int Channels => m_c;

	public float this[int x, int y, int c]
	{
		get
		{
			return m_data[(x + y * m_sx) * m_c + c];
		}
		set
		{
			m_data[(x + y * m_sx) * m_c + c] = value;
		}
	}

	public InterpolatedArray2f(int sx, int sy, int c, bool wrap)
		: base(wrap)
	{
		m_sx = sx;
		m_sy = sy;
		m_c = c;
		m_data = new float[m_sx * m_sy * m_c];
	}

	public InterpolatedArray2f(float[] data, int sx, int sy, int c, bool wrap)
		: base(wrap)
	{
		m_sx = sx;
		m_sy = sy;
		m_c = c;
		m_data = new float[m_sx * m_sy * m_c];
		Copy(data);
	}

	public InterpolatedArray2f(float[,,] data, bool wrap)
		: base(wrap)
	{
		m_sx = data.GetLength(0);
		m_sy = data.GetLength(1);
		m_c = data.GetLength(2);
		m_data = new float[m_sx * m_sy * m_c];
		Copy(data);
	}

	public void Clear()
	{
		Array.Clear(m_data, 0, m_data.Length);
	}

	public void Copy(Array data)
	{
		Array.Copy(data, m_data, m_data.Length);
	}

	public float Get(int x, int y, int c)
	{
		return m_data[(x + y * m_sx) * m_c + c];
	}

	public void Set(int x, int y, int c, float v)
	{
		m_data[(x + y * m_sx) * m_c + c] = v;
	}

	public void Set(int x, int y, float[] v)
	{
		for (int i = 0; i < m_c; i++)
		{
			m_data[(x + y * m_sx) * m_c + i] = v[i];
		}
	}

	public void Get(int x, int y, float[] v)
	{
		for (int i = 0; i < m_c; i++)
		{
			v[i] = m_data[(x + y * m_sx) * m_c + i];
		}
	}

	public void Get(float x, float y, float[] v)
	{
		if (HalfPixelOffset)
		{
			x *= (float)m_sx;
			y *= (float)m_sy;
			x -= 0.5f;
			y -= 0.5f;
		}
		else
		{
			x *= (float)(m_sx - 1);
			y *= (float)(m_sy - 1);
		}
		float num = Math.Abs(x - (float)(int)x);
		Index(x, m_sx, out var ix, out var ix2);
		float num2 = Math.Abs(y - (float)(int)y);
		Index(y, m_sy, out var ix3, out var ix4);
		for (int i = 0; i < m_c; i++)
		{
			float num3 = m_data[(ix + ix3 * m_sx) * m_c + i] * (1f - num) + m_data[(ix2 + ix3 * m_sx) * m_c + i] * num;
			float num4 = m_data[(ix + ix4 * m_sx) * m_c + i] * (1f - num) + m_data[(ix2 + ix4 * m_sx) * m_c + i] * num;
			v[i] = num3 * (1f - num2) + num4 * num2;
		}
	}

	public float Get(float x, float y, int c)
	{
		if (HalfPixelOffset)
		{
			x *= (float)m_sx;
			y *= (float)m_sy;
			x -= 0.5f;
			y -= 0.5f;
		}
		else
		{
			x *= (float)(m_sx - 1);
			y *= (float)(m_sy - 1);
		}
		float num = Math.Abs(x - (float)(int)x);
		Index(x, m_sx, out var ix, out var ix2);
		float num2 = Math.Abs(y - (float)(int)y);
		Index(y, m_sy, out var ix3, out var ix4);
		float num3 = m_data[(ix + ix3 * m_sx) * m_c + c] * (1f - num) + m_data[(ix2 + ix3 * m_sx) * m_c + c] * num;
		float num4 = m_data[(ix + ix4 * m_sx) * m_c + c] * (1f - num) + m_data[(ix2 + ix4 * m_sx) * m_c + c] * num;
		return num3 * (1f - num2) + num4 * num2;
	}
}
