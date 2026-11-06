using UnityEngine;

public class FTDLut
{
	public bool Validate2DLut(Texture2D texture)
	{
		if (!Object.op_Implicit((Object)(object)texture))
		{
			return false;
		}
		if (((Texture)texture).height != Mathf.FloorToInt(Mathf.Sqrt((float)((Texture)texture).width)))
		{
			return false;
		}
		return true;
	}

	public Texture3D Allocate3DLut(Texture2D source)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		int height = ((Texture)source).height;
		return new Texture3D(height, height, height, (TextureFormat)5, false);
	}

	public void SetNeutralLUT(Texture2D destination)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		int height = ((Texture)destination).height;
		Color[] array = new Color[height * height * height];
		float num = 1f / (1f * (float)height - 1f);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < height; k++)
				{
					int num2 = height - j - 1;
					array[k * height + i + num2 * height * height] = new Color((float)i * 1f * num, (float)j * 1f * num, (float)k * 1f * num, 1f);
				}
			}
		}
		destination.SetPixels(array);
		destination.Apply();
	}

	public bool ConvertLUT(Texture2D source, Texture3D destination)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		int height = ((Texture)source).height;
		if (!Validate2DLut(source))
		{
			Debug.LogWarning((object)("The given 2D texture " + ((Object)source).name + " cannot be used as a 3D LUT."));
			return false;
		}
		Color[] pixels = source.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < height; k++)
				{
					int num = height - j - 1;
					array[i + j * height + k * height * height] = pixels[k * height + i + num * height * height];
				}
			}
		}
		destination.SetPixels(array);
		destination.Apply();
		return true;
	}
}
