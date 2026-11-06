using Beam;
using Beam.Utilities;
using UnityEngine;

public class IntroAtmo : MonoBehaviour
{
	[SerializeField]
	private Atmosphere _atmospherePrefab;

	[SerializeField]
	private float _normalizedTime;

	[SerializeField]
	private Light _sun;

	[SerializeField]
	private RenderTexture _rt;

	private Material _mat;

	private Texture3D _inscatter;

	private void Start()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		_rt = _atmospherePrefab.CreateBRDFTexture();
		_mat = _atmospherePrefab.CreateBRDFMaterial();
		_inscatter = Atmosphere.ConvertLookUpTable(_atmospherePrefab.Inscatter);
		Color color = _sun.color;
		Shader.SetGlobalColor("_SunColor", color.linear);
		Shader.SetGlobalFloat("_SunIntensity", _sun.intensity);
		Shader.SetGlobalVector("_SunPosition", Vector4.op_Implicit(((Component)_sun).transform.position));
		Shader.SetGlobalVector("_SunDirection", Vector4.op_Implicit(((Component)_sun).transform.forward * -1f));
		_atmospherePrefab.SetMaterialUniforms(_mat);
		_mat.SetTexture("_Inscatter", (Texture)(object)_inscatter);
		_atmospherePrefab.ComputeAtmosphereLookupTexture(_mat, _normalizedTime);
		CetoSettings cetoSettings = Object.FindObjectOfType<CetoSettings>();
		cetoSettings.UpdateWaterVisibility(_normalizedTime);
		cetoSettings.UpdateWaterColor(_sun, _normalizedTime);
	}

	private void Update()
	{
		Shader.SetGlobalTexture("_SkyMap", (Texture)(object)_rt);
	}

	private void OnDestroy()
	{
		BeamTeam.Destroy<Material>(_mat);
		BeamTeam.Destroy<RenderTexture>(_rt);
		BeamTeam.Destroy<Texture3D>(_inscatter);
	}
}
