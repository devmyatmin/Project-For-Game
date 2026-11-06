using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightingSystem;

[DisallowMultipleComponent]
[AddComponentMenu("")]
public class HighlighterRenderer : MonoBehaviour
{
	protected struct Data
	{
		public Material material;

		public int submeshIndex;

		public bool transparent;
	}

	private static float transparentCutoff = 0.5f;

	private const HideFlags flags = (HideFlags)28;

	private const int cullOff = 0;

	private static readonly string sRenderType = "RenderType";

	private static readonly string sOpaque = "Opaque";

	private static readonly string sTransparent = "Transparent";

	private static readonly string sTransparentCutout = "TransparentCutout";

	private static readonly string sMainTex = "_MainTex";

	private Renderer r;

	protected List<Data> data;

	protected Camera lastCamera;

	private Coroutine endOfFrame;

	private static ShaderPropertyID shaderPropertyID => HighlightingBase.shaderPropertyID;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		lastCamera = null;
		if (endOfFrame != null)
		{
			((MonoBehaviour)this).StopCoroutine(endOfFrame);
		}
	}

	private void OnWillRenderObject()
	{
		Camera current = Camera.current;
		if (HighlightingBase.IsHighlightingCamera(current))
		{
			lastCamera = current;
		}
	}

	private void OnDestroy()
	{
		if (this.data == null)
		{
			return;
		}
		int i = 0;
		for (int count = this.data.Count; i < count; i++)
		{
			Data data = this.data[i];
			if (data.transparent)
			{
				Object.Destroy((Object)(object)data.material);
			}
		}
	}

	public virtual void Initialize(Material sharedOpaqueMaterial, Shader transparentShader, bool seeThrough)
	{
		data = new List<Data>();
		r = ((Component)this).GetComponent<Renderer>();
		((Object)this).hideFlags = (HideFlags)28;
		Material[] sharedMaterials = r.sharedMaterials;
		if (sharedMaterials == null)
		{
			return;
		}
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			Material val = sharedMaterials[i];
			if (!((Object)(object)val == (Object)null))
			{
				Data item = default;
				item.material = GetHighlighterMaterial(val, sharedOpaqueMaterial, transparentShader, seeThrough, out item.transparent);
				item.submeshIndex = i;
				data.Add(item);
			}
		}
	}

	protected Material GetHighlighterMaterial(Material sourceMat, Material sharedOpaqueMaterial, Shader transparentShader, bool seeThrough, out bool transparent)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected Obj, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected Obj, but got Unknown
		Material val = null;
		string tag = sourceMat.GetTag(sRenderType, true, sOpaque);
		if (tag == sTransparent || tag == sTransparentCutout || tag == "Foliage" || tag == "SpeedTree")
		{
			val = new Material(transparentShader);
			val.enableInstancing = true;
			if (seeThrough)
			{
				val.EnableKeyword("SEE_THROUGH");
			}
			else
			{
				val.DisableKeyword("SEE_THROUGH");
			}
			if (sourceMat.HasProperty(shaderPropertyID._Cull))
			{
				int num = sourceMat.GetInt(shaderPropertyID._Cull);
				val.SetInt(shaderPropertyID._Cull, num);
			}
			if (sourceMat.HasProperty(shaderPropertyID._MainTex))
			{
				val.SetTexture(shaderPropertyID._MainTex, sourceMat.mainTexture);
				val.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
				val.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
			}
			int cutoff = shaderPropertyID._Cutoff;
			val.SetFloat(cutoff, sourceMat.HasProperty(cutoff) ? sourceMat.GetFloat(cutoff) : transparentCutoff);
			transparent = true;
		}
		else
		{
			transparent = false;
		}
		if (!string.IsNullOrWhiteSpace(sourceMat.GetTag("Fish", false, "")))
		{
			if ((Object)(object)val == (Object)null)
			{
				val = new Material(sharedOpaqueMaterial.shader);
			}
			val.EnableKeyword("FISH_VERTEX_ANIMATION");
			Vector4 vector = sourceMat.GetVector(shaderPropertyID._Speed);
			val.SetVector(shaderPropertyID._Speed, vector);
			Vector4 vector2 = sourceMat.GetVector(shaderPropertyID._Frequency);
			val.SetVector(shaderPropertyID._Frequency, vector2);
			Vector4 vector3 = sourceMat.GetVector(shaderPropertyID._Amplitude);
			val.SetVector(shaderPropertyID._Amplitude, vector3);
			float num2 = sourceMat.GetFloat(shaderPropertyID._HeadLimit);
			val.SetFloat(shaderPropertyID._HeadLimit, num2);
		}
		if (!((Object)(object)val != (Object)null))
		{
			return sharedOpaqueMaterial;
		}
		return val;
	}

	public virtual bool FillBuffer(CommandBuffer buffer, bool forceRender)
	{
		if ((Object)(object)r == (Object)null)
		{
			return false;
		}
		if (((Object)(object)lastCamera == (Object)(object)Camera.current) | forceRender)
		{
			int i = 0;
			for (int count = this.data.Count; i < count; i++)
			{
				Data data = this.data[i];
				buffer.DrawRenderer(r, data.material, data.submeshIndex);
			}
		}
		lastCamera = null;
		return true;
	}

	public void SetColorForTransparent(Color clr)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (this.data == null)
		{
			return;
		}
		int i = 0;
		for (int count = this.data.Count; i < count; i++)
		{
			Data data = this.data[i];
			if (data.transparent)
			{
				data.material.SetColor(shaderPropertyID._Color, clr);
			}
		}
	}

	public void SetSeeThroughForTransparent(bool seeThrough)
	{
		if (this.data == null)
		{
			return;
		}
		int i = 0;
		for (int count = this.data.Count; i < count; i++)
		{
			Data data = this.data[i];
			if (data.transparent)
			{
				if (seeThrough)
				{
					data.material.EnableKeyword("SEE_THROUGH");
				}
				else
				{
					data.material.DisableKeyword("SEE_THROUGH");
				}
			}
		}
	}
}
