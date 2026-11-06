using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SE Natural Bloom and Dirty Lens")]
public class SENaturalBloomAndDirtyLens : MonoBehaviour
{
	[Range(0f, 0.4f)]
	public float bloomIntensity = 0.05f;

	public Shader shader;

	private Material material;

	public Texture2D lensDirtTexture;

	[Range(0f, 0.95f)]
	public float lensDirtIntensity = 0.05f;

	private bool isSupported;

	private float blurSize = 4f;

	public bool inputIsHDR;

	private void Start()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected Obj, but got Unknown
		isSupported = true;
		if (!Object.op_Implicit((Object)(object)material))
		{
			material = new Material(shader);
		}
		if (!SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)2))
		{
			isSupported = false;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)material))
		{
			Object.DestroyImmediate((Object)(object)material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		if (!isSupported)
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		if (!Object.op_Implicit((Object)(object)material))
		{
			material = new Material(shader);
		}
		((Object)material).hideFlags = (HideFlags)61;
		material.SetFloat("_BloomIntensity", Mathf.Exp(bloomIntensity) - 1f);
		material.SetFloat("_LensDirtIntensity", Mathf.Exp(lensDirtIntensity) - 1f);
		((Texture)source).filterMode = (FilterMode)1;
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height, 0, source.format);
		Graphics.Blit((Texture)(object)source, temporary, material, 4);
		int num = ((Texture)source).width / 2;
		int num2 = ((Texture)source).height / 2;
		RenderTexture val = temporary;
		float num3 = 1f;
		int num4 = 2;
		for (int i = 0; i < 6; i++)
		{
			RenderTexture val2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
			((Texture)val2).filterMode = (FilterMode)1;
			Graphics.Blit((Texture)(object)val, val2, material, 1);
			val = val2;
			num3 = ((i <= 1) ? 0.5f : 1f);
			if (i == 2)
			{
				num3 = 0.75f;
			}
			for (int j = 0; j < num4; j++)
			{
				material.SetFloat("_BlurSize", (blurSize * 0.5f + (float)j) * num3);
				RenderTexture temporary2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
				((Texture)temporary2).filterMode = (FilterMode)1;
				Graphics.Blit((Texture)(object)val2, temporary2, material, 2);
				RenderTexture.ReleaseTemporary(val2);
				val2 = temporary2;
				temporary2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
				((Texture)temporary2).filterMode = (FilterMode)1;
				Graphics.Blit((Texture)(object)val2, temporary2, material, 3);
				RenderTexture.ReleaseTemporary(val2);
				val2 = temporary2;
			}
			switch (i)
			{
			case 0:
				material.SetTexture("_Bloom0", (Texture)(object)val2);
				break;
			case 1:
				material.SetTexture("_Bloom1", (Texture)(object)val2);
				break;
			case 2:
				material.SetTexture("_Bloom2", (Texture)(object)val2);
				break;
			case 3:
				material.SetTexture("_Bloom3", (Texture)(object)val2);
				break;
			case 4:
				material.SetTexture("_Bloom4", (Texture)(object)val2);
				break;
			case 5:
				material.SetTexture("_Bloom5", (Texture)(object)val2);
				break;
			}
			RenderTexture.ReleaseTemporary(val2);
			num /= 2;
			num2 /= 2;
		}
		material.SetTexture("_LensDirt", (Texture)(object)lensDirtTexture);
		Graphics.Blit((Texture)(object)temporary, destination, material, 0);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
