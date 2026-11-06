using Ceto;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public class AtmosphericFogController : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private float _originalHeight;

	private float _originalStartDistance;

	[SerializeField]
	private float _height;

	[SerializeField]
	private float _startDistance;

	private AtmosphericFogSettings _settings;

	public float OriginalHeight => _originalHeight;

	public float OriginalStartDistance => _originalStartDistance;

	public float Height
	{
		set
		{
			_height = value;
			if ((Object)(object)_settings != (Object)null)
			{
				((ParameterOverride<float>)(object)_settings.Height).Override(_height);
			}
		}
	}

	public float StartDistance
	{
		set
		{
			_startDistance = value;
			if ((Object)(object)_settings != (Object)null)
			{
				((ParameterOverride<float>)(object)_settings.StartDistance).Override(_startDistance);
			}
		}
	}

	private void OnEnable()
	{
		BeamPfxProfile profile = ((Component)_camera).GetComponent<BeamPfxLayer>().Profile;
		if ((Object)(object)profile == (Object)null || !profile.TryGetSettings<AtmosphericFogSettings>(ref _settings))
		{
			((Behaviour)this).enabled = false;
			return;
		}
		Height = _height;
		StartDistance = _startDistance;
		_originalHeight = _height;
		_originalStartDistance = _startDistance;
	}

	private void OnPreRender()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Ceto.Ocean.Instance == (Object)null))
		{
			float num = Ceto.Ocean.Instance.level - Ceto.Ocean.Instance.FindMaxDisplacement(inculdeOverlays: true) * 0.51f;
			bool flag = ((Component)_camera).transform.position.y < num;
			((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_settings).enabled).Override(!flag);
		}
	}
}
