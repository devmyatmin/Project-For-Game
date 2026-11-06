using System;
using Beam.Rendering;
using UnityEngine;

[Serializable]
public class ImposterTexture
{
	public int size;

	public RenderTexture texture;

	public DynamicImpostor owner;

	public int x;

	public int y;

	public int h;

	public int w;

	public int createdTime;

	public int lastUsedTime;

	public float getMemoryAmount()
	{
		return 12f * (float)(size * size) / 1048576f;
	}
}
