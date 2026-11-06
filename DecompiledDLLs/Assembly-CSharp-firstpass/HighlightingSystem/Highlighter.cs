using System;
using System.Collections.Generic;
using Beam.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightingSystem;

[DisallowMultipleComponent]
public class Highlighter : MonoBehaviour
{
	private enum Mode
	{
		None,
		Highlighter,
		HighlighterSeeThrough,
		OccluderSeeThrough
	}

	public static readonly List<Type> types = new List<Type>
	{
		typeof(MeshRenderer),
		typeof(SkinnedMeshRenderer),
		typeof(SpriteRenderer),
		typeof(ParticleSystemRenderer),
		typeof(InstancedMeshRenderer)
	};

	public const string keywordSeeThrough = "SEE_THROUGH";

	private const float doublePI = (float)Math.PI * 2f;

	private readonly Color occluderColor = new Color(0f, 0f, 0f, 0f);

	private static readonly Mode[] renderingOrder = new Mode[3]
	{
		Mode.Highlighter,
		Mode.HighlighterSeeThrough,
		Mode.OccluderSeeThrough
	};

	private static HashSet<Highlighter> highlighters = new HashSet<Highlighter>();

	[HideInInspector]
	public bool seeThrough;

	[HideInInspector]
	public bool occluder;

	[HideInInspector]
	public bool forceRender;

	private Transform tr;

	private List<HighlighterRenderer> highlightableRenderers = new List<HighlighterRenderer>();

	private bool renderersDirty;

	private static List<Component> sComponents = new List<Component>(4);

	private Mode mode;

	private bool cachedSeeThrough;

	private int cachedOnce = -1;

	private bool flashing;

	private Color currentColor;

	private float transitionValue;

	private float transitionTarget;

	private float transitionTime;

	private Color onceColor;

	private float flashingFreq;

	private Color flashingColorMin;

	private Color flashingColorMax;

	private Color constantColor;

	private static Shader _opaqueShader;

	private static Shader _transparentShader;

	private Material _opaqueMaterial;

	private static ShaderPropertyID shaderPropertyID => HighlightingBase.shaderPropertyID;

	private bool once
	{
		get
		{
			return cachedOnce == Time.frameCount;
		}
		set
		{
			cachedOnce = (value ? Time.frameCount : (-1));
		}
	}

	public static Shader opaqueShader
	{
		get
		{
			if ((Object)(object)_opaqueShader == (Object)null)
			{
				_opaqueShader = Shader.Find("Hidden/Highlighted/Opaque");
			}
			return _opaqueShader;
		}
	}

	public static Shader transparentShader
	{
		get
		{
			if ((Object)(object)_transparentShader == (Object)null)
			{
				_transparentShader = Shader.Find("Hidden/Highlighted/Transparent");
			}
			return _transparentShader;
		}
	}

