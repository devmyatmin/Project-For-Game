using System;
using System.Collections.Generic;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

public class OverlayManager : IOverlaySampler
{
	private static readonly Vector2 TEXTURE_FOAM = new Vector2(1f, 0f);

	private static readonly Vector2 DONT_TEXTURE_FOAM = new Vector2(0f, 1f);

	private Material m_overlayMat;

	private LinkedList<WaveOverlay> m_waveOverlays;

	private List<WaveOverlay> m_queryableOverlays;

	private List<WaveOverlay> m_removeOverlays;

	private List<QueryableOverlayResult> m_containingOverlays;

	private Texture2D m_blankNormal;

	private bool m_beenCleared;

	private List<WaveOverlay> m_heightOverlays;

	private List<WaveOverlay> m_normalOverlays;

	private List<WaveOverlay> m_foamOverlays;

	private List<WaveOverlay> m_clipOverlays;

	private Plane[] m_planes;

	private Color m_clearColor;

	public bool HasHeightOverlay { get; private set; }

	public bool HasNormalOverlay { get; private set; }

	public bool HasFoamOverlay { get; private set; }

	public bool HasClipOverlay { get; private set; }

	public float MaxDisplacement { get; private set; }

	public OVERLAY_BLEND_MODE HeightOverlayBlendMode { get; set; }

	public OVERLAY_BLEND_MODE FoamOverlayBlendMode { get; set; }

