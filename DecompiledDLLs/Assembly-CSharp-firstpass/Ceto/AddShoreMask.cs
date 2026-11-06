using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Overlays/AddShoreMask")]
public class AddShoreMask : AddWaveOverlayBase
{
	public enum ROTATION
	{
		RELATIVE_TO_PARENT,
		INDEPENDANT_TO_PARENT
	}

	public bool checkTextures = true;

	public bool ignoreQuerys;

	public Texture heightMask;

	[Range(0f, 1f)]
	public float heightAlpha = 1f;

	public Texture normalMask;

	[Range(0f, 1f)]
	public float normalAlpha = 1f;

	public Texture edgeFoam;

	[Range(0f, 10f)]
	public float edgeFoamAlpha = 1f;

	public bool textureFoam = true;

	public Texture foamMask;

	[Range(0f, 1f)]
	public float foamMaskAlpha = 1f;

	public Texture clipMask;

	public float width = 10f;

	public float height = 10f;

	public Vector3 offset;

	public ROTATION m_rotationMode;

	[Range(0f, 360f)]
	public float rotation;

	private bool m_registered;

	protected override void Start()
	{
		if (checkTextures && !ignoreQuerys)
		{
			CheckCanSampleTex(heightMask, "height mask");
			CheckCanSampleTex(clipMask, "clip mask");
		}
		m_overlays.Add(new WaveOverlay());
		UpdateOverlay();
	}

	protected override void Update()
	{
		if (m_overlays != null && m_overlays.Count == 1)
		{
			UpdateOverlay();
		}
	}

	private void UpdateOverlay()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!m_registered && (Object)(object)Ocean.Instance != (Object)null)
		{
			Ocean.Instance.OverlayManager.Add(m_overlays[0]);
			m_registered = true;
		}
		m_overlays[0].Position = ((Component)this).transform.position + offset;
		m_overlays[0].HalfSize = new Vector2(width * 0.5f, height * 0.5f);
		m_overlays[0].Rotation = Rotation();
		m_overlays[0].HeightTex.mask = heightMask;
		m_overlays[0].HeightTex.maskAlpha = heightAlpha;
		m_overlays[0].HeightTex.ignoreQuerys = ignoreQuerys;
		m_overlays[0].NormalTex.mask = normalMask;
		m_overlays[0].NormalTex.maskAlpha = normalAlpha;
		m_overlays[0].NormalTex.maskMode = OVERLAY_MASK_MODE.WAVES_AND_OVERLAY_BLEND;
		m_overlays[0].FoamTex.tex = edgeFoam;
		m_overlays[0].FoamTex.alpha = edgeFoamAlpha;
		m_overlays[0].FoamTex.textureFoam = textureFoam;
		m_overlays[0].FoamTex.mask = foamMask;
		m_overlays[0].FoamTex.maskAlpha = foamMaskAlpha;
		m_overlays[0].ClipTex.tex = clipMask;
		m_overlays[0].ClipTex.ignoreQuerys = ignoreQuerys;
		m_overlays[0].UpdateOverlay();
	}

	private float Rotation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return m_rotationMode switch
		{
			ROTATION.RELATIVE_TO_PARENT => ((Component)this).transform.eulerAngles.y + rotation, 
			ROTATION.INDEPENDANT_TO_PARENT => rotation, 
			_ => rotation, 
		};
	}

	private void OnDrawGizmos()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			Vector3 val = new Vector3(width * 0.5f, 1f, height * 0.5f);
			Vector3 val2 = ((Component)this).transform.position + offset;
			Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(val2.x, 0f, val2.z), Quaternion.Euler(0f, Rotation(), 0f), val);
			Gizmos.color = Color.yellow;
			Gizmos.matrix = matrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 10f, 2f));
		}
	}
}
