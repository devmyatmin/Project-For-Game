using System.Collections.Generic;
using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class RTSettings
{
	public string name = "";

	public int width = 1;

	public int height = 1;

	public int depth;

	public int ansioLevel = 1;

	public bool mipmaps;

	public bool randomWrite;

	public RenderTextureReadWrite readWrite;

	public TextureWrapMode wrap = (TextureWrapMode)1;

	public FilterMode filer = (FilterMode)1;

	public RenderTextureFormat format;

	public List<RenderTextureFormat> fallbackFormats = new List<RenderTextureFormat>();

	public RTSettings()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
	}
}
