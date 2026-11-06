using UnityEngine;

namespace AmplifyImpostors;

public static class RenderTextureEx
{
	public static RenderTexture GetTemporary(RenderTexture renderTexture)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return RenderTexture.GetTemporary(((Texture)renderTexture).width, ((Texture)renderTexture).height, renderTexture.depth, renderTexture.format);
	}
}