	public OverlayManager(Material mat)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected Obj, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		HeightOverlayBlendMode = OVERLAY_BLEND_MODE.ADD;
		FoamOverlayBlendMode = OVERLAY_BLEND_MODE.ADD;
		MaxDisplacement = 1f;
		m_overlayMat = mat;
		m_waveOverlays = new LinkedList<WaveOverlay>();
		m_queryableOverlays = new List<WaveOverlay>(32);
		m_removeOverlays = new List<WaveOverlay>(32);
		m_containingOverlays = new List<QueryableOverlayResult>(32);
		m_heightOverlays = new List<WaveOverlay>(32);
		m_normalOverlays = new List<WaveOverlay>(32);
		m_foamOverlays = new List<WaveOverlay>(32);
		m_clipOverlays = new List<WaveOverlay>(32);
		m_blankNormal = new Texture2D(1, 1, (TextureFormat)5, false, true);
		m_blankNormal.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f, 0.5f));
		((Object)m_blankNormal).hideFlags = (HideFlags)61;
		((Object)m_blankNormal).name = "Ceto Blank Normal Texture";
		m_blankNormal.Apply();
		m_clearColor = new Color(0f, 0f, 0f, 0f);
		m_planes = new Plane[6];
	}

	public void Release()
	{
		Object.DestroyImmediate((Object)(object)m_blankNormal);
	}

	public void Update()
	{
		HasHeightOverlay = false;
		HasNormalOverlay = false;
		HasFoamOverlay = false;
		HasClipOverlay = false;
		MaxDisplacement = 1f;
		m_queryableOverlays.Clear();
		m_removeOverlays.Clear();
		LinkedList<WaveOverlay>.Enumerator enumerator = m_waveOverlays.GetEnumerator();
		while (enumerator.MoveNext())
		{
			WaveOverlay current = enumerator.Current;
			if (current.Kill)
			{
				m_removeOverlays.Add(current);
			}
			else
			{
				if (current.Hide)
				{
					continue;
				}
				bool flag = false;
				if (current.HeightTex.IsDrawable)
				{
					HasHeightOverlay = true;
					flag = true;
					if (current.HeightTex.alpha > MaxDisplacement)
					{
						MaxDisplacement = current.HeightTex.alpha;
					}
				}
				if (current.NormalTex.IsDrawable)
				{
					HasNormalOverlay = true;
				}
				if (current.FoamTex.IsDrawable)
				{
					HasFoamOverlay = true;
				}
				if (current.ClipTex.IsDrawable)
				{
					HasClipOverlay = true;
					flag = true;
				}
				if (flag)
				{
					m_queryableOverlays.Add(current);
				}
			}
		}
		MaxDisplacement = Mathf.Min(MaxDisplacement, 20f);
		for (int i = 0; i < m_removeOverlays.Count; i++)
		{
			m_waveOverlays.Remove(m_removeOverlays[i]);
		}
	}

	public void Add(WaveOverlay overlay)
	{
		if (!overlay.Kill)
		{
			m_waveOverlays.AddLast(overlay);
		}
	}

	public void Remove(WaveOverlay overlay)
	{
		m_waveOverlays.Remove(overlay);
	}

	public void Clear()
	{
		m_waveOverlays.Clear();
	}

	public void ClearBuffers(WaveOverlayData data)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		RTUtility.ClearColor(data.height, m_clearColor);
		RTUtility.ClearColor(data.normal, m_clearColor);
		RTUtility.ClearColor(data.foam, m_clearColor);
		RTUtility.ClearColor(data.clip, m_clearColor);
	}

	public void DestroyBuffers(WaveOverlayData data)
	{
		RTUtility.ReleaseAndDestroy(data.normal);
		RTUtility.ReleaseAndDestroy(data.height);
		RTUtility.ReleaseAndDestroy(data.foam);
		RTUtility.ReleaseAndDestroy(data.clip);
		data.normal = null;
		data.height = null;
		data.foam = null;
		data.clip = null;
	}

	public bool QueryableContains(float x, float z, bool overrideIqnoreQuerys)
	{
		for (int i = 0; i < m_queryableOverlays.Count; i++)
		{
			WaveOverlay waveOverlay = m_queryableOverlays[i];
			if (!waveOverlay.Hide)
			{
				bool num = (overrideIqnoreQuerys || !waveOverlay.HeightTex.ignoreQuerys) && waveOverlay.HeightTex.IsDrawable;
				bool flag = (overrideIqnoreQuerys || !waveOverlay.ClipTex.ignoreQuerys) && waveOverlay.ClipTex.IsDrawable;
				if ((num || flag) && waveOverlay.Contains(x, z))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetQueryableContaining(float x, float z, bool overrideIqnoreQuerys, bool clipOnly)
	{
		m_containingOverlays.Clear();
		QueryableOverlayResult item = default;
		for (int i = 0; i < m_queryableOverlays.Count; i++)
		{
			WaveOverlay waveOverlay = m_queryableOverlays[i];
			if (!waveOverlay.Hide)
			{
				bool num = !clipOnly && (overrideIqnoreQuerys || !waveOverlay.HeightTex.ignoreQuerys) && waveOverlay.HeightTex.IsDrawable;
				bool flag = (overrideIqnoreQuerys || !waveOverlay.ClipTex.ignoreQuerys) && waveOverlay.ClipTex.IsDrawable;
				if ((num || flag) && waveOverlay.Contains(x, z, out var u, out var v))
				{
					item.overlay = waveOverlay;
					item.u = u;
					item.v = v;
					m_containingOverlays.Add(item);
				}
			}
		}
	}

	public void QueryWaves(WaveQuery query)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (m_queryableOverlays.Count == 0 || !query.sampleOverlay)
		{
			return;
		}
		bool flag = query.mode == QUERY_MODE.CLIP_TEST;
		float posX = query.posX;
		float posZ = query.posZ;
		GetQueryableContaining(posX, posZ, query.overrideIgnoreQuerys, flag);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		OverlayClipTexture overlayClipTexture = null;
		OverlayHeightTexture overlayHeightTexture = null;
		for (int i = 0; i < m_containingOverlays.Count; i++)
		{
			QueryableOverlayResult queryableOverlayResult = m_containingOverlays[i];
			try
			{
				overlayClipTexture = queryableOverlayResult.overlay.ClipTex;
				overlayHeightTexture = queryableOverlayResult.overlay.HeightTex;
				if (overlayClipTexture.IsDrawable && overlayClipTexture.tex is Texture2D)
				{
					Texture tex = overlayClipTexture.tex;
					float a = ((Texture2D)((tex is Texture2D) ? tex : null)).GetPixelBilinear(queryableOverlayResult.u, queryableOverlayResult.v).a;
					num += a * Mathf.Max(0f, overlayClipTexture.alpha);
				}
				if (!flag && overlayHeightTexture.IsDrawable)
				{
					float alpha = overlayHeightTexture.alpha;
					float num4 = Mathf.Max(0f, overlayHeightTexture.maskAlpha);
					float num5 = 0f;
					float num6 = 0f;
					if ((Object)(object)overlayHeightTexture.tex != (Object)null && overlayHeightTexture.tex is Texture2D)
					{
						Texture tex2 = overlayHeightTexture.tex;
						num5 = ((Texture2D)((tex2 is Texture2D) ? tex2 : null)).GetPixelBilinear(queryableOverlayResult.u, queryableOverlayResult.v).a;
					}
					if ((Object)(object)overlayHeightTexture.mask != (Object)null && overlayHeightTexture.mask is Texture2D)
					{
						Texture mask = overlayHeightTexture.mask;
						num6 = ((Texture2D)((mask is Texture2D) ? mask : null)).GetPixelBilinear(queryableOverlayResult.u, queryableOverlayResult.v).a;
						num6 = Mathf.Clamp01(num6 * num4);
					}
					if (overlayHeightTexture.maskMode == OVERLAY_MASK_MODE.WAVES)
					{
						num5 *= alpha;
					}
					else if (overlayHeightTexture.maskMode == OVERLAY_MASK_MODE.OVERLAY)
					{
						num5 *= alpha * num6;
						num6 = 0f;
					}
					else if (overlayHeightTexture.maskMode == OVERLAY_MASK_MODE.WAVES_AND_OVERLAY)
					{
						num5 *= alpha * (1f - num6);
					}
					else if (overlayHeightTexture.maskMode == OVERLAY_MASK_MODE.WAVES_AND_OVERLAY_BLEND)
					{
						num5 *= alpha * num6;
					}
					if (HeightOverlayBlendMode == OVERLAY_BLEND_MODE.ADD)
					{
						num2 += num5;
						num3 += num6;
					}
					else if (HeightOverlayBlendMode == OVERLAY_BLEND_MODE.MAX)
					{
						num2 = Mathf.Max(num5, num2);
						num3 = Mathf.Max(num6, num3);
					}
				}
			}
			catch
			{
			}
		}
		num = Mathf.Clamp01(num);
		if (0.5f - num < 0f)
		{
			query.result.isClipped = true;
		}
		num3 = 1f - Mathf.Clamp01(num3);
		query.result.height *= num3;
		query.result.displacementX *= num3;
		query.result.displacementZ *= num3;
		query.result.height += num2;
		query.result.overlayHeight = num2;
	}

	public void RenderWaveOverlays(Camera cam, WaveOverlayData data)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_beenCleared)
		{
			ClearBuffers(data);
			m_beenCleared = true;
		}
		if (m_waveOverlays.Count == 0)
		{
			return;
		}
		GetFrustumPlanes(cam.projectionMatrix * cam.worldToCameraMatrix);
		m_heightOverlays.Clear();
		m_normalOverlays.Clear();
		m_foamOverlays.Clear();
		m_clipOverlays.Clear();
		LinkedList<WaveOverlay>.Enumerator enumerator = m_waveOverlays.GetEnumerator();
		while (enumerator.MoveNext())
		{
			WaveOverlay current = enumerator.Current;
			if (!current.Hide && GeometryUtility.TestPlanesAABB(m_planes, current.BoundingBox))
			{
				if (current.HeightTex.IsDrawable)
				{
					m_heightOverlays.Add(current);
				}
				if (current.NormalTex.IsDrawable)
				{
					m_normalOverlays.Add(current);
				}
				if (current.FoamTex.IsDrawable)
				{
					m_foamOverlays.Add(current);
				}
				if (current.ClipTex.IsDrawable)
				{
					m_clipOverlays.Add(current);
				}
			}
		}
		RenderHeightOverlays(data.height);
		RenderNormalOverlays(data.normal);
		RenderFoamOverlays(data.foam);
		RenderClipOverlays(data.clip);
		if ((Object)(object)data.normal != (Object)null)
		{
			Shader.SetGlobalTexture("Ceto_Overlay_NormalMap", (Texture)(object)data.normal);
		}
		else
		{
			Shader.SetGlobalTexture("Ceto_Overlay_NormalMap", (Texture)(object)Texture2D.blackTexture);
		}
		if ((Object)(object)data.height != (Object)null)
		{
			Shader.SetGlobalTexture("Ceto_Overlay_HeightMap", (Texture)(object)data.height);
		}
		else
		{
			Shader.SetGlobalTexture("Ceto_Overlay_HeightMap", (Texture)(object)Texture2D.blackTexture);
		}
		if ((Object)(object)data.foam != (Object)null)
		{
			Shader.SetGlobalTexture("Ceto_Overlay_FoamMap", (Texture)(object)data.foam);
		}
		else
		{
			Shader.SetGlobalTexture("Ceto_Overlay_FoamMap", (Texture)(object)Texture2D.blackTexture);
		}
		if ((Object)(object)data.clip != (Object)null)
		{
			Shader.SetGlobalTexture("Ceto_Overlay_ClipMap", (Texture)(object)data.clip);
		}
		else
		{
			Shader.SetGlobalTexture("Ceto_Overlay_ClipMap", (Texture)(object)Texture2D.blackTexture);
		}
		m_beenCleared = false;
	}

	public void GetFrustumPlanes(Matrix4x4 mat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		float num = mat.m30 + mat.m00;
		float num2 = mat.m31 + mat.m01;
		float num3 = mat.m32 + mat.m02;
		m_planes[0].normal = new Vector3(num, num2, num3);
		m_planes[0].distance = mat.m33 + mat.m03;
		num = mat.m30 - mat.m00;
		num2 = mat.m31 - mat.m01;
		num3 = mat.m32 - mat.m02;
		m_planes[1].normal = new Vector3(num, num2, num3);
		m_planes[1].distance = mat.m33 - mat.m03;
		num = mat.m30 + mat.m10;
		num2 = mat.m31 + mat.m11;
		num3 = mat.m32 + mat.m12;
		m_planes[2].normal = new Vector3(num, num2, num3);
		m_planes[2].distance = mat.m33 + mat.m13;
		num = mat.m30 - mat.m10;
		num2 = mat.m31 - mat.m11;
		num3 = mat.m32 - mat.m12;
		m_planes[3].normal = new Vector3(num, num2, num3);
		m_planes[3].distance = mat.m33 - mat.m13;
		num = mat.m30 + mat.m20;
		num2 = mat.m31 + mat.m21;
		num3 = mat.m32 + mat.m22;
		m_planes[4].normal = new Vector3(num, num2, num3);
		m_planes[4].distance = mat.m33 + mat.m23;
		num = mat.m30 - mat.m20;
		num2 = mat.m31 - mat.m21;
		num3 = mat.m32 - mat.m22;
		m_planes[5].normal = new Vector3(num, num2, num3);
		m_planes[5].distance = mat.m33 - mat.m23;
	}

	private void RenderHeightOverlays(RenderTexture target)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			int pass = ((HeightOverlayBlendMode != OVERLAY_BLEND_MODE.ADD) ? 4 : 0);
			for (int i = 0; i < m_heightOverlays.Count; i++)
			{
				WaveOverlay waveOverlay = m_heightOverlays[i];
				m_overlayMat.SetFloat("Ceto_Overlay_Alpha", waveOverlay.HeightTex.alpha);
				m_overlayMat.SetFloat("Ceto_Overlay_MaskAlpha", Mathf.Max(0f, waveOverlay.HeightTex.maskAlpha));
				m_overlayMat.SetTexture("Ceto_Overlay_Height", (Texture)(((Object)(object)waveOverlay.HeightTex.tex != (Object)null) ? ((object)waveOverlay.HeightTex.tex) : ((object)Texture2D.blackTexture)));
				m_overlayMat.SetTexture("Ceto_Overlay_HeightMask", (Texture)(((Object)(object)waveOverlay.HeightTex.mask != (Object)null) ? ((object)waveOverlay.HeightTex.mask) : ((object)Texture2D.blackTexture)));
				m_overlayMat.SetFloat("Ceto_Overlay_MaskMode", (float)waveOverlay.HeightTex.maskMode);
				Blit(waveOverlay.Corners, waveOverlay.HeightTex.scaleUV, waveOverlay.HeightTex.offsetUV, target, m_overlayMat, pass);
			}
		}
	}

	private void RenderNormalOverlays(RenderTexture target)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			Matrix4x4 val = default;
			for (int i = 0; i < m_normalOverlays.Count; i++)
			{
				WaveOverlay waveOverlay = m_normalOverlays[i];
				RotateY(val, waveOverlay.Rotation);
				m_overlayMat.SetMatrix("Ceto_Overlay_Rotation", val);
				m_overlayMat.SetFloat("Ceto_Overlay_Alpha", Mathf.Max(0f, waveOverlay.NormalTex.alpha));
				m_overlayMat.SetFloat("Ceto_Overlay_MaskAlpha", Mathf.Max(0f, waveOverlay.NormalTex.maskAlpha));
				m_overlayMat.SetTexture("Ceto_Overlay_Normal", (Texture)(((Object)(object)waveOverlay.NormalTex.tex != (Object)null) ? ((object)waveOverlay.NormalTex.tex) : ((object)m_blankNormal)));
				m_overlayMat.SetTexture("Ceto_Overlay_NormalMask", (Texture)(((Object)(object)waveOverlay.NormalTex.mask != (Object)null) ? ((object)waveOverlay.NormalTex.mask) : ((object)Texture2D.blackTexture)));
				m_overlayMat.SetFloat("Ceto_Overlay_MaskMode", (float)waveOverlay.NormalTex.maskMode);
				Blit(waveOverlay.Corners, waveOverlay.NormalTex.scaleUV, waveOverlay.NormalTex.offsetUV, target, m_overlayMat, 1);
			}
		}
	}

	private void RotateY(Matrix4x4 m, float angle)
	{
		float num = Mathf.Cos(angle * (float)Math.PI / 180f);
		float num2 = Mathf.Sin(angle * (float)Math.PI / 180f);
		m.m00 = num;
		m.m01 = 0f;
		m.m02 = num2;
		m.m03 = 0f;
		m.m10 = 0f;
		m.m11 = 1f;
		m.m12 = 0f;
		m.m13 = 0f;
		m.m20 = 0f - num2;
		m.m21 = 0f;
		m.m22 = num;
		m.m23 = 1f;
		m.m30 = 0f;
		m.m31 = 0f;
		m.m32 = 0f;
		m.m33 = 0f;
	}

	private void RenderFoamOverlays(RenderTexture target)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			int pass = ((FoamOverlayBlendMode == OVERLAY_BLEND_MODE.ADD) ? 2 : 5);
			for (int i = 0; i < m_foamOverlays.Count; i++)
			{
				WaveOverlay waveOverlay = m_foamOverlays[i];
				m_overlayMat.SetFloat("Ceto_Overlay_Alpha", Mathf.Max(0f, waveOverlay.FoamTex.alpha));
				m_overlayMat.SetFloat("Ceto_Overlay_MaskAlpha", Mathf.Max(0f, waveOverlay.FoamTex.maskAlpha));
				m_overlayMat.SetTexture("Ceto_Overlay_Foam", (Texture)(((Object)(object)waveOverlay.FoamTex.tex != (Object)null) ? ((object)waveOverlay.FoamTex.tex) : ((object)Texture2D.blackTexture)));
				m_overlayMat.SetTexture("Ceto_Overlay_FoamMask", (Texture)(((Object)(object)waveOverlay.FoamTex.mask != (Object)null) ? ((object)waveOverlay.FoamTex.mask) : ((object)Texture2D.blackTexture)));
				m_overlayMat.SetFloat("Ceto_Overlay_MaskMode", (float)waveOverlay.FoamTex.maskMode);
				m_overlayMat.SetVector("Ceto_TextureFoam", Vector4.op_Implicit(waveOverlay.FoamTex.textureFoam ? TEXTURE_FOAM : DONT_TEXTURE_FOAM));
				Blit(waveOverlay.Corners, waveOverlay.FoamTex.scaleUV, waveOverlay.FoamTex.offsetUV, target, m_overlayMat, pass);
			}
		}
	}

	private void RenderClipOverlays(RenderTexture target)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			for (int i = 0; i < m_clipOverlays.Count; i++)
			{
				WaveOverlay waveOverlay = m_clipOverlays[i];
				m_overlayMat.SetFloat("Ceto_Overlay_Alpha", Mathf.Max(0f, waveOverlay.ClipTex.alpha));
				m_overlayMat.SetTexture("Ceto_Overlay_Clip", (Texture)(((Object)(object)waveOverlay.ClipTex.tex != (Object)null) ? ((object)waveOverlay.ClipTex.tex) : ((object)Texture2D.blackTexture)));
				Blit(waveOverlay.Corners, waveOverlay.ClipTex.scaleUV, waveOverlay.ClipTex.offsetUV, target, m_overlayMat, 3);
			}
		}
	}

	private void Blit(Vector4[] corners, Vector2 scale, Vector2 offset, RenderTexture des, Material mat, int pass)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		Graphics.SetRenderTarget(des);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(pass);
		GL.Begin(7);
		GL.MultiTexCoord2(0, offset.x, offset.y);
		GL.MultiTexCoord2(1, 0f, 0f);
		GL.Vertex(Vector4.op_Implicit(corners[0]));
		GL.MultiTexCoord2(0, offset.x + 1f * scale.x, offset.y);
		GL.MultiTexCoord2(1, 1f, 0f);
		GL.Vertex(Vector4.op_Implicit(corners[1]));
		GL.MultiTexCoord2(0, offset.x + 1f * scale.x, offset.y + 1f * scale.y);
		GL.MultiTexCoord2(1, 1f, 1f);
		GL.Vertex(Vector4.op_Implicit(corners[2]));
		GL.MultiTexCoord2(0, offset.x, offset.y + 1f * scale.y);
		GL.MultiTexCoord2(1, 0f, 1f);
		GL.Vertex(Vector4.op_Implicit(corners[3]));
		GL.End();
		GL.PopMatrix();
	}

	public void CreateOverlays(Camera cam, WaveOverlayData overlay, OVERLAY_MAP_SIZE normalOverlaySize, OVERLAY_MAP_SIZE heightOverlaySize, OVERLAY_MAP_SIZE foamOverlaySize, OVERLAY_MAP_SIZE clipOverlaySize)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (HasNormalOverlay)
		{
			RenderTextureFormat val = (RenderTextureFormat)2;
			if (!SystemInfo.SupportsRenderTextureFormat(val))
			{
				val = (RenderTextureFormat)0;
			}
			CreateBuffer("Normal", cam, normalOverlaySize, val, isLinear: true, ref overlay.normal);
		}
		if (HasHeightOverlay)
		{
			RenderTextureFormat val2 = (RenderTextureFormat)13;
			if (!SystemInfo.SupportsRenderTextureFormat(val2))
			{
				val2 = (RenderTextureFormat)2;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(val2))
			{
				val2 = (RenderTextureFormat)0;
			}
			CreateBuffer("Height", cam, heightOverlaySize, val2, isLinear: true, ref overlay.height);
		}
		if (HasFoamOverlay)
		{
			RenderTextureFormat format = (RenderTextureFormat)0;
			CreateBuffer("Foam", cam, foamOverlaySize, format, isLinear: false, ref overlay.foam);
		}
		if (HasClipOverlay)
		{
			RenderTextureFormat val3 = (RenderTextureFormat)16;
			if (!SystemInfo.SupportsRenderTextureFormat(val3))
			{
				val3 = (RenderTextureFormat)15;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(val3))
			{
				val3 = (RenderTextureFormat)0;
			}
			CreateBuffer("Clip", cam, clipOverlaySize, val3, isLinear: true, ref overlay.clip);
		}
	}

	public void CreateBuffer(string name, Camera cam, OVERLAY_MAP_SIZE size, RenderTextureFormat format, bool isLinear, ref RenderTexture map)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected Obj, but got Unknown
		float num = SizeToValue(size);
		int num2 = Mathf.Min(4096, (int)((float)cam.pixelWidth * num));
		int num3 = Mathf.Min(4096, (int)((float)cam.pixelHeight * num));
		if ((Object)(object)map == (Object)null || ((Texture)map).width != num2 || ((Texture)map).height != num3)
		{
			if ((Object)(object)map != (Object)null)
			{
				RTUtility.ReleaseAndDestroy(map);
			}
			RenderTextureReadWrite val = (RenderTextureReadWrite)(isLinear ? 1 : 0);
			map = new RenderTexture(num2, num3, 0, format, val);
			map.useMipMap = false;
			((Texture)map).filterMode = (FilterMode)1;
			((Object)map).hideFlags = (HideFlags)52;
			((Object)map).name = "Ceto Overlay " + name + " Buffer: " + ((Object)cam).name;
		}
	}

	private float SizeToValue(OVERLAY_MAP_SIZE size)
	{
		return (int)size switch
		{
			0 => 2f, 
			1 => 1.5f, 
			2 => 1f, 
			3 => 0.5f, 
			4 => 0.25f, 
			_ => 1f, 
		};
	}

	static OverlayManager()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
	}
}
