using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[ExecuteInEditMode]
public class SunshaftsController : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private SunshaftsSettings _sunEffect;

	private Sunshafts2Settings _moonEffect;

	private void OnEnable()
	{
		BeamPfxProfile profile = ((Component)_camera).GetComponent<BeamPfxLayer>().Profile;
		if (!((Object)(object)profile == (Object)null) && profile.TryGetSettings<SunshaftsSettings>(ref _sunEffect) && profile.TryGetSettings<Sunshafts2Settings>(ref _moonEffect) && (Object)(object)Singleton<Atmosphere>.Instance != (Object)null)
		{
			Singleton<Atmosphere>.Instance.SunsetEvent += Atmosphere_SunsetEvent;
			Atmosphere_SunsetEvent();
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)Singleton<Atmosphere>.Instance != (Object)null)
		{
			Singleton<Atmosphere>.Instance.SunsetEvent -= Atmosphere_SunsetEvent;
		}
	}

	private void Atmosphere_SunsetEvent()
	{
		bool dayTime = Singleton<Atmosphere>.Instance.DayTime;
		((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_sunEffect).enabled).Override(dayTime);
		((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_moonEffect).enabled).Override(!dayTime);
	}
}
