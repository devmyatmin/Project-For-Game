using System;
using UnityEngine;

[Serializable]
public class FilmicCurve
{
	[SerializeField]
	public float m_BlackPoint;

	[SerializeField]
	public float m_WhitePoint = 1f;

	[SerializeField]
	public float m_CrossOverPoint = 0.5f;

	[SerializeField]
	public float m_ToeStrength;

	[SerializeField]
	public float m_ShoulderStrength;

	[SerializeField]
	public float m_LuminositySaturationPoint = 0.95f;

	public bool m_UseLegacyCurve;

	public Vector4 m_ToeCoef;

	public Vector4 m_ShoulderCoef;

	public float m_k;

	public float ComputeK(float t, float c, float b, float s, float w)
	{
		float num = (1f - t) * (c - b);
		float num2 = (1f - s) * (w - c) + (1f - t) * (c - b);
		return num / num2;
	}

	public float Toe(float x, float t, float c, float b, float s, float w, float k)
	{
		float num = m_ToeCoef.x * x;
		float num2 = m_ToeCoef.y * x;
		return (num + m_ToeCoef.z) / (num2 + m_ToeCoef.w);
	}

	public float Shoulder(float x, float t, float c, float b, float s, float w, float k)
	{
		float num = m_ShoulderCoef.x * x;
		float num2 = m_ShoulderCoef.y * x;
		return (num + m_ShoulderCoef.z) / (num2 + m_ShoulderCoef.w) + k;
	}

	public float Graph(float x, float t, float c, float b, float s, float w, float k)
	{
		if (x <= m_CrossOverPoint)
		{
			return Toe(x, t, c, b, s, w, k);
		}
		return Shoulder(x, t, c, b, s, w, k);
	}

	public void StoreK()
	{
		m_k = ComputeK(m_ToeStrength, m_CrossOverPoint, m_BlackPoint, m_ShoulderStrength, m_WhitePoint);
	}

	public void ComputeShaderCoefficientsLegacy(float t, float c, float b, float s, float w, float k)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		float num = k * (1f - t);
		float num2 = k * (1f - t) * (0f - b);
		float num3 = 0f - t;
		float num4 = c - (1f - t) * b;
		m_ToeCoef = new Vector4(num, num3, num2, num4);
		float num5 = 1f - k;
		float num6 = (1f - k) * (0f - c);
		float num7 = (1f - s) * w - c;
		m_ShoulderCoef = new Vector4(num5, s, num6, num7);
	}

	private float ToRange(float x)
	{
		return (x + 1f) * 0.5f;
	}

	public float GraphHD(float _lOll01ll00)
	{
		float num = Mathf.Log10(_lOll01ll00);
		float x = m_ToeCoef.x;
		float w = m_ToeCoef.w;
		float x2 = m_ShoulderCoef.x;
		float num2 = m_ToeCoef.y / (1f + Mathf.Exp(m_ToeCoef.z * (num - x)));
		float num3 = 1f - m_ShoulderCoef.y / (1f + Mathf.Exp(m_ShoulderCoef.z * (num - x2)));
		float num4 = m_ShoulderCoef.w * (num + w);
		float num5 = Mathf.Clamp01((num - x) / (x2 - x));
		float num6 = 0f;
		float num7 = ((num < x) ? num2 : num4);
		float num8 = ((num > x2) ? num3 : num4);
		if (x > x2)
		{
			return Mathf.Lerp(num8, num7, num5);
		}
		return Mathf.Lerp(num7, num8, num5);
	}

	public void ComputeShaderCoefficientsHD(float _001l0l01Ol, float _ll00Ol1O0O, float _ll00l0O1O1, float _OlOO100ll0, float _OOlO1l0ll0, float _OOO1O00O0l)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		float num = (1f - ToRange(_001l0l01Ol)) / 1.25f;
		float num2 = ToRange(_OlOO100ll0);
		float num3 = Mathf.Clamp01(_ll00Ol1O0O * 0.5f + 0.5f);
		float num4 = 1f - num;
		float num5 = 1f - num2;
		float num6 = 0.18f;
		float num7 = -0.5f * Mathf.Log((1f + (num6 / num4 - 1f)) / (1f - (num6 / num4 - 1f))) * (num4 / num3) + Mathf.Log10(num6);
		float num8 = num4 / num3 - num7;
		float num9 = num2 / num3 - num8;
		float num10 = 2f * num4;
		float num11 = -2f * num3 / num4;
		float num12 = 2f * num5;
		float num13 = 2f * num3 / num5;
		m_ToeCoef = new Vector4(num7, num10, num11, num8);
		m_ShoulderCoef = new Vector4(num9, num12, num13, num3);
	}

	public void UpdateCoefficients()
	{
		if (m_UseLegacyCurve)
		{
			StoreK();
			ComputeShaderCoefficientsLegacy(m_ToeStrength, m_CrossOverPoint, m_BlackPoint, m_ShoulderStrength, m_WhitePoint, m_k);
		}
		else
		{
			ComputeShaderCoefficientsHD(m_ToeStrength, m_CrossOverPoint, m_BlackPoint, m_ShoulderStrength, m_WhitePoint, m_k);
		}
	}
}
