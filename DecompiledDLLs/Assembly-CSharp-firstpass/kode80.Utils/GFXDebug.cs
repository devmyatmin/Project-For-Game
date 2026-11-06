using System;
using System.Collections;
using UnityEngine;

namespace kode80.Utils;

public class GFXDebug
{
	private static GFXDebug _instance;

	private EndOfFrameShim _eofShim;

	private RenderTexture _renderTexture;

	private bool _drawQueued;

	private Material _material;

	public static GFXDebug instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GFXDebug();
			}
			return _instance;
		}
	}

	private GFXDebug()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		Camera.onPostRender = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPostRender, (Delegate?)new CameraCallback(OnPostRender));
		_material = new Material(Shader.Find("Hidden/kode80/UnlitAlphaMultiply"));
	}

	~GFXDebug()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPostRender = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPostRender, (Delegate?)new CameraCallback(OnPostRender));
	}

	public static void DrawTexture(Camera targetCamera, Texture texture)
	{
		instance.InternalDrawTexture(targetCamera, texture);
	}

	private void InternalDrawTexture(Camera targetCamera, Texture texture)
	{
		RenderTexture temporaryRenderTexture = GetTemporaryRenderTexture();
		_material.SetFloat("_AlphaMultiplier", 1f);
		Graphics.Blit(texture, temporaryRenderTexture, _material);
	}

	private void OnPostRender(Camera camera)
	{
		if ((Object)(object)_renderTexture != (Object)null && !_drawQueued)
		{
			_drawQueued = true;
			((MonoBehaviour)GetEndOfFrameShim()).StartCoroutine(DrawToScreen());
		}
	}

	private IEnumerator DrawToScreen()
	{
		yield return (object)new WaitForEndOfFrame();
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, (float)((Texture)_renderTexture).width, (float)((Texture)_renderTexture).height, 0f);
		_material.SetFloat("_AlphaMultiplier", 0.5f);
		Graphics.DrawTexture(new Rect(0f, 0f, (float)((Texture)_renderTexture).width, (float)((Texture)_renderTexture).height), (Texture)(object)_renderTexture, _material);
		GL.PopMatrix();
		RenderTexture.ReleaseTemporary(_renderTexture);
		_renderTexture = null;
		_drawQueued = false;
	}

	private RenderTexture GetTemporaryRenderTexture()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_renderTexture == (Object)null)
		{
			_renderTexture = RenderTexture.GetTemporary(Camera.main.pixelWidth, Camera.main.pixelHeight, 16);
			((Texture)_renderTexture).filterMode = (FilterMode)0;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = _renderTexture;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = active;
		}
		return _renderTexture;
	}

	private EndOfFrameShim GetEndOfFrameShim()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_eofShim == (Object)null)
		{
			_eofShim = new GameObject().AddComponent<EndOfFrameShim>();
			((Object)((Component)_eofShim).gameObject).hideFlags = (HideFlags)61;
		}
		return _eofShim;
	}
}
