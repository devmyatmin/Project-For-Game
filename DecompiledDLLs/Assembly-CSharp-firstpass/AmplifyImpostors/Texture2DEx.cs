using System;
using UnityEngine;

namespace AmplifyImpostors;

public static class Texture2DEx
{
	private static byte[] Footer = new byte[26]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 84, 82,
		85, 69, 86, 73, 83, 73, 79, 78, 45, 88,
		70, 73, 76, 69, 46, 0
	};

	public static byte[] EncodeToTGA(this Texture2D tex, bool withAlpha = true)
	{
		int num = (withAlpha ? 4 : 3);
		byte[] rawTextureData = tex.GetRawTextureData();
		int num2 = rawTextureData.Length;
		int width = ((Texture)tex).width;
		int height = ((Texture)tex).height;
		int num3 = num * (num2 / num);
		int num4 = Footer.Length;
		byte[] array = new byte[18 + num4 + num3];
		int num5 = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 2;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = 0;
		array[num5++] = (byte)(width & 0xFF);
		array[num5++] = (byte)((width >> 8) & 0xFF);
		array[num5++] = (byte)(height & 0xFF);
		array[num5++] = (byte)((height >> 8) & 0xFF);
		array[num5++] = (byte)(8 * num);
		array[num5++] = 0;
		Array.Copy(rawTextureData, 0, array, num5, num2);
		num5 += num2;
		Array.Copy(Footer, 0, array, num5, num4);
		return array;
	}
}
