using Beam;
using UnityEngine;

public class TreeManager : Singleton<TreeManager>
{
	public InteractiveObject_PALM_LOG[] TreeSegments;

	public InteractiveObject_PALM_LOG TreeTopSegement;

	[SerializeField]
	[HideInInspector]
	private Vector4 _wind = new Vector4(0.85f, 0.075f, 0.4f, 0.5f);

	[SerializeField]
	[HideInInspector]
	private float _windFrequency = 0.75f;

	[SerializeField]
	[HideInInspector]
	private float _waveSizeFoliage = 10f;

	[SerializeField]
	[HideInInspector]
	private float _waveSizeGrassDetail = 10f;

	[SerializeField]
	[HideInInspector]
	private float _windMultiplierGrassDetail = 1f;

	[SerializeField]
	[HideInInspector]
	private float _rainAmount;

	[SerializeField]
	[HideInInspector]
	private float _specPower = 1f;

	[SerializeField]
	[HideInInspector]
	private Color _vertexLitTranslucencyColor = new Color(0.73f, 0.85f, 0.4f, 1f);

	[SerializeField]
	[HideInInspector]
	private float _vertexLitTranslucencyViewDependency = 0.7f;

	[SerializeField]
	[HideInInspector]
	private float _vertexLitShadowStrength = 0.8f;

	[SerializeField]
	[HideInInspector]
	private float _vertexLitShininess = 0.2f;

	[SerializeField]
	[HideInInspector]
	private float _detailDistanceForGrassShader = 80f;

	public Vector4 Wind
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _wind;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_wind = value;
		}
	}

	public float WindFrequency
	{
		get
		{
			return _windFrequency;
		}
		set
		{
			_windFrequency = value;
		}
	}

	public float WaveSizeFoliage
	{
		get
		{
			return _waveSizeFoliage;
		}
		set
		{
			_waveSizeFoliage = value;
		}
	}

	public float WaveSizeGrassDetail
	{
		get
		{
			return _waveSizeGrassDetail;
		}
		set
		{
			_waveSizeGrassDetail = value;
		}
	}

	public float WindMultiplierGrassDetail
	{
		get
		{
			return _windMultiplierGrassDetail;
		}
		set
		{
			_windMultiplierGrassDetail = value;
		}
	}

	public float RainAmount
	{
		get
		{
			return _rainAmount;
		}
		set
		{
			_rainAmount = value;
		}
	}

	public float SpecPower
	{
		get
		{
			return _specPower;
		}
		set
		{
			_specPower = value;
		}
	}

	public Color VertexLitTranslucencyColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _vertexLitTranslucencyColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_vertexLitTranslucencyColor = value;
		}
	}

	public float VertexLitTranslucencyViewDependency
	{
		get
		{
			return _vertexLitTranslucencyViewDependency;
		}
		set
		{
			_vertexLitTranslucencyViewDependency = value;
		}
	}

	public float VertexLitShadowStrength
	{
		get
		{
			return _vertexLitShadowStrength;
		}
		set
		{
			_vertexLitShadowStrength = value;
		}
	}

	public float VertexLitShininess
	{
		get
		{
			return _vertexLitShininess;
		}
		set
		{
			_vertexLitShininess = value;
		}
	}

	public float VetailDistanceForGrassShader
	{
		get
		{
			return _detailDistanceForGrassShader;
		}
		set
		{
			_detailDistanceForGrassShader = value;
		}
	}

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_Wind", _wind);
		Shader.SetGlobalFloat("_AfsWaveSize", _waveSizeFoliage);
		float num = (_wind.x + _wind.z) * 0.5f;
		Shader.SetGlobalVector("_AfsWaveAndDistance", new Vector4(Time.time * (_windFrequency * 0.05f), 0.5f / _waveSizeGrassDetail, num * _windMultiplierGrassDetail, (_detailDistanceForGrassShader + 10f) * (_detailDistanceForGrassShader + 10f)));
		Shader.SetGlobalFloat("_AfsRainamount", _rainAmount);
		Shader.SetGlobalFloat("_AfsSpecPower", _specPower);
		Shader.SetGlobalFloat("_AfsShininess", Mathf.Clamp(_vertexLitShininess, 0.03f, 1f));
		Shader.SetGlobalColor("_AfsTranslucencyColor", _vertexLitTranslucencyColor);
		Shader.SetGlobalFloat("_AfsTranslucencyViewDependency", _vertexLitTranslucencyViewDependency);
		Shader.SetGlobalFloat("_AfsShadowStrength", _vertexLitShadowStrength);
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector4 wind = _wind;
		wind.x *= (1.25f + Mathf.Sin(Time.time * _windFrequency) * Mathf.Sin(Time.time * 0.375f)) * 0.5f;
		wind.z *= (1.25f + Mathf.Sin(Time.time * _windFrequency) * Mathf.Sin(Time.time * 0.193f)) * 0.5f;
		wind.w = _wind.w;
		float num = (wind.x + wind.z) * 0.5f;
		Shader.SetGlobalVector("_Wind", wind);
		Shader.SetGlobalFloat("_AfsWaveSize", 0.5f / _waveSizeFoliage);
		Shader.SetGlobalVector("_AfsWaveAndDistance", new Vector4(Time.time * (_windFrequency * 0.05f), 0.5f / _waveSizeGrassDetail, num * _windMultiplierGrassDetail, (_detailDistanceForGrassShader + 10f) * (_detailDistanceForGrassShader + 10f)));
	}

	public TreeManager()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
	}
}