	private Material opaqueMaterial
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected Obj, but got Unknown
			if ((Object)(object)_opaqueMaterial == (Object)null)
			{
				_opaqueMaterial = new Material(opaqueShader);
				_opaqueMaterial.enableInstancing = true;
				if (cachedSeeThrough)
				{
					_opaqueMaterial.EnableKeyword("SEE_THROUGH");
				}
				else
				{
					_opaqueMaterial.DisableKeyword("SEE_THROUGH");
				}
			}
			return _opaqueMaterial;
		}
	}

	public void ReinitMaterials()
	{
		renderersDirty = true;
	}

	public void OnParams(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		onceColor = color;
	}

	public void On()
	{
		once = true;
	}

	public void On(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		onceColor = color;
		once = true;
	}

	public void FlashingParams(Color color1, Color color2, float freq)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		flashingColorMin = color1;
		flashingColorMax = color2;
		flashingFreq = freq;
	}

	public void FlashingOn()
	{
		flashing = true;
	}

	public void FlashingOn(Color color1, Color color2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		flashingColorMin = color1;
		flashingColorMax = color2;
		flashing = true;
	}

	public void FlashingOn(Color color1, Color color2, float freq)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		flashingColorMin = color1;
		flashingColorMax = color2;
		flashingFreq = freq;
		flashing = true;
	}

	public void FlashingOn(float freq)
	{
		flashingFreq = freq;
		flashing = true;
	}

	public void FlashingOff()
	{
		flashing = false;
	}

	public void FlashingSwitch()
	{
		flashing = !flashing;
	}

	public void ConstantParams(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		constantColor = color;
	}

	public void ConstantOn(float time = 0.25f)
	{
		transitionTime = ((time >= 0f) ? time : 0f);
		transitionTarget = 1f;
	}

	public void ConstantOn(Color color, float time = 0.25f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		constantColor = color;
		transitionTime = ((time >= 0f) ? time : 0f);
		transitionTarget = 1f;
	}

	public void ConstantOff(float time = 0.25f)
	{
		transitionTime = ((time >= 0f) ? time : 0f);
		transitionTarget = 0f;
	}

	public void ConstantSwitch(float time = 0.25f)
	{
		transitionTime = ((time >= 0f) ? time : 0f);
		transitionTarget = ((transitionTarget > 0f) ? 0f : 1f);
	}

	public void ConstantOnImmediate()
	{
		transitionValue = (transitionTarget = 1f);
	}

	public void ConstantOnImmediate(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		constantColor = color;
		transitionValue = (transitionTarget = 1f);
	}

	public void ConstantOffImmediate()
	{
		transitionValue = (transitionTarget = 0f);
	}

	public void ConstantSwitchImmediate()
	{
		transitionValue = (transitionTarget = ((transitionTarget > 0f) ? 0f : 1f));
	}

	public void Off()
	{
		once = false;
		flashing = false;
		transitionValue = (transitionTarget = 0f);
	}

	public void Die()
	{
		Object.Destroy((Object)(object)this);
	}

	public void SeeThrough(bool state)
	{
		seeThrough = state;
	}

	public void SeeThroughOn()
	{
		seeThrough = true;
	}

	public void SeeThroughOff()
	{
		seeThrough = false;
	}

	public void SeeThroughSwitch()
	{
		seeThrough = !seeThrough;
	}

	public void OccluderOn()
	{
		occluder = true;
	}

	public void OccluderOff()
	{
		occluder = false;
	}

	public void OccluderSwitch()
	{
		occluder = !occluder;
	}

	private void Awake()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		tr = ((Component)this).GetComponent<Transform>();
		renderersDirty = true;
		cachedSeeThrough = (seeThrough = true);
		mode = Mode.None;
		once = false;
		flashing = false;
		occluder = false;
		transitionValue = (transitionTarget = 0f);
		onceColor = Color.red;
		flashingFreq = 2f;
		flashingColorMin = new Color(0f, 1f, 1f, 0f);
		flashingColorMax = new Color(0f, 1f, 1f, 1f);
		constantColor = Color.yellow;
	}

	private void OnEnable()
	{
		highlighters.Add(this);
	}

	private void OnDisable()
	{
		highlighters.Remove(this);
		ClearRenderers();
		renderersDirty = true;
		once = false;
		flashing = false;
		transitionValue = (transitionTarget = 0f);
	}

	private void Update()
	{
		UpdateTransition();
	}

	private void OnDestroy()
	{
		if ((Object)(object)_opaqueMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)_opaqueMaterial);
		}
	}

	private void ClearRenderers()
	{
		for (int num = highlightableRenderers.Count - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)highlightableRenderers[num]);
		}
		highlightableRenderers.Clear();
	}

	private void UpdateRenderers()
	{
		if (LevelLoader.IsLoading() || !renderersDirty)
		{
			return;
		}
		ClearRenderers();
		List<Component> list = new List<Component>();
		GrabRenderers(tr, list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i].gameObject;
			HighlighterRenderer highlighterRenderer = gameObject.GetComponent<HighlighterRenderer>();
			if ((Object)(object)highlighterRenderer == (Object)null)
			{
				highlighterRenderer = gameObject.AddComponent<HighlighterRenderer>();
			}
			highlighterRenderer.Initialize(opaqueMaterial, transparentShader, cachedSeeThrough);
			highlightableRenderers.Add(highlighterRenderer);
		}
		renderersDirty = false;
	}

	private void GrabRenderers(Transform t, List<Component> renderers)
	{
		GameObject gameObject = ((Component)t).gameObject;
		if (gameObject.CompareTag("DontHighlight"))
		{
			return;
		}
		int i = 0;
		for (int count = types.Count; i < count; i++)
		{
			gameObject.GetComponents(types[i], sComponents);
			int j = 0;
			for (int count2 = sComponents.Count; j < count2; j++)
			{
				renderers.Add(sComponents[j]);
			}
		}
		sComponents.Clear();
		int childCount = t.childCount;
		if (childCount == 0)
		{
			return;
		}
		for (int k = 0; k < childCount; k++)
		{
			Transform child = t.GetChild(k);
			if (!((Object)(object)((Component)child).GetComponent<Highlighter>() != (Object)null) && !((Object)(object)((Component)child).GetComponent<HighlighterBlocker>() != (Object)null))
			{
				GrabRenderers(child, renderers);
			}
		}
	}

	private void UpdateColors()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (once)
		{
			currentColor = onceColor;
		}
		else if (flashing)
		{
			currentColor = Color.Lerp(flashingColorMin, flashingColorMax, 0.5f * Mathf.Sin(Time.realtimeSinceStartup * flashingFreq * ((float)Math.PI * 2f)) + 0.5f);
		}
		else if (transitionValue > 0f)
		{
			currentColor = constantColor;
			currentColor.a *= transitionValue;
		}
		else
		{
			if (!occluder)
			{
				return;
			}
			currentColor = occluderColor;
		}
		opaqueMaterial.SetColor(shaderPropertyID._Color, currentColor);
		for (int i = 0; i < highlightableRenderers.Count; i++)
		{
			highlightableRenderers[i].SetColorForTransparent(currentColor);
		}
	}

	private void UpdateTransition()
	{
		if (transitionValue != transitionTarget)
		{
			if (transitionTime <= 0f)
			{
				transitionValue = transitionTarget;
				return;
			}
			float num = ((transitionTarget > 0f) ? 1f : (-1f));
			transitionValue = Mathf.Clamp01(transitionValue + num * Time.unscaledDeltaTime / transitionTime);
		}
	}

	private void FillBufferInternal(CommandBuffer buffer, Mode m)
	{
		UpdateRenderers();
		mode = Mode.None;
		if (once || flashing || transitionValue > 0f)
		{
			mode = ((!seeThrough) ? Mode.Highlighter : Mode.HighlighterSeeThrough);
		}
		else if (occluder && seeThrough)
		{
			mode = Mode.OccluderSeeThrough;
		}
		if (mode == Mode.None || mode != m)
		{
			return;
		}
		if (cachedSeeThrough != seeThrough)
		{
			cachedSeeThrough = seeThrough;
			if (cachedSeeThrough)
			{
				_opaqueMaterial.EnableKeyword("SEE_THROUGH");
			}
			else
			{
				_opaqueMaterial.DisableKeyword("SEE_THROUGH");
			}
			for (int i = 0; i < highlightableRenderers.Count; i++)
			{
				highlightableRenderers[i].SetSeeThroughForTransparent(seeThrough);
			}
		}
		UpdateColors();
		for (int num = highlightableRenderers.Count - 1; num >= 0; num--)
		{
			HighlighterRenderer highlighterRenderer = highlightableRenderers[num];
			if ((Object)(object)highlighterRenderer == (Object)null)
			{
				highlightableRenderers.RemoveAt(num);
			}
			else if (!highlighterRenderer.FillBuffer(buffer, forceRender))
			{
				highlightableRenderers.RemoveAt(num);
				Object.Destroy((Object)(object)highlighterRenderer);
			}
		}
	}

	public static void FillBuffer(CommandBuffer buffer)
	{
		for (int i = 0; i < renderingOrder.Length; i++)
		{
			Mode m = renderingOrder[i];
			HashSet<Highlighter>.Enumerator enumerator = highlighters.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.FillBufferInternal(buffer, m);
			}
		}
	}

	public Highlighter()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
	}
}
