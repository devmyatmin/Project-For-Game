using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Overlays/AddFoamTrail")]
public class AddFoamTrail : AddWaveOverlayBase
{
	public enum ROTATION
	{
		NONE,
		RANDOM,
		RELATIVE
	}

	private readonly float MIN_MOVEMENT = 0.1f;

	private readonly float MAX_MOVEMENT = 100f;

	public Texture foamTexture;

	public bool textureFoam = true;

	public ROTATION rotation = ROTATION.RANDOM;

	public AnimationCurve timeLine = AddWaveOverlayBase.DefaultCurve();

	public float duration = 10f;

	public float size = 10f;

	public float spacing = 4f;

	public float expansion = 1f;

	public float momentum = 1f;

	public float spin = 10f;

	public bool mustBeBelowWater = true;

	[Range(0f, 2f)]
	public float alpha = 0.8f;

	[Range(0f, 1f)]
	public float jitter = 0.2f;

	private Vector3 m_lastPosition;

	private float m_remainingDistance;

	private List<FoamOverlay> m_remove = new List<FoamOverlay>();

	private LinkedList<FoamOverlay> m_pool = new LinkedList<FoamOverlay>();

	protected override void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_lastPosition = ((Component)this).transform.position;
	}

	protected override void Update()
	{
		UpdateOverlays();
		AddFoam();
		RemoveOverlays();
	}

	protected override void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lastPosition = ((Component)this).transform.position;
	}

	public override void Translate(Vector3 amount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		base.Translate(amount);
		m_lastPosition += amount;
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
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		if (duration <= 0f || (Object)(object)Ocean.Instance == (Object)null)
		{
			m_lastPosition = ((Component)this).transform.position;
			return;
		}
		spacing = Mathf.Max(1f, spacing);
		size = Mathf.Max(1f, size);
		Vector3 position = ((Component)this).transform.position;
		float num = position.y;
		if (mustBeBelowWater)
		{
			num = Ocean.Instance.QueryWaves(position.x, position.z);
		}
		if (num < position.y)
		{
			m_lastPosition = position;
			return;
		}
		position.y = 0f;
		m_lastPosition.y = 0f;
		Vector3 val = m_lastPosition - position;
		Vector3 normalized = val.normalized;
		float magnitude = val.magnitude;
		if (magnitude < MIN_MOVEMENT)
		{
			return;
		}
		magnitude = Mathf.Min(MAX_MOVEMENT, magnitude);
		Vector3 val2 = normalized * momentum;
		m_remainingDistance += magnitude;
		float num2 = 0f;
		while (m_remainingDistance > spacing)
		{
			Vector3 pos = position + normalized * num2;
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
			m_remainingDistance -= spacing;
			num2 += spacing;
		}
		m_lastPosition = position;
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
