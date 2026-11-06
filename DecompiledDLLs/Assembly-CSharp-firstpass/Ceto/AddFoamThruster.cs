using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Overlays/AddFoamThruster")]
public class AddFoamThruster : AddWaveOverlayBase
{
	public enum ROTATION
	{
		NONE,
		RANDOM,
		RELATIVE
	}

	public Texture foamTexture;

	public bool textureFoam = true;

	public ROTATION rotation = ROTATION.RANDOM;

	public AnimationCurve timeLine = AddWaveOverlayBase.DefaultCurve();

	public float duration = 4f;

	public float size = 2f;

	[Range(16f, 1000f)]
	public float rate = 128f;

	public float expansion = 4f;

	public float momentum = 10f;

	public float spin = 10f;

	public bool mustBeBelowWater = true;

	[Range(0f, 2f)]
	public float alpha = 0.8f;

	[Range(0f, 1f)]
	public float jitter = 0.2f;

	private float m_lastTime;

	private float m_remainingTime;

	private List<FoamOverlay> m_remove = new List<FoamOverlay>();

	private LinkedList<FoamOverlay> m_pool = new LinkedList<FoamOverlay>();

	protected override void Start()
	{
		m_lastTime = Time.time;
	}

	protected override void Update()
	{
		UpdateOverlays();
		AddFoam();
		RemoveOverlays();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lastTime = Time.time;
	}

	public override void Translate(Vector3 amount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Translate(amount);
	}

	private float Rotation()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return rotation switch
		{
			ROTATION.NONE => 0f, 
			ROTATION.RANDOM => Random.Range(0f, 360f), 
			ROTATION.RELATIVE => ((Component)this).transform.eulerAngles.y, 
			_ => 0f, 
		};
	}

	private FoamOverlay NewFoamOverlay(Vector3 pos, float rotation, float size, float duration, Texture texture)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		FoamOverlay foamOverlay = null;
		if (m_pool.Count > 0)
		{
			foamOverlay = m_pool.First.Value;
			foamOverlay.Reset(pos, Rotation(), size, duration, foamTexture);
			m_pool.RemoveFirst();
		}
		else
		{
			foamOverlay = new FoamOverlay(pos, Rotation(), size, duration, foamTexture);
		}
		return foamOverlay;
	}

	private void AddFoam()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (duration <= 0f || (Object)(object)Ocean.Instance == (Object)null)
		{
			m_lastTime = Time.time;
			return;
		}
		size = Mathf.Max(1f, size);
		float num = ((Component)this).transform.position.y;
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = ((Component)this).transform.forward;
		if (mustBeBelowWater)
		{
			num = Ocean.Instance.QueryWaves(position.x, position.z);
		}
		if (num < position.y || (val.x == 0f && val.z == 0f))
		{
			m_lastTime = Time.time;
			return;
		}
		float num2 = Time.time - m_lastTime;
		val = val.normalized;
		position.y = 0f;
		Vector3 val2 = val * momentum;
		m_remainingTime += num2;
		float num3 = rate / 1000f;
		float num4 = 0f;
		while (m_remainingTime > num3)
		{
			Vector3 pos = position + val * num4;
			FoamOverlay foamOverlay = NewFoamOverlay(pos, Rotation(), size, duration, foamTexture);
			foamOverlay.FoamTex.alpha = 0f;
			foamOverlay.FoamTex.textureFoam = textureFoam;
			foamOverlay.Momentum = val2;
			foamOverlay.Spin = ((Random.value > 0.5f) ? (0f - spin) : spin);
			foamOverlay.Expansion = expansion;
			if (jitter > 0f)
			{
				foamOverlay.Spin *= 1f + Random.Range(-1f, 1f) * jitter;
				foamOverlay.Expansion *= 1f + Random.Range(-1f, 1f) * jitter;
			}
			m_overlays.Add(foamOverlay);
			Ocean.Instance.OverlayManager.Add(foamOverlay);
			m_remainingTime -= num3;
			num4 += num3;
		}
		m_lastTime = Time.time;
	}

	private void UpdateOverlays()
	{
		for (int i = 0; i < m_overlays.Count; i++)
		{
			WaveOverlay waveOverlay = m_overlays[i];
			float normalizedAge = waveOverlay.NormalizedAge;
			waveOverlay.FoamTex.alpha = timeLine.Evaluate(normalizedAge) * alpha;
			waveOverlay.FoamTex.textureFoam = textureFoam;
			waveOverlay.UpdateOverlay();
		}
	}

	private void RemoveOverlays()
	{
		m_remove.Clear();
		for (int i = 0; i < m_overlays.Count; i++)
		{
			FoamOverlay foamOverlay = m_overlays[i] as FoamOverlay;
			if (foamOverlay.Age >= foamOverlay.Duration)
			{
				m_remove.Add(foamOverlay);
				foamOverlay.Kill = true;
			}
		}
		for (int j = 0; j < m_remove.Count; j++)
		{
			m_overlays.Remove(m_remove[j]);
			m_pool.AddLast(m_remove[j]);
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(((Component)this).transform.position, 2f);
		}
	}
}
