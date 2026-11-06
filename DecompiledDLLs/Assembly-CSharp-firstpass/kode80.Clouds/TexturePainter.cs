using UnityEngine;

namespace kode80.Clouds;

public class TexturePainter : ScriptableObject
{
	private Material _material;

	private void OnEnable()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		_material = new Material(Shader.Find("Hidden/kode80/TextureBrush"));
		((Object)_material).hideFlags = (HideFlags)61;
	}

	private void OnDisable()
	{
		Object.DestroyImmediate((Object)(object)_material);
		_material = null;
	}

	public void Render(Vector2 point, float radius, float coverageOpacity, float typeOpacity, bool drawCoverage, bool drawType, bool blendValues, RenderTexture target, Texture2D brushTexture = null)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		point.x *= ((Texture)target).width;
		point.y *= ((Texture)target).height;
		RenderTexture active = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)target).width, ((Texture)target).height, target.depth, target.format, (RenderTextureReadWrite)1);
		Graphics.Blit((Texture)(object)target, temporary);
		RenderTexture.active = temporary;
		float num = ((Texture)target).width;
		float num2 = ((Texture)target).height;
		float num3 = 0f;
		_material.SetTexture("_MainTex", (Texture)(object)target);
		_material.SetFloat("_CoverageOpacity", coverageOpacity);
		_material.SetFloat("_TypeOpacity", typeOpacity);
		_material.SetFloat("_ShouldDrawCoverage", drawCoverage ? 1f : 0f);
		_material.SetFloat("_ShouldDrawType", drawType ? 1f : 0f);
		_material.SetFloat("_ShouldBlendValues", blendValues ? 1f : 0f);
		if ((Object)(object)brushTexture != (Object)null)
		{
			_material.SetTexture("_BrushTexture", (Texture)(object)brushTexture);
		}
		_material.SetFloat("_BrushTextureAlpha", ((Object)(object)brushTexture == (Object)null) ? 0f : 1f);
		GL.PushMatrix();
		_material.SetPass(0);
		GL.LoadIdentity();
		GL.LoadPixelMatrix(0f, (float)((Texture)target).width, 0f, (float)((Texture)target).height);
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.MultiTexCoord2(1, (point.x - radius) / num, (point.y - radius) / num2);
		GL.Vertex(new Vector3(point.x - radius, point.y - radius, num3));
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.MultiTexCoord2(1, (point.x + radius) / num, (point.y - radius) / num2);
		GL.Vertex(new Vector3(point.x + radius, point.y - radius, num3));
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.MultiTexCoord2(1, (point.x + radius) / num, (point.y + radius) / num2);
		GL.Vertex(new Vector3(point.x + radius, point.y + radius, num3));
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.MultiTexCoord2(1, (point.x - radius) / num, (point.y + radius) / num2);
		GL.Vertex(new Vector3(point.x - radius, point.y + radius, num3));
		GL.End();
		GL.PopMatrix();
		Graphics.Blit((Texture)(object)temporary, target);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.active = active;
	}

	public void Clear(RenderTexture target)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = target;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = active;
	}
}
