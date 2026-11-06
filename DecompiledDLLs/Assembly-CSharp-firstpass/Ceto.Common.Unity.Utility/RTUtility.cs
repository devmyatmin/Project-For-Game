using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public static class RTUtility
{
	public static void Blit(RenderTexture des, Material mat, int pass = 0)
	{
		Graphics.SetRenderTarget(des);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static void Blit(RenderTexture des, Material mat, Vector3[] verts, int pass = 0)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Graphics.SetRenderTarget(des);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex(verts[0]);
		GL.TexCoord2(1f, 0f);
		GL.Vertex(verts[1]);
		GL.TexCoord2(1f, 1f);
		GL.Vertex(verts[2]);
		GL.TexCoord2(0f, 1f);
		GL.Vertex(verts[3]);
		GL.End();
		GL.PopMatrix();
	}

	public static void Blit(RenderTexture des, Material mat, Vector3[] verts, Vector2[] uvs, int pass = 0)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Graphics.SetRenderTarget(des);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord(Vector2.op_Implicit(uvs[0]));
		GL.Vertex(verts[0]);
		GL.TexCoord(Vector2.op_Implicit(uvs[1]));
		GL.Vertex(verts[1]);
		GL.TexCoord(Vector2.op_Implicit(uvs[2]));
		GL.Vertex(verts[2]);
		GL.TexCoord(Vector2.op_Implicit(uvs[3]));
		GL.Vertex(verts[3]);
		GL.End();
		GL.PopMatrix();
	}

	public static void MultiTargetBlit(IList<RenderTexture> des, Material mat, int pass = 0)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		RenderBuffer[] array = new RenderBuffer[des.Count];
		for (int i = 0; i < des.Count; i++)
		{
			array[i] = des[i].colorBuffer;
		}
		Graphics.SetRenderTarget(array, des[0].depthBuffer);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Graphics.SetRenderTarget(des_rb, des_db);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static void ClearColor(RenderTexture tex, Color col)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)tex == (Object)null) && SystemInfo.SupportsRenderTextureFormat(tex.format))
		{
			Graphics.SetRenderTarget(tex);
			GL.Clear(false, true, col);
		}
	}

	public static void Release(RenderTexture tex)
	{
		if (!((Object)(object)tex == (Object)null))
		{
			tex.Release();
		}
	}

	public static void Release(IList<RenderTexture> texList)
	{
		if (texList == null)
		{
			return;
		}
		int count = texList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!((Object)(object)texList[i] == (Object)null))
			{
				texList[i].Release();
			}
		}
	}

	public static void ReleaseAndDestroy(RenderTexture tex)
	{
		if (!((Object)(object)tex == (Object)null))
		{
			tex.Release();
			Object.Destroy((Object)(object)tex);
		}
	}

	public static void ReleaseAndDestroy(IList<RenderTexture> texList)
	{
		if (texList == null)
		{
			return;
		}
		int count = texList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!((Object)(object)texList[i] == (Object)null))
			{
				texList[i].Release();
				Object.Destroy((Object)(object)texList[i]);
			}
		}
	}

	public static void ReleaseTemporary(RenderTexture tex)
	{
		if (!((Object)(object)tex == (Object)null))
		{
			RenderTexture.ReleaseTemporary(tex);
		}
	}

	public static void ReleaseTemporary(IList<RenderTexture> texList)
	{
		if (texList == null)
		{
			return;
		}
		int count = texList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!((Object)(object)texList[i] == (Object)null))
			{
				RenderTexture.ReleaseTemporary(texList[i]);
			}
		}
	}

	private static RenderTextureFormat CheckFormat(RTSettings setting)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		RenderTextureFormat val = setting.format;
		if (!SystemInfo.SupportsRenderTextureFormat(val))
		{
			Debug.Log((object)("System does not support " + ((object)val/*cast due to constrained. prefix*/).ToString() + " render texture format."));
			bool flag = false;
			int count = setting.fallbackFormats.Count;
			for (int i = 0; i < count; i++)
			{
				if (SystemInfo.SupportsRenderTextureFormat(setting.fallbackFormats[i]))
				{
					val = setting.fallbackFormats[i];
					Debug.Log((object)("Found fallback format: " + ((object)val/*cast due to constrained. prefix*/).ToString()));
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				throw new InvalidOperationException("Could not find fallback render texture format");
			}
		}
		return val;
	}

	public static RenderTexture CreateRenderTexture(RTSettings setting)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected Obj, but got Unknown
		if (setting == null)
		{
			throw new NullReferenceException("RTSettings is null");
		}
		RenderTextureFormat val = CheckFormat(setting);
		return new RenderTexture(setting.width, setting.height, setting.depth, val, setting.readWrite)
		{
			name = setting.name,
			wrapMode = setting.wrap,
			filterMode = setting.filer,
			useMipMap = setting.mipmaps,
			anisoLevel = setting.ansioLevel,
			enableRandomWrite = setting.randomWrite
		};
	}

	public static RenderTexture CreateTemporyRenderTexture(RTSettings setting)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (setting == null)
		{
			throw new NullReferenceException("RTSettings is null");
		}
		RenderTextureFormat val = CheckFormat(setting);
		RenderTexture temporary = RenderTexture.GetTemporary(setting.width, setting.height, setting.depth, val, setting.readWrite);
		((Object)temporary).name = setting.name;
		((Texture)temporary).wrapMode = setting.wrap;
		((Texture)temporary).filterMode = setting.filer;
		((Texture)temporary).anisoLevel = setting.ansioLevel;
		return temporary;
	}
}
