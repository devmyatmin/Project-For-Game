using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Overlays/AddWaveOverlay")]
public class AddWaveOverlay : AddWaveOverlayBase
{
	public enum ROTATION
	{
		RELATIVE_TO_PARENT,
		INDEPENDANT_TO_PARENT
	}

	public bool checkTextures = true;

	public OverlayHeightTexture heightTexture = new OverlayHeightTexture();

	public OverlayNormalTexture normalTexture = new OverlayNormalTexture();

	public OverlayFoamTexture foamTexture = new OverlayFoamTexture();

	public OverlayClipTexture clipTexture = new OverlayClipTexture();

	public float width = 10f;

	public float height = 10f;

	public ROTATION m_rotationMode;

	[Range(0f, 360f)]
	public float rotation;

	private bool m_registered;

	protected override void Start()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (checkTextures)
		{
			if (!heightTexture.ignoreQuerys)
			{
				CheckCanSampleTex(heightTexture.tex, "height texture");
			}
			if (!heightTexture.ignoreQuerys)
			{
				CheckCanSampleTex(heightTexture.mask, "height mask");
			}
			if (!clipTexture.ignoreQuerys)
			{
				CheckCanSampleTex(clipTexture.tex, "clip texture");
			}
		}
		Vector2 halfSize = new Vector2(width * 0.5f, height * 0.5f);
		m_overlays.Add(new WaveOverlay(((Component)this).transform.position, Rotation(), halfSize, 0f));
		m_overlays[0].HeightTex = heightTexture;
		m_overlays[0].NormalTex = normalTexture;
		m_overlays[0].FoamTex = foamTexture;
		m_overlays[0].ClipTex = clipTexture;
		if (!m_registered && (Object)(object)Ocean.Instance != (Object)null)
		{
			Ocean.Instance.OverlayManager.Add(m_overlays[0]);
			m_registered = true;
		}
	}

	protected override void Update()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (m_overlays != null && m_overlays.Count == 1)
		{
			if (!m_registered && (Object)(object)Ocean.Instance != (Object)null)
			{
				Ocean.Instance.OverlayManager.Add(m_overlays[0]);
				m_registered = true;
			}
			m_overlays[0].Position = ((Component)this).transform.position;
			m_overlays[0].HalfSize = new Vector2(width * 0.5f, height * 0.5f);
			m_overlays[0].Rotation = Rotation();
			m_overlays[0].UpdateOverlay();
		}
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			Vector3 val = new Vector3(width * 0.5f, 1f, height * 0.5f);
			Vector3 position = ((Component)this).transform.position;
			Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(position.x, 0f, position.z), Quaternion.Euler(0f, Rotation(), 0f), val);
			Gizmos.color = Color.yellow;
			Gizmos.matrix = matrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 10f, 2f));
		}
	}
}
