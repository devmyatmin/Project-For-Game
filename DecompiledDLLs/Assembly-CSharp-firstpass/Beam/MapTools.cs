using UnityEngine;

namespace Beam;

public static class MapTools
{
	public static Texture2D CreateHeightmapTexture(float[,] heightData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(257, 257, (TextureFormat)5, false, false);
		for (int i = 0; i < 257; i++)
		{
			for (int j = 0; j < 257; j++)
			{
				float num = heightData[i, j];
				float num2 = 1f;
				if (num * 150f - (float)Mathf.Abs(-100) < 0f)
				{
					num = 0f;
					num2 = 0f;
				}
				Color val2 = new Color(num, num, num, num2);
				val.SetPixel(j, i, val2);
			}
		}
		val.Apply();
		return val;
	}

	public static float[,] GetHeightsFromTexture(Texture2D heightmap)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float[,] array = new float[257, 257];
		Color[] pixels = heightmap.GetPixels();
		for (int i = 0; i < 257; i++)
		{
			for (int j = 0; j < 257; j++)
			{
				Color val = pixels[j * 257 + i];
				array[j, i] = val.grayscale;
			}
		}
		return array;
	}

	public static string FormatNameForSave(string mapName)
	{
		return mapName.Replace(" ", "_");
	}
}
