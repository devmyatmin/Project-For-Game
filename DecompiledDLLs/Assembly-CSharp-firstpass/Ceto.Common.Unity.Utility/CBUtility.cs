using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public static class CBUtility
{
	private static string[,] readNames2D = new string[4, 3]
	{
		{ "read2DC1", "_Tex2D", "_Buffer2DC1" },
		{ "read2DC2", "_Tex2D", "_Buffer2DC2" },
		{ "read2DC3", "_Tex2D", "_Buffer2DC3" },
		{ "read2DC4", "_Tex2D", "_Buffer2DC4" }
	};

	public static void ReadFromRenderTexture(RenderTexture tex, int channels, ComputeBuffer buffer, ComputeShader readData)
	{
		if ((Object)(object)tex == (Object)null)
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - RenderTexture is null");
			return;
		}
		if (buffer == null)
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - buffer is null");
			return;
		}
		if ((Object)(object)readData == (Object)null)
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - Computer shader is null");
			return;
		}
		if (channels < 1 || channels > 4)
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}
		if (!tex.IsCreated())
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - tex has not been created (Call Create() on tex)");
			return;
		}
		int num = -1;
		int num2 = 1;
		num = readData.FindKernel(readNames2D[channels - 1, 0]);
		readData.SetTexture(num, readNames2D[channels - 1, 1], (Texture)(object)tex);
		readData.SetBuffer(num, readNames2D[channels - 1, 2], buffer);
		if (num == -1)
		{
			Debug.Log((object)"CBUtility::ReadFromRenderTexture - could not find kernels");
			return;
		}
		int width = ((Texture)tex).width;
		int height = ((Texture)tex).height;
		readData.SetInt("_Width", width);
		readData.SetInt("_Height", height);
		readData.SetInt("_Depth", num2);
		int num3 = ((width % 8 != 0) ? 1 : 0);
		int num4 = ((height % 8 != 0) ? 1 : 0);
		int num5 = ((num2 % 8 != 0) ? 1 : 0);
		readData.Dispatch(num, Mathf.Max(1, width / 8 + num3), Mathf.Max(1, height / 8 + num4), Mathf.Max(1, num2 / 8 + num5));
	}

	public static void ReadSingleFromRenderTexture(RenderTexture tex, float x, float y, float z, ComputeBuffer buffer, ComputeShader readData, bool useBilinear)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tex == (Object)null)
		{
			Debug.Log((object)"CBUtility::ReadSingleFromRenderTexture - RenderTexture is null");
			return;
		}
		if (buffer == null)
		{
			Debug.Log((object)"CBUtility::ReadSingleFromRenderTexture - buffer is null");
			return;
		}
		if ((Object)(object)readData == (Object)null)
		{
			Debug.Log((object)"CBUtility::ReadSingleFromRenderTexture - Computer shader is null");
			return;
		}
		if (!tex.IsCreated())
		{
			Debug.Log((object)"CBUtility::ReadSingleFromRenderTexture - tex has not been created (Call Create() on tex)");
			return;
		}
		int num = -1;
		int num2 = 1;
		num = ((!useBilinear) ? readData.FindKernel("readSingleBilinear2D") : readData.FindKernel("readSingle2D"));
		readData.SetTexture(num, "_Tex2D", (Texture)(object)tex);
		readData.SetBuffer(num, "BufferSingle2D", buffer);
		if (num == -1)
		{
			Debug.Log((object)"CBUtility::ReadSingleFromRenderTexture - could not find kernels");
			return;
		}
		int width = ((Texture)tex).width;
		int height = ((Texture)tex).height;
		readData.SetInt("_IdxX", (int)x);
		readData.SetInt("_IdxY", (int)y);
		readData.SetInt("_IdxZ", (int)z);
		readData.SetVector("_UV", new Vector4(x / (float)(width - 1), y / (float)(height - 1), z / (float)(num2 - 1), 0f));
		readData.Dispatch(num, 1, 1, 1);
	}
}
