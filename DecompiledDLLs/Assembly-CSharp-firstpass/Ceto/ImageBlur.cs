using UnityEngine;

namespace Ceto;

public class ImageBlur
{
	public enum BLUR_MODE
	{
		OFF = 0,
		NO_DOWNSAMPLE = 1,
		DOWNSAMPLE_2 = 2,
		DOWNSAMPLE_4 = 4
	}

	private Vector2[] m_offsets = new Vector2[4];

	public Material m_blurMaterial;

	public BLUR_MODE BlurMode { get; set; }

	public int BlurIterations { get; set; }

	public float BlurSpread { get; set; }

	public ImageBlur(Shader blurShader)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		BlurIterations = 1;
		BlurSpread = 0.6f;
		BlurMode = BLUR_MODE.DOWNSAMPLE_2;
		if ((Object)(object)blurShader != (Object)null)
		{
			m_blurMaterial = new Material(blurShader);
		}
	}

	public void Blur(RenderTexture source)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		int blurMode = (int)BlurMode;
		if (BlurIterations > 0 && (Object)(object)m_blurMaterial != (Object)null && blurMode > 0)
		{
			int num = ((Texture)source).width / blurMode;
			int num2 = ((Texture)source).height / blurMode;
			RenderTexture val = RenderTexture.GetTemporary(num, num2, 0, source.format, (RenderTextureReadWrite)0);
			DownSample(source, val);
			for (int i = 0; i < BlurIterations; i++)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, source.format, (RenderTextureReadWrite)0);
				FourTapCone(val, temporary, i);
				RenderTexture.ReleaseTemporary(val);
				val = temporary;
			}
			Graphics.Blit((Texture)(object)val, source);
			RenderTexture.ReleaseTemporary(val);
		}
	}

	private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * BlurSpread;
		m_offsets[0].x = 0f - num;
		m_offsets[0].y = 0f - num;
		m_offsets[1].x = 0f - num;
		m_offsets[1].y = num;
		m_offsets[2].x = num;
		m_offsets[2].y = num;
		m_offsets[3].x = num;
		m_offsets[3].y = 0f - num;
		Graphics.BlitMultiTap((Texture)(object)source, dest, m_blurMaterial, m_offsets);
	}

	private void DownSample(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		m_offsets[0].x = 0f - num;
		m_offsets[0].y = 0f - num;
		m_offsets[1].x = 0f - num;
		m_offsets[1].y = num;
		m_offsets[2].x = num;
		m_offsets[2].y = num;
		m_offsets[3].x = num;
		m_offsets[3].y = 0f - num;
		Graphics.BlitMultiTap((Texture)(object)source, dest, m_blurMaterial, m_offsets);
	}
}
