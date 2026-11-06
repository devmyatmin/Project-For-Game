using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

public abstract class AddWaveOverlayBase : MonoBehaviour
{
	protected List<WaveOverlay> m_overlays = new List<WaveOverlay>();

	public IEnumerable<WaveOverlay> Overlays => m_overlays;

	public WaveOverlay Overlay => m_overlays[0];

	protected virtual void Start()
	{
	}

	public virtual void Translate(Vector3 amount)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_overlays != null)
		{
			for (int i = 0; i < m_overlays.Count; i++)
			{
				m_overlays[i].Position = m_overlays[i].Position + amount;
			}
		}
	}

	protected virtual void Update()
	{
		if (m_overlays != null)
		{
			for (int i = 0; i < m_overlays.Count; i++)
			{
				m_overlays[i].UpdateOverlay();
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (m_overlays != null)
		{
			for (int i = 0; i < m_overlays.Count; i++)
			{
				m_overlays[i].Hide = false;
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (m_overlays != null)
		{
			for (int i = 0; i < m_overlays.Count; i++)
			{
				m_overlays[i].Hide = true;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_overlays != null)
		{
			for (int i = 0; i < m_overlays.Count; i++)
			{
				m_overlays[i].Kill = true;
			}
		}
	}

	protected static AnimationCurve DefaultCurve()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		return new AnimationCurve(new Keyframe[4]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.012f, 0.98f),
			new Keyframe(0.026f, 1f),
			new Keyframe(1f, 0f)
		});
	}

	protected void CheckCanSampleTex(Texture tex, string name)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tex == (Object)null)
		{
			return;
		}
		if (!(tex is Texture2D))
		{
			Ocean.LogWarning("Can not query overlays " + name + " if texture is not Texture2D");
			return;
		}
		Texture2D val = (Texture2D)(object)((tex is Texture2D) ? tex : null);
		try
		{
			val.GetPixel(0, 0);
		}
		catch
		{
			Ocean.LogWarning("Can not query overlays " + name + " if read/write is not enabled");
		}
	}
}
